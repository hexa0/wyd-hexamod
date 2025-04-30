using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using HexaVoiceChatShared.MessageProtocol;
using HexaVoiceChatShared.Net;
using NAudio.Wave;
using UnityEngine;
using static HexaVoiceChatShared.HexaVoiceChat;

namespace HexaMod.Voice
{
    internal static class VoiceChat
    {
        static int FreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static bool testMode = false;
        public static Dictionary<ulong, List<short[]>> audioBuffers = new Dictionary<ulong, List<short[]>>();
        public static Dictionary<ulong, bool> speakingStates = new Dictionary<ulong, bool>();
        public static VoiceChatClient voicechatTranscodeClient;
        private static int transcodeServerPort = 0;
        public static Process internalTranscodeServerProcess;

        public static int micSampleRate = 48000;
        public static int micBufferMillis = 20;
        public static int micChannels = 1;
        public static int micBits = 16;
        public static int opusBitrate = 8192;
        public static int opusSegmentFrames = 960;


        internal static int underrunPreventionSize = 3;
        internal static bool listening = false;
        internal static WaveInEvent waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(    
                rate: micSampleRate,
                bits: micBits,
                channels: micChannels
            ),
            BufferMilliseconds = micBufferMillis,
            NumberOfBuffers = 1
        };
        public static void StartListening()
        {
            if (!microphoneInitialized)
            {
                InitMicrophone();
            }

            Mod.Print("Mic Activated");
            if (!listening)
            {
                listening = true;
                waveIn.StartRecording();
            }
        }
        public static void StopListening()
        {
            Mod.Print("Mic Deactivated");
            if (listening)
            {
                listening = false;
                waveIn.StopRecording();
            }
        }
        private static bool microphoneInitialized = false;
        public static void InitMicrophone()
        {
            Mod.Print("Mic Initialized");
            microphoneInitialized = true;
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.RecordingStopped += delegate (object sender, StoppedEventArgs e)
            {
                if (listening)
                {
                    Mod.Warn("mic dropout, attempt to reconnect");
                    try
                    {
                        waveIn.StopRecording();
                    }
                    catch { }
                    try
                    {
                        waveIn.StartRecording();
                    }
                    catch { }
                }
            };

            StartListening();
        }

        public static void InitTranscodeServerProcess()
        {
            Mod.Print("Start internalTranscodeServerProcess");

            transcodeServerPort = FreePort();

            internalTranscodeServerProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "voice/VoiceChatHost.exe"),
                    Arguments = $"t 127.0.0.1 {transcodeServerPort}", // 39200
                    UseShellExecute = false
                }
            };

            internalTranscodeServerProcess.Start();
        }

        public static bool completedHandshake = false;
        static void OnHandshake(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            if (!completedHandshake)
            {
                Mod.Print("Completed Handshake");
                completedHandshake = true;
                new Thread(new ThreadStart(KeepTranscodeAliveThread)).Start();
            }
            else
            {
                Mod.Warn("Already Completed Handshake");
            }
        }

        public static void CreateTranscodeServerConnection()
        {
            Mod.Print("Create voicechatTranscodeClient");

            voicechatTranscodeClient = new VoiceChatClient(new IPEndPoint(
                IPAddress.Parse("127.0.0.1"),
                transcodeServerPort
            ));
        }

        public static void InitTranscodeServerConnection()
        {
            Mod.Print("Start voicechatTranscodeClient");

            voicechatTranscodeClient.endPoint = null;
            voicechatTranscodeClient.SwitchToEndPoint(new IPEndPoint(
                IPAddress.Parse("127.0.0.1"),
                transcodeServerPort
            ));

            Mod.Print("On Message");

            voicechatTranscodeClient.OnMessage(Protocol.VoiceChatMessageType.PCMData, OnPCMData);
            voicechatTranscodeClient.OnMessage(Protocol.VoiceChatMessageType.SpeakingStateUpdated, OnSpeakingState);
            voicechatTranscodeClient.OnMessage(Protocol.VoiceChatMessageType.Handshake, OnHandshake);
        }

        public static void SendTranscodeServerHandshake()
        {
            Mod.Print("Attempt Handshake");
            voicechatTranscodeClient.SendMessage(VoiceChatMessage.BuildMessage(Protocol.VoiceChatMessageType.Handshake, new byte[1]));
        }

        public static void InitUnityForVoiceChat()
        {
            // force stereo output and lower audio latency to make it suitable for voice
            // we also force a standard sample rate of 48000 to avoid having to manually resample mic audio buffers

            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();

            audioConfiguration.dspBufferSize = 512;
            audioConfiguration.sampleRate = 48000;
            audioConfiguration.speakerMode = AudioSpeakerMode.Stereo;

            AudioSettings.Reset(audioConfiguration);
        }

        static void OnPCMData(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

            short[] pcm = new short[Mathf.CeilToInt(clientMessage.body.Length / 2f)];
            Buffer.BlockCopy(clientMessage.body, 0, pcm, 0, clientMessage.body.Length);

            if (!audioBuffers.ContainsKey(clientMessage.clientId))
            {
                audioBuffers.Add(clientMessage.clientId, new List<short[]>());
            }

            List<short[]> buffer = audioBuffers[clientMessage.clientId];
            buffer.Add(pcm);

            if (buffer.Count > underrunPreventionSize)
            {
                buffer.RemoveAt(0);
                // Mod.Warn("too many buffers exist? did you forget to consume the audio data?");
            }
        }

        static void OnSpeakingState(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
            speakingStates[clientMessage.clientId] = clientMessage.body[0] == 1;

            if (clientMessage.body[0] == 1 && audioBuffers.ContainsKey(clientMessage.clientId))
            {
                audioBuffers[clientMessage.clientId].Clear();
            }
        }

        static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (room != null)
                {
                    voicechatTranscodeClient.SendMessage(
                        VoiceChatMessage.BuildMessage(
                            Protocol.VoiceChatMessageType.PCMData,
                            e.Buffer
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                Mod.Warn(ex);
            }
        }

        public static void SetRelay(string ip)
        {
            string oldRoom = room;

            if (room != null)
            {
                LeaveVoiceRoom();
            }

            voicechatTranscodeClient.SendMessage(VoiceChatMessage.BuildMessage(
                Protocol.VoiceChatMessageType.SwitchRelay,
                Encoding.ASCII.GetBytes(ip)
            ));

            if (oldRoom != null)
            {
                JoinVoiceRoom(oldRoom);
            }
        }

        public static string room = null;

        public static void JoinVoiceRoom(string roomName)
        {
            if (room != roomName)
            {
                voicechatTranscodeClient.SendMessage(ClientWrappedMessage.BuildMessage(
                    testMode ? 0 : (ulong)PhotonNetwork.player.ID,
                    Protocol.VoiceChatMessageType.VoiceRoomJoin,
                    Encoding.ASCII.GetBytes(roomName)
                ));
            }

            room = roomName;
        }

        public static void LeaveVoiceRoom()
        {
            voicechatTranscodeClient.SendMessage(VoiceChatMessage.BuildMessage(
                Protocol.VoiceChatMessageType.VoiceRoomLeave,
                new byte[1]
            ));

            room = null;
            speakingStates.Clear();
            audioBuffers.Clear();
        }

        public static void KeepTranscodeAliveThread()
        {
            while (true)
            {
                try
                {
                    voicechatTranscodeClient.SendMessage(
                        VoiceChatMessage.BuildMessage(
                            Protocol.VoiceChatMessageType.KeepTranscodeAlive,
                            new byte[1]
                        )
                    );
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