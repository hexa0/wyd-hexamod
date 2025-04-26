using UnityEngine;
using UnityEngine.Video;

namespace HexaMod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ModLevel.asset", menuName = "HexaMod/ModLevel")]
    public class ModLevel : ScriptableObject
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
        [field: Space(1)]
        public Sprite levelSprite;
        public VideoClip levelVideo;
        [field: Space(1)]
        [field: Header("Level Data")]
        [field: Space(1)]
        public GameObject levelPrefab;
    }
}
