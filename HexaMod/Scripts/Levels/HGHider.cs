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
				if (HexaGlobal.networkManager.curGameMode != GameModes.GetId("hungryGames") && hgOnly)
				{
					Destroy(gameObject);
				}
				else if (HexaGlobal.networkManager.curGameMode == GameModes.GetId("hungryGames") && !hgOnly)
				{
					Destroy(gameObject);
				}
			}
		}
	}
}
