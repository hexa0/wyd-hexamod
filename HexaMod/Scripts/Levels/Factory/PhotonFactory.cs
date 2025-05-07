using System.Collections.Generic;
using UnityEngine;

namespace HexaMapAssemblies
{
	public static class GlobalPhotonFactory
	{
		public static int startingNetId = 10000;
		private static int currentNetId = startingNetId;

		public static void Reset()
		{
			currentNetId = startingNetId;
		}

		public static void Register(GameObject gameObject)
		{
			var view = gameObject.AddComponent<PhotonView>();
			view.viewID = Next();
			view.ObservedComponents = new List<Component>(0);
		}

		public static int Next()
		{
			currentNetId += 1;
			return currentNetId - 1;
		}
	}

	public class PhotonFactory : MonoBehaviour
	{
		void Start()
		{
			GlobalPhotonFactory.Register(gameObject);
		}
	}
}