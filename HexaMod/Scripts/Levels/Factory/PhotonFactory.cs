using System.Collections.Generic;
using UnityEngine;

namespace HexaMapAssemblies
{
	public static class GlobalPhotonFactory
	{
		private static int startingNetId = 10000;
		private static int currentNetId = startingNetId;

		public static void Reset()
		{
			currentNetId = startingNetId;
		}

		public static void Register(GameObject gameObject)
		{
			var view = gameObject.GetComponent<PhotonView>();

			if (view == null)
			{
				view = gameObject.AddComponent<PhotonView>();
			}

			view.viewID = Next();
			view.ObservedComponents = new List<Component>(0);
		}

		private static int Next()
		{
			int id = currentNetId;
			currentNetId += 1;
			return id;
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