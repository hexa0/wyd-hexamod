using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using HexaVoiceChatShared.MessageProtocol;
using HexaVoiceChatShared.Net;
using NAudio.Wave;
using UnityEngine;
using static HexaVoiceChatShared.HexaVoiceChat;

namespace HexaMod.Voice
{
    internal static class VoiceChat
    {
        public static bool testMode = false;
        public static Dictionary<ulong, List<short[]>> audioBuffers = new Dictionary<ulong, List<short[]>>();
        public static Dictionary<ulong, bool> speakingStates = new Dictionary<ulong, bool>();
        public static VoiceChatClient voicechatTranscodeClient;
        public static Process internalTranscodeServerProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "voice/VoiceChatHost.exe"),
                Arguments = "t 127.39.20.0", // 39200
                UseShellExecute = false
            }
        };

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
                    waveIn.StartRecording();
                }
            };

            internalTranscodeServerProcess.Start();
            voicechatTranscodeClient = new VoiceChatClient(new IPEndPoint(IPAddress.Parse("127.39.20.0"), Ports.transcode));
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

            InitMicrophone();

            voicechatTranscodeClient.OnMessage(Protocol.VoiceChatMessageType.PCMData, OnPCMData);
            voicechatTranscodeClient.OnMessage(Protocol.VoiceChatMessageType.SpeakingStateUpdated, OnSpeakingState);
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
        }

        static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                voicechatTranscodeClient.SendMessage(
                    VoiceChatMessage.BuildMessage(
                        Protocol.VoiceChatMessageType.PCMData,
                        e.Buffer
                    )
                );
            }
            catch (Exception ex)
            {
                Mod.Warn(ex);
            }
        }

        public static void SetRelay(string ip)
        {
            voicechatTranscodeClient.SendMessage(VoiceChatMessage.BuildMessage(
                Protocol.VoiceChatMessageType.SwitchRelay,
                Encoding.ASCII.GetBytes(ip)
            ));
        }

        public static string room = null;

        public static void JoinVoiceRoom(string roomName)
        {
            voicechatTranscodeClient.SendMessage(ClientWrappedMessage.BuildMessage(
                testMode ? 0 : (ulong)PhotonNetwork.player.ID,
                Protocol.VoiceChatMessageType.VoiceRoomJoin,
                Encoding.ASCII.GetBytes(roomName)
            ));

            room = roomName;

            StartListening();
        }

        public static void LeaveVoiceRoom()
        {
            StopListening();

            voicechatTranscodeClient.SendMessage(VoiceChatMessage.BuildMessage(
                Protocol.VoiceChatMessageType.VoiceRoomLeave,
                new byte[0]
            ));

            room = null;
        }
    }
}