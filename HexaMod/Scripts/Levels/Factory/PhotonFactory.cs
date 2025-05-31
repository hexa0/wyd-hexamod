using System.Collections.Generic;
using UnityEngine;

namespace HexaMapAssemblies
{
	public static class GlobalPhotonFactory
	{
		private static int startingNetId = 10000;
		private static int startingAltNetId = 90000; // alt ids are for fixing issues with the base map without messing with mod ids
		private static int currentNetId = startingNetId;
		private static int currentAltNetId = startingAltNetId;

		public static void Reset()
		{
			currentNetId = startingNetId;
			currentAltNetId = startingAltNetId;
		}

		public static void Register(GameObject gameObject, bool alt = false)
		{
			var view = gameObject.GetComponent<PhotonView>();

			if (view == null)
			{
				view = gameObject.AddComponent<PhotonView>();
			}

			if (alt)
			{
				view.viewID = NextAlt();
			}
			else
			{
				view.viewID = Next();
			}

			view.ObservedComponents = new List<Component>(0);
		}

		private static int NextAlt()
		{
			int id = currentAltNetId;
			currentAltNetId += 1;
			return id;
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