using System.Collections.Generic;
using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	public static class GlobalPhotonFactory
	{
		private static readonly int startingNetId = 10000;
		private static readonly int startingAltNetId = 90000; // alt ids are for fixing issues with the base map without messing with mod ids
		private static int currentNetId = startingNetId;
		private static int currentAltNetId = startingAltNetId;

		public static void Reset()
		{
			currentNetId = startingNetId;
			currentAltNetId = startingAltNetId;
		}

		public static void Register(GameObject gameObject, bool alt = false)
		{
			var view = gameObject.GetComponent<PhotonView>() ?? gameObject.AddComponent<PhotonView>();

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

	[UnityMigrationIdentifier("HexaMod.3528d1b4-dca7-4e5e-bf46-fc99c5d99aab")]
	public class PhotonFactory : MigratableMonoBehavior
	{
		void Awake()
		{
			GlobalPhotonFactory.Register(gameObject);

			Destroy(this);
		}
	}
}