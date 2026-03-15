using HexaMod.API.UI.Element.Control.TextButton;
using HexaMod.API.Util.Unity.Settings;

namespace HexaMod.API.UI.Menu.HostMenu
{
	public class PhotonRoomAccesibilityButton : WTextButton
	{
		public override void Update()
		{
			base.Update();

			if (PhotonNetwork.inRoom) {
				SetText(PhotonNetwork.room.IsOpen ? "Open:\nAnyone Can Join" : "Closed:\nNobody Can Join");
			}
			else {
				SetText("Unknown:\nUnknown");
			}

			SetInteractable(PhotonNetwork.isMasterClient);
		}
		
		public PhotonRoomAccesibilityButton() : base()
		{
			AddListener(() => {
				if (PhotonNetwork.isMasterClient) {
					PhotonNetwork.room.IsOpen = !PhotonNetwork.room.IsOpen;
					HexaModPreferences.defaultRoomAccesibilityIsOpen.Value = PhotonNetwork.room.IsOpen;
				}
			});
		}
	}
}
