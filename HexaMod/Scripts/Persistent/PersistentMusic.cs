using HexaMod.API.Util.Unity.Settings;
using HexaMod.SDK.Levels.Scripts.Customization;
using UnityEngine;

namespace HexaMod.Scripts.Persistent
{
	[RequireComponent(typeof(AudioSource))]
	public class PersistentMusic : MonoBehaviour
	{
		public static PersistentMusic instance;
		AudioSource source;

		public bool automaticallySelectTracks = true;

		void ChangeTrack(AudioClip track)
		{
			if (source.clip != track)
			{
				source.clip = track;

				if (track)
				{
					source.Play();
				}
				else
				{
					source.Stop();
				}
			}
		}

		void Awake()
		{
			instance = this;
			source = GetComponent<AudioSource>();
			source.loop = true;

			source.volume = WYDPreferences.musicVolume.Value;

			WYDPreferences.musicVolume.LinkTo(value =>
			{
				source.volume = value;
			});
		}

		void FixedUpdate()
		{
			if (automaticallySelectTracks)
			{
				LevelSongEntry[] themes;
					
				if (Assets.customLevelMusic)
				{
					themes = Assets.customLevelMusic.themes;
				}
				else
				{
					themes = Assets.StaticAssets.defaultLevelThemes;
				}

				if (themes.Length > 0)
				{
					ChangeTrack(LevelMusic.selector.Select(themes));
				}
				else
				{
					ChangeTrack(null);
				}
			}
		}
	}
}
