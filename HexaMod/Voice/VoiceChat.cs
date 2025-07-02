using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using NAudio.Wave;
using UnityEngine;
using VoiceChatShared.Enums;
using VoiceChatShared.Net;
using VoiceChatShared.Net.PeerConnection;

namespace HexaMod.Voice
{
	internal static class VoiceChat
	{
		public class AudioBuffer
		{
			public short[] samples;
			public int channels = 1;
			public int sampleRate = 48000;
		}

		public static Dictionary<ulong, List<AudioBuffer>> audioBuffers = new Dictionary<ulong, List<AudioBuffer>>();
		public static Dictionary<ulong, bool> speakingStates = new Dictionary<ulong, bool>();
		public static PeerDuelProtocolConnection<HVCMessage> transcodeClient;
		private static int transcodeServerPort = 0;
		internal static ManualLogSource voiceChatProcessLog = BepInEx.Logging.Logger.CreateLogSource("VoiceChatHost");
		public static float shortMaxValueMul = 1f / short.MaxValue;

		internal static int underrunPreventionSize = 2;

		public static void Init()
		{
			InitUnityForVoiceChat();
			InitTranscode();
		}

		public static bool listening = false;

		public static void StartListening()
		{
			transcodeClient.udp.SendMessage(HVCMessage.SetListening, NetData.As(true));
			listening = true;
		}

		public static void StopListening()
		{
			transcodeClient.udp.SendMessage(HVCMessage.SetListening, NetData.As(false));
			listening = false;
		}

		public static void InitTranscodeServerProcess()
		{
			// technically we need a port open to both TCP and UDP but in practice the chances of this collision are practically zero and even if that did happen it would fail and then promptly retry with another port that works
			transcodeServerPort = TCP<HVCMessage>.GetOpenPort();

			string voiceChatHostDirectory = PathJoin.Join(Mod.LOCATION, "voice");
			string voiceChatHostExecutable = PathJoin.Join(voiceChatHostDirectory, "VoiceChatHost.exe");

			NativeProcess.StartProcess(
				voiceChatHostExecutable,
				NativeProcess.BuildArguments(
					Environment.GetCommandLineArgs().Contains("VoiceChatHostVerboseLogging") ? "tv" : "t",
					"127.0.0.1",
					transcodeServerPort
				),
				voiceChatHostDirectory,
				standard =>
				{
					foreach (string line in standard.Substring(0, standard.Length - 1).Split(new[] { '\r', '\n' }))
					{
						if (line.Length == 0) continue;
						voiceChatProcessLog.LogInfo(line);
					}
				},
				error =>
				{
					foreach (string line in error.Substring(0, error.Length - 1).Split(new[] { '\r', '\n' }))
					{
						if (line.Length == 0) continue;
						voiceChatProcessLog.LogFatal(line);
					}
				}
			);
		}

		public static bool transcodeReady = false;
		static void OnHandshake(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			if (!transcodeReady)
			{
				transcodeReady = true;
				ConnectedToTranscodeServer();
			}
			else
			{
				Mod.Fatal("Receive Handshake: Already Completed Handshake");
			}
		}

		static void ConnectedToTranscodeServer()
		{
			SetDenoiseEnabled(denoisingEnabled.Value);
			SetMicrophoneDeviceId(microphoneDeviceId.Value);
			SetMicrophoneBufferMillis(microphoneBufferMillis.Value);
			SetMicrophoneBitrate(microphoneBitrate.Value);

			if (room != null && relayIp != null)
			{
				string oldRelay = relayIp;
				string oldRoom = room;
				DisconnectFromRelay();
				ConnectToRelay(oldRelay, oldRoom);
			}
		}

