using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.60b2c17f-6fa9-424f-81f1-ee25473b108e")]
	public class SmokeDetectorFactory : MigratableMonoBehavior
	{
		void Start()
		{
			var smokeDetector = gameObject.AddComponent<SmokeDectector>();
			smokeDetector.lightOnMat = lightOnMat;
			smokeDetector.lightOffMat = lightOffMat;
			smokeDetector.rend = rend;
			smokeDetector.gameStateController = GameObject.Find("GameStateController");
			smokeDetector.beepSound = beepSound;
			smokeDetector.tag = "Use";

			Destroy(this);
		}

		public Material lightOnMat;
		public Material lightOffMat;
		public Renderer rend;
		public GameObject beepSound;
	}
}
