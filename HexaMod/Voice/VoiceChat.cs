using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using HexaVoiceChatShared.MessageProtocol;
using HexaVoiceChatShared.Net;
using NAudio.Wave;
using UnityEngine;
using HexaVoiceChatShared;
using VoiceChatShared;

namespace HexaMod.Voice
{
	internal static class VoiceChat
	{
		static int FreePort()
		{
			if (Environment.GetCommandLineArgs().Contains("ForceConsistantTranscodePort"))
			{
				return HexaVoiceChat.Ports.transcode;
			}
			else
			{
				TcpListener l = new TcpListener(IPAddress.Loopback, 0);
				l.Start();
				int port = ((IPEndPoint)l.LocalEndpoint).Port;
				l.Stop();
				return port;
			}
		}

		public class AudioBuffer
		{
			public short[] samples;
			public int channels = 1;
			public int sampleRate = 48000;
		}

		public static bool testMode = false;
		public static Dictionary<ulong, List<AudioBuffer>> audioBuffers = new Dictionary<ulong, List<AudioBuffer>>();
		public static Dictionary<ulong, bool> speakingStates = new Dictionary<ulong, bool>();
		public static VoiceChatClient voicechatTranscodeClient;
		private static int transcodeServerPort = 0;
		public static Process internalTranscodeServerProcess;
		internal static ManualLogSource voiceChatProcessLog = BepInEx.Logging.Logger.CreateLogSource("VoiceChatHost");
		public static float shortMaxValueMul = 1f / short.MaxValue;

		internal static int underrunPreventionSize = 2;

		public static void Init()
		{
			InitWithoutTranscodeProcess();
			InitTranscodeServerProcess();
		}

		public static void InitWithoutTranscodeProcess()
		{
			InitUnityForVoiceChat();
		}

		public static bool listening = false;

		public static void StartListening()
		{
			voicechatTranscodeClient.SendMessage(HVCMessage.SetListening, UDP.AsData(true));
			listening = true;
		}

		public static void StopListening()
		{
			voicechatTranscodeClient.SendMessage(HVCMessage.SetListening, UDP.AsData(false));
			listening = false;
		}

