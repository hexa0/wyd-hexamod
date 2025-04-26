using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using HexaVoiceChatShared.MessageProtocol;
using HexaVoiceChatShared.Net;
using NAudio.Wave;
using UnityEngine;
using static HexaVoiceChatShared.HexaVoiceChat;

namespace HexaMod
{
    internal static class AudioInput
    {
        public static bool testMode = true;
        public static short[] audioBuffer = null;
        public static List<short[]> audioBuffers = new List<short[]>();
        public static VoiceChatClient voicechatTranscodeClient = new VoiceChatClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports.transcode));
        public static Process internalTranscodeServerProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "voice/VoiceChatHost.exe"),
                Arguments = "t 127.0.0.1",
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
            listening = true;
            waveIn.StartRecording();
        }
        public static void StopListening()
        {
            Mod.Print("Mic Deactivated");
            listening = false;
            waveIn.StopRecording();
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
                    audioBuffers.Clear();
                    for (int i = 0; i < audioBuffer.Length; i++)
                    {
                        audioBuffer[i] = 0;
                    }
                    Mod.Warn("mic dropout, attempt to reconnect");
                    waveIn.StartRecording();
                }
            };

            internalTranscodeServerProcess.Start();
        }

        public static void InitUnityForVoiceChat()
        {
            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();

            audioConfiguration.dspBufferSize = 512;
            audioConfiguration.sampleRate = 48000;
            audioConfiguration.speakerMode = AudioSpeakerMode.Stereo;

            AudioSettings.Reset(audioConfiguration);
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

        static void OnMessage(DecodedVoiceChatMessage message)
        {
            if (message.type == Protocol.VoiceChatMessageType.PCMData)
            {
                short[] pcm = new short[Mathf.CeilToInt(message.body.Length / 2f)];
                Buffer.BlockCopy(message.body, 0, pcm, 0, message.body.Length);

                audioBuffer = pcm;
                audioBuffers.Add(pcm);

                if (audioBuffers.Count > underrunPreventionSize)
                {
                    audioBuffers.RemoveAt(0);
                    // Mod.Warn("too many buffers exist? did you forget to consume the audio data?");
                }
            }
        }
    }
}
