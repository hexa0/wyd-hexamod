using System;
using HexaMod.API.Util.Migration;
using HexaMod.SDK.Levels.Scripts.System;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Customization
{
	[Serializable]
	public struct LevelSongEntry
	{
		public string gamemode;
		public AudioClip value;
	}

	[ExecuteInEditMode]
	[UnityMigrationIdentifier("HexaMod.91b16547-1aa4-4223-b293-adfc70343a92")]
	public class LevelMusic : MigratableMonoBehavior
	{
		[SerializeField]
		public LevelSongEntry[] themes = new LevelSongEntry[0];

		[SerializeField]
		[HideInInspector]
		private string[] _serializedGamemodes = new string[0];
		[SerializeField]
		[HideInInspector]
		private AudioClip[] _serializedAudioClips = new AudioClip[0];

		public static readonly GamemodeSelector<AudioClip, LevelSongEntry> selector = new GamemodeSelector<AudioClip, LevelSongEntry>();

		void Awake()
		{
			if (!Application.isEditor)
			{
				selector.Deserialize(_serializedGamemodes, _serializedAudioClips, out themes);
			}
		}

		void OnValidate()
		{
			if (Application.isEditor)
			{
				selector.Serialize(out _serializedGamemodes, out _serializedAudioClips, themes);
			}
		}
	}
}
