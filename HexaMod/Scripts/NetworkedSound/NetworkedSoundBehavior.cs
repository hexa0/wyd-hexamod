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

		[PunRPC]
		void PlayNetworkedSound(byte soundID)
		{
			if (soundID < clips.Count)
			{
				GetComponent<AudioSource>().PlayOneShot(clips[soundID]);
			}
			else
			{
				Mod.Warn($"Attempted to play networked sound clip with invalid ID of {soundID},\nthat id wasn't defined by the NetworkedSoundBehavior\nmake sure to define them when adding the behavior.");
			}
		}
	}
}