		public static void InitTranscodeServerConnection()
		{
			if (transcodeClient != null)
			{
				transcodeClient.Close();
				transcodeClient = null;
			}

			transcodeClient = new PeerDuelProtocolConnection<HVCMessage>(new IPEndPoint(
				IPAddress.Parse("127.0.0.1"),
				transcodeServerPort
			));

			transcodeClient.Connect();

			transcodeClient.OnMessage(HVCMessage.PCMData, OnPCMData);
			transcodeClient.OnMessage(HVCMessage.SpeakingStateUpdated, OnSpeakingState);
			transcodeClient.OnMessage(HVCMessage.VoiceRoomPeerIdAllocated, (message, peer) =>
			{
				ulong allocatedId = BitConverter.ToUInt64(message.Body, 0);
				Mod.Print($"We've been assigned to peer ID {allocatedId} by the relay server");

				Hashtable hash = PhotonNetwork.player.CustomProperties;

				if (hash.ContainsKey("VoicePeerId"))
				{
					hash.Remove("VoicePeerId");
				}

				hash.Add("VoicePeerId", message.Body);

				PhotonNetwork.player.SetCustomProperties(hash);
			});
			transcodeClient.OnDisconnect(peer =>
			{
				if (transcodeReady)
				{
					Mod.Fatal($"transcodeClient has unexpectedly disconnected");
					transcodeReady = false;
					InitTranscode();
				}
			});
		}

		public static void SendTranscodeServerHandshake()
		{
			transcodeClient.tcp.Once(HVCMessage.Handshake, OnHandshake);
			transcodeClient.tcp.SendEventMessage(HVCMessage.Handshake);
		}

		public static void InitUnityForVoiceChat()
		{
			// this causes a hard crash if we call it while the scene is waiting to activate due to a race condition
			// force stereo output and lower audio latency to make it suitable for voice
			// we also force a standard sample rate of 48000 to avoid having to manually resample mic audio buffers

			AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();

			audioConfiguration.dspBufferSize = 512;
			audioConfiguration.sampleRate = 48000;
			audioConfiguration.speakerMode = AudioSpeakerMode.Stereo;

			AudioSettings.Reset(audioConfiguration);
		}

		public static readonly ModPreference<bool> debugOverlayEnabled = new ModPreference<bool>("VoiceChatDebugOverlay", false);
		public static readonly ModPreference<int> microphoneDeviceId = new ModPreference<int>("MicrophoneDevice", 0).LinkTo(SetMicrophoneDeviceId);
		public static readonly ModPreference<byte> microphoneBufferMillis = new ModPreference<byte>("MicrophoneBufferMillis", 20).LinkTo(SetMicrophoneBufferMillis);
		public static readonly ModPreference<int> microphoneBitrate = new ModPreference<int>("MicrophoneBitrate", 2).LinkTo(SetMicrophoneBitrate);
		public static readonly ModPreference<bool> denoisingEnabled = new ModPreference<bool>("UseRnNoise", false).LinkTo(SetDenoiseEnabled);

		public static float currentPeak = 0f;

		static void OnPCMData(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			int sampleRate = BitConverter.ToInt32(message.Body, 0);
			int channels = BitConverter.ToInt32(message.Body, 4);
			short[] pcm = new short[Mathf.CeilToInt((message.Body.Length - 8) / 2f)];
			Buffer.BlockCopy(message.Body, 8, pcm, 0, message.Body.Length - 8);

			if (!audioBuffers.ContainsKey(message.Client))
			{
				audioBuffers.Add(message.Client, new List<AudioBuffer>());
			}

			List<AudioBuffer> buffers = audioBuffers[message.Client];

			lock (buffers)
			{
				buffers.Add(new AudioBuffer()
				{
					samples = pcm,
					sampleRate = sampleRate,
					channels = channels
				});

				if (buffers.Count > underrunPreventionSize)
				{
					buffers.RemoveAt(0);
					// Mod.Warn("too many buffers exist? did you forget to consume the audio data?");
				}
			}
		}

		static void OnSpeakingState(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			speakingStates[message.Client] = message.Body[0] == 1;
		}

		public static string relayIp = null;

