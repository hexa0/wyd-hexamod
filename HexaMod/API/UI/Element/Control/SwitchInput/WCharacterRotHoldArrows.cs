using HarmonyLib;
using HexaMod.API.UI.Element.Control.SwitchInput;
using HexaMod.API.UI.Element.Utility;
using HexaMod.SDK.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace HexaMod.API.UI.Element.Control.SwitchInput
{
	public class WCharacterRotHoldArrows : WHoldArrows
	{
		public RotModel rotModel;

		public override void LeftArrowDown() {
			rotModel.enabled = true;
			rotModel.rotSpeed = Mathf.Abs(rotModel.rotSpeed);
		}

		public override void LeftArrowUp() {
			rotModel.enabled = false;
		}

		public override void RightArrowDown() {
			rotModel.enabled = true;
			rotModel.rotSpeed = -Mathf.Abs(rotModel.rotSpeed);
		}

		public override void RightArrowUp() {
			rotModel.enabled = false;
		}

		public override void Shown() {
			base.Shown();

			rotModel.rotSpeed = 250;
			rotModel.enabled = false;
		}
	}
}
