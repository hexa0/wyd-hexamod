using UnityEngine;

namespace HexaMod.API.UI.Element
{
	public class WUIGlobals
	{
		public static WUIGlobals instance;
		public class Resources
		{
			// the primary pixelated font used by all of WYD
			public readonly Font fontPrimary = HexaGlobal.coreBundle?.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/FFFFORWA.ttf");
			// used by the microwave time display
			public readonly Font fontMicrowave = HexaGlobal.coreBundle?.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/UpheavalPro.ttf");
			// a hand written note, fun fact! this goes unused as it's referenced by a disabled GameObject that was probably intended to hint at the various chores that you can complete
			public readonly Font fontHandNote = HexaGlobal.coreBundle?.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/AlanisHand.ttf");
			// used on the toybox
			public readonly Font fontToyBox = HexaGlobal.coreBundle?.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/KatahdinRound.ttf");
			// used by the barebones debug console commands system in WYD (normally disabled but can be enabled by activating the Console behavior on the player, all it can do is spawn prefabs with the syntax of s000x001)
			public readonly Font fontConsole = HexaGlobal.coreBundle?.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/pixelFJ8pt1.ttf");

			public enum FontSizes : int
			{
				ButtonSmall = 28,
				ButtonRegular = 44,
				Title = 100,
				MenuError = 50
			}

			public readonly Sprite spriteInputField128 = HexaGlobal.coreBundle?.LoadAsset<Sprite>("Assets/ModResources/Core/Sprite/InputField128.png");
			public readonly Sprite spriteButton = HexaGlobal.coreBundle?.LoadAsset<Sprite>("Assets/ModResources/Core/Sprite/Button.png");
		}

		public Resources resources = new Resources();

		public WUIGlobals()
		{
			instance = this;
			resources = new Resources();
		}
	}
}