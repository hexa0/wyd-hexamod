using HexaMod.Scripts.Character.Controller.Character;
using UnityEngine;

namespace HexaMod.API.Util.WhosYourDaddy
{
	public static class PlayerControllers
	{
		internal static Transform parent;
		// TODO: make a static list of different transforms and have this parent just be the main transform so that custom logic that needs to reparent players to another object is supported

		public static HexaPlayerController[] GetPlayers()
		{
			int children = parent.childCount;

			HexaPlayerController[] players = new HexaPlayerController[children];

			for (int i = 0; i < children; i++)
			{
				players[i] = parent.GetChild(i).GetComponent<HexaPlayerController>();
			}

			return players;
		}

		public static HexaPlayerController LocalPlayer => HexaGlobal.networkManager.playerObj?.GetComponent<HexaPlayerController>();
		public static HexaPlayerController HostPlayer => GetPlayers()[0];


		public static Transform[] GetPlayerTransforms()
		{
			int children = parent.childCount;

			Transform[] playerTransforms = new Transform[children];

			for (int i = 0; i < children; i++)
			{
				playerTransforms[i] = parent.GetChild(i);
			}

			return playerTransforms;
		}

		public static HexaPlayerController GetPlayer(string name)
		{
			return parent.Find(name)?.GetComponent<HexaPlayerController>();
		}
	}
}
