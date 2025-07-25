using System;
using HexaMod.API.Util.Migration;
using UnityEngine;
using UnityEngine.Video;

namespace HexaMod.SDK.ScriptableObjects
{
	[CreateAssetMenu(fileName = "ModLevel.asset", menuName = "HexaMod/ModLevel")]
	[UnityMigrationIdentifier("HexaMod.757c9901-e06f-49e3-997e-e9b0205800c3")]
	public class ModLevel : MigratableScriptableObject
	{
		[field: Header("Level Metadata")]
		[field: Space(1)]
		public string levelNameReadable = "Modded Level";
		public string levelDescriptionReadable = "A modded level in Who's Your Daddy!";
		[field: Header("Supported Gamemodes")]
		[field: Space(1)]
		public bool regular = true;
		public bool familyGathering = true;
		public bool hungryGames = false;
		public bool dadlympics = false;
		public bool daddysNightmare = false;
		public string[] customGamemodes;
		[field: Space(1)]
		public Sprite levelSprite;
		public VideoClip levelVideo;
		[field: Space(1)]
		[field: Header("Level Data")]
		[field: Space(1)]
		public string levelPrefabPath;

		[NonSerialized]
		[HideInInspector]
		public GameObject levelPrefab;
		[NonSerialized]
		[HideInInspector]
		public AssetBundle levelBundle;

		public string LevelIdentifier
		{
			get { return levelPrefabPath; }
		}
	}
}
