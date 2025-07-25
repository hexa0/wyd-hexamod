using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.e08c2f74-e456-44ba-b1ef-284d5ade30ee")]
	public class DestroyTimerFactory : MigratableMonoBehavior
	{
		void Start()
		{
			var timer = gameObject.AddComponent<DestroyTimer>();
			timer.timeToDestroy = timeToDestroy;
			timer.afterSound = afterSound;

			Destroy(this);
		}

		public float timeToDestroy;
		public GameObject afterSound;
	}
}