		public static void InitTranscodeServerProcess()
		{
			Mod.Print("Start internalTranscodeServerProcess");

			transcodeServerPort = FreePort();

			internalTranscodeServerProcess = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = PathJoin.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "voice", "VoiceChatHost.exe"),
					Arguments = $"{(Environment.GetCommandLineArgs().Contains("VoiceChatHostVerboseLogging") ? "tv" : "t")} 127.0.0.1 {transcodeServerPort}", // 39200
					UseShellExecute = false,
					RedirectStandardOutput = true,
				}
			};

			internalTranscodeServerProcess.OutputDataReceived += new DataReceivedEventHandler((object sendingProcess, DataReceivedEventArgs outLine) =>
			{
				string output = outLine.Data.Substring(0, outLine.Data.Length - 1);
				foreach (string line in output.Split(new[] {'\r', '\n'}))
				{
					voiceChatProcessLog.LogInfo(outLine.Data.Substring(0, outLine.Data.Length - 1));
				}
			});

			internalTranscodeServerProcess.Start();
			internalTranscodeServerProcess.BeginOutputReadLine();
		}

		public static bool transcodeServerReady = false;
		static void OnHandshake(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			if (!transcodeServerReady)
			{
				Mod.Print($"Completed Handshake With Signature \"{Encoding.ASCII.GetString(message.body)}\" which should always match \"!\"");
				transcodeServerReady = true;
				new Thread(new ThreadStart(KeepTranscodeAliveThread)).Start();
				ConnectedToTranscodeServer();
			}
			else
			{
				Mod.Warn("Receive Handshake: Already Completed Handshake");
			}
		}

		static void ConnectedToTranscodeServer()
		{
			SetDenoiseEnabled(denoisingEnabled.Value);
			SetMicrophoneDeviceId(microphoneDeviceId.Value);
			SetMicrophoneBufferMillis(microphoneBufferMillis.Value);
			SetMicrophoneBitrate(microphoneBitrate.Value);
		}

		public static void InitTranscodeServerConnection()
		{
			Mod.Print("Start voicechatTranscodeClient");

			voicechatTranscodeClient = new VoiceChatClient(new IPEndPoint(
				IPAddress.Parse("127.0.0.1"),
				transcodeServerPort
			));

			voicechatTranscodeClient.Connect();

			Mod.Print("On Message");

			voicechatTranscodeClient.OnClientMessage(HVCMessage.PCMData, OnPCMData);
			voicechatTranscodeClient.OnMessage(HVCMessage.SpeakingStateUpdated, OnSpeakingState);
			voicechatTranscodeClient.OnMessage(HVCMessage.Handshake, OnHandshake);
		}

		public static void SendTranscodeServerHandshake()
		{
			Mod.Print("Attempt Handshake");
			voicechatTranscodeClient.SendMessage(HVCMessage.Handshake, new byte[1]);
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

		static void OnPCMData(DecodedClientWrappedMessage message, IPEndPoint from)
		{
			int sampleRate = BitConverter.ToInt32(message.body, 0);
			int channels = BitConverter.ToInt32(message.body, 4);
			short[] pcm = new short[Mathf.CeilToInt((message.body.Length - 8) / 2f)];
			Buffer.BlockCopy(message.body, 8, pcm, 0, message.body.Length - 8);

			if (!audioBuffers.ContainsKey(message.clientId))
			{
				audioBuffers.Add(message.clientId, new List<AudioBuffer>());
			}

			List<AudioBuffer> buffers = audioBuffers[message.clientId];

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

		static void OnSpeakingState(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
			speakingStates[clientMessage.clientId] = clientMessage.body[0] == 1;
		}

		public static string relayIp = null;

		public static void DisconnectFromRelay()
		{
			voicechatTranscodeClient.SendEventMessage(HVCMessage.DisconnectFromRelay);
			relayIp = null;
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

				voicechatTranscodeClient.SendMessage(
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

				Mod.Print($"JoinVoiceRoom {roomName}");

				voicechatTranscodeClient.SendClientWrappedMessage(
					testMode ? 0 : (ulong)PhotonNetwork.player.ID,
					HVCMessage.VoiceRoomJoin,
					Encoding.ASCII.GetBytes(roomName)
				);

				StartListening();
			}
		}

		static void SetDenoiseEnabled(bool denoise)
		{
			if (transcodeServerReady)
			{
				voicechatTranscodeClient.SendMessage(HVCMessage.SetRNNoiseEnabled, new byte[] {
				(byte)(denoise ? 0x01 : 0x00)
			});
			}
		}

		public static void LeaveVoiceRoom()
		{
			Mod.Print($"LeaveVoiceRoom {room}");
			voicechatTranscodeClient.SendEventMessage(HVCMessage.VoiceRoomLeave);

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
			if (transcodeServerReady)
			{
				voicechatTranscodeClient.SendMessage(HVCMessage.SetMicDeviceId, UDP.AsData(device));
			}
		}

		public static void SetMicrophoneBufferMillis(byte millis)
		{
			if (transcodeServerReady)
			{
				voicechatTranscodeClient.SendMessage(HVCMessage.SetMicBufferMillis, UDP.AsData(millis));
			}
		}

		public static void SetMicrophoneBitrate(int bitrate)
		{
			if (transcodeServerReady)
			{
				voicechatTranscodeClient.SendMessage(HVCMessage.SetBitrate, UDP.AsData((byte)Enum.GetValues(typeof(Bitrate)).GetValue(bitrate)));
			}
		}

		public static void SetMicrophoneChannels(byte channels)
		{
			if (transcodeServerReady)			{
				voicechatTranscodeClient.SendMessage(HVCMessage.SetMicChannels, UDP.AsData(channels));
			}

		}

		public static void KeepTranscodeAliveThread()
		{
			while (true)
			{
				if (internalTranscodeServerProcess.HasExited)
				{
					Mod.Warn("transcode server has exited, restarting it");

					internalTranscodeServerProcess = null;
					transcodeServerReady = false;

					InitTranscodeServerProcess();

					while (!transcodeServerReady)
					{
						Thread.Sleep(100);

						try
						{
							InitTranscodeServerConnection();
						}
						catch (Exception e)
						{
							Mod.Warn(e);
						}

						Thread.Sleep(100);

						try
						{
							if (!transcodeServerReady)
							{
								SendTranscodeServerHandshake();
							}
						}
						catch (Exception e)
						{
							Mod.Warn(e);
						}

						Thread.Sleep(100);
					}

					Mod.Warn("transcode has been re-initialized");
				}


				try
				{
					voicechatTranscodeClient.SendEventMessage(HVCMessage.KeepTranscodeAlive);
				}
				catch (Exception e)
				{
					Mod.Warn(e);
				}

				Thread.Sleep(500);
			}
		}
	}
}