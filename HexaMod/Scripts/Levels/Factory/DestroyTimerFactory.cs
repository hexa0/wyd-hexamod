using UnityEngine;

namespace HexaMapAssemblies
{
	public class DestroyTimerFactory : MonoBehaviour
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
