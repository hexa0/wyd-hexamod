using System.Collections.Generic;
using UnityEngine;

namespace HexaMod.Scripts
{
	public class NetworkedSoundBehavior : MonoBehaviour
	{
		private List<AudioClip> clips = new List<AudioClip>();

		public void RegisterSound(AudioClip sound)
		{
			clips.Add(sound);
		}

		public void RegisterSounds(AudioClip[] sounds)
		{
			foreach (AudioClip sound in sounds)
			{
				RegisterSound(sound);
			}
		}

		public void Play(AudioClip sound)
		{
			GetComponent<AudioSource>().PlayOneShot(sound);

			GetComponent<PhotonView>().RPC("PlayNetworkedSound", PhotonTargets.Others, new object[]
			{
				(byte)clips.FindIndex(s => s == sound)
			});
		}

		public void Play(AudioClip sound, float volume)
		{
			GetComponent<AudioSource>().PlayOneShot(sound, volume);

			GetComponent<PhotonView>().RPC("PlayNetworkedSoundWithVolume", PhotonTargets.Others, new object[]
			{
				(byte)clips.FindIndex(s => s == sound), (byte)(volume * byte.MaxValue)
			});
		}

		private AudioClip GetSound(byte soundID)
		{
			if (soundID < clips.Count)
			{
				return clips[soundID];
			}
			else
			{
				throw new System.Exception($"Attempted to get networked sound clip with invalid ID of {soundID},\nthat id wasn't defined by the NetworkedSoundBehavior\nmake sure to define them when adding the behavior.");
			}
		}

		[PunRPC]
		void PlayNetworkedSound(byte soundID)
		{
			AudioClip sound = GetSound(soundID);
			GetComponent<AudioSource>().PlayOneShot(sound);
		}

		[PunRPC]
		void PlayNetworkedSoundWithVolume(byte soundID, byte volume)
		{
			AudioClip sound = GetSound(soundID);
			GetComponent<AudioSource>().PlayOneShot(sound, (float)volume / byte.MaxValue);
		}
	}
}
