using UnityEngine;

namespace HexaMod.UI.Element
{
	public class WUIGlobals
	{
		public static WUIGlobals instance;
		public class Fonts
		{
			// the primary pixelated font used by all of WYD
			public readonly Font primary = HexaGlobal.coreBundle.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/FFFFORWA.ttf");
			// used by the microwave time display
			public readonly Font microwave = HexaGlobal.coreBundle.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/UpheavalPro.ttf");
			// a hand written note, fun fact! this goes unused as it's referenced by a disabled GameObject that was probably intended to hint at the various chores that you can complete
			public readonly Font handNote = HexaGlobal.coreBundle.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/AlanisHand.ttf");
			// used on the toybox
			public readonly Font toyBox = HexaGlobal.coreBundle.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/KatahdinRound.ttf");
			// used by the barebones debug console commands system in WYD (normally disabled but can be enabled by activating the Console behavior on the player, all it can do is spawn prefabs with the syntax of s000x001)
			public readonly Font console = HexaGlobal.coreBundle.LoadAsset<Font>("Assets/ModResources/Core/WYD/Font/pixelFJ8pt1.ttf");

			public enum Sizes : int
			{
				ButtonSmall = 28,
				ButtonRegular = 44,
				Title = 100,
				MenuError = 50
			}
		}

		public Fonts fonts = new Fonts();

		public WUIGlobals()
		{
			instance = this;
			fonts = new Fonts();
		}
	}
}