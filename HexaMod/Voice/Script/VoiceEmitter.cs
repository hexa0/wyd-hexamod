using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaMod.Voice
{
    public class VoiceEmitter : MonoBehaviour
    {
        public GameObject speakingObject;
        public AudioSource audioSource;
        public ulong clientId = 0;
        private List<short[]> buffers;
        private short[] lastBuffer = new short[512];
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.dopplerLevel = 0f;
        }

        private short[] NextBuffer()
        {
            short[] buffer = buffers.ElementAtOrDefault(0);
            if (buffer == null)
            {
                buffer = lastBuffer;

                bool speaking = false;

                if (VoiceChat.speakingStates.ContainsKey(clientId))
                {
                    if (VoiceChat.speakingStates[clientId])
                    {
                        speaking = true;
                    }
                }

                if (speaking)
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (short)(buffer[i] * 0.7f); // haha discord funni
                    }
                }
                else
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = 0;
                    }
                }
            }
            else
            {
                lastBuffer = buffer;
                buffers.RemoveAt(0);
            }

            return buffer;
        }

        private int digestPosition = 0;
        private short[] buffer;
        private static void CopyData(short[] source, int sourceIndex, short[] destination, int destinationIndex, int length)
        {
            Buffer.BlockCopy(source, (sourceIndex * 2), destination, destinationIndex * 2, length * 2);
        }
        private short[] NextChunk(int chunkSize)
        {
            short[] slice = new short[chunkSize];
            if (buffer == null)
            {
                // this really shouldn't happen but still does
                buffer = NextBuffer();
                if (buffer == null)
                {
                    return slice;
                }
            }

            if (digestPosition >= buffer.Length)
            {
                digestPosition = 0;
                buffer = NextBuffer();
                CopyData(buffer, digestPosition, slice, 0, chunkSize);
                digestPosition += chunkSize;
            }
            else if ((digestPosition + chunkSize) > buffer.Length)
            {
                int firstHalfReduction = (digestPosition + chunkSize) - buffer.Length;
                CopyData(
                    buffer, // source
                    digestPosition, // source start
                    slice, // destination
                    0, // write start
                    buffer.Length - digestPosition // length
                );
                buffer = NextBuffer();
                CopyData(
                    buffer, // source
                    0, // source start
                    slice, // destination
                    chunkSize - firstHalfReduction, // write start
                    firstHalfReduction // length
                );
                digestPosition = firstHalfReduction;
            }
            else
            {
                CopyData(buffer, digestPosition, slice, 0, chunkSize);
                digestPosition += chunkSize;
            }

            return slice;
        }

        private float volumeL = 0;
        private float volumeR = 0;

        private void Update()
        {
            if (speakingObject != null && VoiceChat.speakingStates.ContainsKey(clientId))
            {
                var speaking = VoiceChat.speakingStates[clientId];
                speakingObject.SetActive(speaking);
            }

            Transform cameraTransform = Camera.current.transform;
            Vector3 unitVector = (transform.position - cameraTransform.position).normalized;
            float dot = Vector3.Dot(unitVector, cameraTransform.right);
            float panAbs = Mathf.Abs(dot) * gameObject.GetComponent<AudioSource>().spatialBlend;

            if (dot < 0)
            {
                volumeR = Mathf.Lerp(1, 0, panAbs);
                volumeL = Mathf.Lerp(1, 1, panAbs);
            }
            else
            {
                volumeL = Mathf.Lerp(1, 0, panAbs);
                volumeR = Mathf.Lerp(1, 1, panAbs);
            }

            if (gameObject.GetComponent<AudioSource>().spatialBlend > 0f)
            {
                volumeL *= 2f;
                volumeR *= 2f;
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            try
            {
                if (VoiceChat.audioBuffers.ContainsKey(clientId))
                {
                    buffers = VoiceChat.audioBuffers[clientId];

                    int sampleCount = data.Length / channels;
                    short[] monoInput = NextChunk(sampleCount);

                    for (int sample = 0; sample < sampleCount; sample++)
                    {
                        float currentSampleValue = monoInput[sample] / (float)short.MaxValue;

                        data[sample * 2] = currentSampleValue * volumeL;
                        data[(sample * 2) + 1] = currentSampleValue * volumeR;
                    }
                }
            }
            catch (Exception e)
            {
                Mod.Fatal(e);
            }
        }
    }
}
