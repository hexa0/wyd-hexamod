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
		}

		public float timeToDestroy;
		public GameObject afterSound;
	}
}
