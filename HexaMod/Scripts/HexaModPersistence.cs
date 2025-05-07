using UnityEngine;

namespace HexaMod
{
	public class HexaModPersistence : MonoBehaviour
	{
		void Awake()
		{
			gameObject.AddComponent<TabOutMuteBehavior>();
		}
	}
}
