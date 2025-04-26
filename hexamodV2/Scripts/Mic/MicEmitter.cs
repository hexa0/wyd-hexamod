using System;
using System.Linq;
using UnityEngine;

namespace HexaMod
{
    [RequireComponent(typeof(AudioSource))]
    public class MicEmitter : MonoBehaviour
    {
        public AudioSource audioSource;
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.dopplerLevel = 0f;
            audioSource.Play();
            AudioInput.StartListening();
        }

        private void OnDestroy() => AudioInput.StopListening();

        short[] NextBuffer()
        {
            short[] buffer = AudioInput.audioBuffers.ElementAtOrDefault(0);
            if (buffer == null)
            {
                buffer = AudioInput.audioBuffer;
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (short)(buffer[i] * 0.7f); // haha discord funni
                }
            }
            else
            {
                AudioInput.audioBuffers.RemoveAt(0);
            }

            return buffer;
        }

        int digestPosition = 0;
        short[] buffer;
        static void CopyData(short[] source, int sourceIndex, short[] destination, int destinationIndex, int length)
        {
            Buffer.BlockCopy(source, (sourceIndex * 2), destination, destinationIndex * 2, length * 2);
        }
        short[] NextChunk(int chunkSize)
        {
            short[] slice = new short[chunkSize];
            int micBufferSize = AudioInput.audioBuffer.Length;

            if (buffer == null)
            {
                // this really shouldn't happen but still does
                buffer = NextBuffer();
                if (buffer == null)
                {
                    return slice;
                }
            }

            if (digestPosition >= micBufferSize)
            {
                digestPosition = 0;
                buffer = NextBuffer();
                CopyData(buffer, digestPosition, slice, 0, chunkSize);
                digestPosition += chunkSize;
            }
            else if ((digestPosition + chunkSize) > micBufferSize)
            {
                int firstHalfReduction = (digestPosition + chunkSize) - micBufferSize;
                CopyData(
                    buffer, // source
                    digestPosition, // source start
                    slice, // destination
                    0, // write start
                    micBufferSize - digestPosition // length
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

        void Update()
        {
            Transform cameraTransform = Camera.main.transform;
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
        }

        short lastEndByte = 0;

        void OnAudioFilterRead(float[] data, int channels)
        {
            int sampleCount = data.Length / channels;
            short[] monoInput = NextChunk(sampleCount);

            for (int sample = 0; sample < sampleCount; sample++)
            {
                float currentSampleValue = monoInput[sample] / (float)short.MaxValue;

                data[sample * 2] = currentSampleValue * volumeL;
                data[(sample * 2) + 1] = currentSampleValue * volumeR;
            }

            lastEndByte = monoInput[sampleCount / 2];
        }
    }
}