		public static void DisconnectFromRelay()
		{
			transcodeClient.udp.SendEventMessage(HVCMessage.DisconnectFromRelay);
			relayIp = null;
			room = null;
		}

		public static void ConnectToRelay(string ip, string reconnectRoom)
		{
			if (relayIp != ip)
			{
				if (room != null)
				{
					LeaveVoiceRoom();
				}

				if (relayIp != null)
				{
					DisconnectFromRelay();
				}

				transcodeClient.udp.SendMessage(
					HVCMessage.ConnectToRelay,
					Encoding.ASCII.GetBytes(ip)
				);

				relayIp = ip;

				if (reconnectRoom != null)
				{
					JoinVoiceRoom(reconnectRoom);
				}
			}
		}

		public static void ConnectToRelay(string ip)
		{
			ConnectToRelay(ip, room);
		}

		public static string room = null;

		public static void JoinVoiceRoom(string roomName)
		{
			if (room != roomName)
			{
				room = roomName;

				transcodeClient.udp.SendMessage(
					HVCMessage.VoiceRoomJoin,
					Encoding.ASCII.GetBytes(roomName)
				);

				StartListening();
			}
		}

		static void SetDenoiseEnabled(bool denoise)
		{
			if (transcodeReady)
			{
				transcodeClient.udp.SendMessage(HVCMessage.SetRNNoiseEnabled, new byte[] {
				(byte)(denoise ? 0x01 : 0x00)
			});
			}
		}

		public static void LeaveVoiceRoom()
		{
			room = null;
			speakingStates.Clear();
			audioBuffers.Clear();

			StopListening();
			DisconnectFromRelay();
		}

		public class MicrophoneDevice
		{
			public WaveInCapabilities capabilities;
			public int deviceId;
		}

		public static MicrophoneDevice[] GetDevices()
		{
			MicrophoneDevice[] devices = new MicrophoneDevice[WaveInEvent.DeviceCount];

			for (int i = 0; i < devices.Length; i++)
			{
				devices[i] = new MicrophoneDevice()
				{
					capabilities = WaveInEvent.GetCapabilities(i),
					deviceId = i
				};
			}

			return devices;
		}

		public static void SetMicrophoneDeviceId(int device)
		{
			if (transcodeReady)
			{
				transcodeClient.udp.SendMessage(HVCMessage.SetMicDeviceId, NetData.As(device));
			}
		}

		public static void SetMicrophoneBufferMillis(byte millis)
		{
			if (transcodeReady)
			{
				transcodeClient.udp.SendMessage(HVCMessage.SetMicBufferMillis, NetData.As(millis));
			}
		}

		public static void SetMicrophoneBitrate(int bitrate)
		{
			if (transcodeReady)
			{
				transcodeClient.udp.SendMessage(HVCMessage.SetBitrate, NetData.As((byte)Enum.GetValues(typeof(Bitrate)).GetValue(bitrate)));
			}
		}

		public static void SetMicrophoneChannels(byte channels)
		{
			if (transcodeReady)			{
				transcodeClient.udp.SendMessage(HVCMessage.SetMicChannels, NetData.As(channels));
			}

		}

		static void InitTranscode()
		{
			new Thread(new ThreadStart(TranscodeServerStartupThread)).Start();
		}

		public static void TranscodeServerStartupThread()
		{
			try
			{
				InitTranscodeServerProcess();
				InitTranscodeServerConnection();

				while (!transcodeClient.tcp.Connected)
				{
					Thread.Sleep(1);
				}

				SendTranscodeServerHandshake();
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				Mod.Fatal($"Failed to start transcode server due to a Win32Exception (code {e.NativeErrorCode}), retrying\n{e}");
				Thread.Sleep(1000);
				InitTranscode();
			}
			catch (Exception e)
			{
				Mod.Fatal("Failed to start transcode server, retrying\n", e);
				Thread.Sleep(100);
				InitTranscode();
			}
		}
	}
}