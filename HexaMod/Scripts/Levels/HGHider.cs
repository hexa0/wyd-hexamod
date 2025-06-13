using HexaMod.Util;
using UnityEngine;

namespace HexaMod.Scripts
{
	public class HGHider : MonoBehaviour
	{
		public bool hgOnly = true;

		void Start()
		{
			if (PhotonNetwork.inRoom)
			{
				if (HexaMod.networkManager.curGameMode != GameModes.GetId("hungryGames") && hgOnly)
				{
					Destroy(gameObject);
				}
				else if (HexaMod.networkManager.curGameMode == GameModes.GetId("hungryGames") && !hgOnly)
				{
					Destroy(gameObject);
				}
			}
		}
	}
}
