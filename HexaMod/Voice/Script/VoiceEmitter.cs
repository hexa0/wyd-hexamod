﻿using System;
using System.Collections.Generic;
using System.Linq;
using HexaMod.UI;
using HexaMod.UI.Element.VoiceChatUI.Debug;
using UnityEngine;

namespace HexaMod.Voice.Script
{
	public class VoiceEmitter : MonoBehaviour
	{
		public GameObject speakingObject;
		public AudioSource audioSource;
		public PhotonPlayer player;
		ulong ClientId {
			get
			{
				if (player == null)
				{
					Mod.Warn("VoiceEmitter: Player is null, returning 0 for ClientId.");
					return 0;
				}
				else
				{
					if (!player.CustomProperties.ContainsKey("VoicePeerId"))
					{
						return 0;
					}
					else
					{
						player.CustomProperties.TryGetValue("VoicePeerId", out object peerId);

						ulong allocatedId = BitConverter.ToUInt64(peerId as byte[], 0);

						return allocatedId;
					}
				}
			}
		}

		private List<VoiceChat.AudioBuffer> buffers;
		private VoiceChat.AudioBuffer lastBuffer = new VoiceChat.AudioBuffer()
		{
			samples = new short[1024],
			sampleRate = 48000,
			channels = 1,
		};
		private void Start()
		{
			audioSource = GetComponent<AudioSource>();
			audioSource.dopplerLevel = 0f;
		}

		bool isWrittenTo = false;

		private VoiceChat.AudioBuffer NextBuffer()
		{
			lock (buffers)
			{
				VoiceChat.AudioBuffer buffer = buffers.ElementAtOrDefault(0);
				if (buffer == null)
				{
					buffer = lastBuffer;
				}
				else
				{
					lastBuffer = buffer;
					buffers.RemoveAt(0);
				}

				return buffer;
			}
		}

		private readonly CircularShortBuffer circularBuffer = new CircularShortBuffer(48000);
		private static void CopyData(short[] source, int sourceIndex, short[] destination, int destinationIndex, int length)
		{
			Buffer.BlockCopy(source, sourceIndex * 2, destination, destinationIndex * 2, length * 2);
		}

		private short[] NextChunk(int chunkSize)
		{
			bool tooCloseToWriteHead = false;

			if (isWrittenTo && !circularBuffer.IsEnough(chunkSize))
			{
				isWrittenTo = false;

				if (!VoiceChat.speakingStates.ContainsKey(ClientId) || VoiceChat.speakingStates[ClientId])
				{
					tooCloseToWriteHead = true;
				}
				else
				{
					circularBuffer.Write(new short[circularBuffer.capacity]);
					circularBuffer.realReadHead = 0;
					circularBuffer.realWriteHead = 0;
				}
			}

			if (buffers != null)
			{
				lock (buffers)
				{
					foreach (VoiceChat.AudioBuffer buffer in buffers.ToArray())
					{
						isWrittenTo = true;
						circularBuffer.Write(buffer.samples);
						lastBuffer = buffer;
					}

					buffers.Clear();
				}
			}

			if (tooCloseToWriteHead)
			{
				circularBuffer.realReadHead = circularBuffer.realWriteHead - lastBuffer.samples.Length * VoiceChat.underrunPreventionSize;
			}

			short[] read = circularBuffer.Read(chunkSize);

			return read;
		}

		private float volumeL = 0;
		private float volumeR = 0;

		private void Update()
		{
			if (speakingObject != null && VoiceChat.speakingStates.ContainsKey(ClientId))
			{
				var speaking = VoiceChat.speakingStates[ClientId];
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

		ShortCircularBufferDebugView bufferDebug;

		void Awake()
		{
			bufferDebug = new ShortCircularBufferDebugView(circularBuffer);
			HexaMenus.voiceChatDebugOverlay.elementStack.AddChild(bufferDebug);
		}

		void OnDestroy()
		{
			HexaMenus.voiceChatDebugOverlay.elementStack.RemoveChild(bufferDebug);
			Destroy(bufferDebug.gameObject);
		}

		private void OnAudioFilterRead(float[] data, int outputChannels)
		{
			if (VoiceChat.audioBuffers.ContainsKey(ClientId))
			{
				buffers = VoiceChat.audioBuffers[ClientId];

				try
				{
					int neededSamples = data.Length / outputChannels;
					int channels = lastBuffer.channels;

					short[] input = NextChunk(neededSamples * channels);

					switch (channels)
					{
						default:
							for (int sample = 0; sample < neededSamples; sample++)
							{
								float currentSampleValue = input[sample] * VoiceChat.shortMaxValueMul;

								data[sample * 2] = currentSampleValue * volumeL;
								data[1 + sample * 2] = currentSampleValue * volumeR;
							}

							break;
						case 2:
							for (int sample = 0; sample < neededSamples; sample++)
							{
								float currentSampleValueL = input[sample * 2] * VoiceChat.shortMaxValueMul;
								float currentSampleValueR = input[1 + sample * 2] * VoiceChat.shortMaxValueMul;

								data[sample * 2] = currentSampleValueL * volumeL;
								data[1 + sample * 2] = currentSampleValueR * volumeR;
							}

							break;
					}
				}
				catch (Exception e)
				{
					Mod.Fatal(e);
				}
			}
		}
	}
}
