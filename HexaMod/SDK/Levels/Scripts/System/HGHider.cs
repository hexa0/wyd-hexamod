using HexaMod.API.Util.Migration;
using HexaMod.API.Util.WhosYourDaddy;

namespace HexaMod.SDK.Levels.Scripts.System
{
	[UnityMigrationIdentifier("HexaMod.7e665105-d97b-46fb-8279-62bcc92a57bd")]
	public class HGHider : MigratableMonoBehavior
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
