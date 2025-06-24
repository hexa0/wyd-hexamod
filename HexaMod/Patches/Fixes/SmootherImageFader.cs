using HarmonyLib;
using HexaMod.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches.Fixes
{
	internal class SmoothImageFader : MonoBehaviour
	{
		ImgFade originalFader;
		Image image;
		float alpha = 0f;

		void Awake()
		{
			originalFader = GetComponent<ImgFade>();
			image = GetComponent<Image>();
		}

		void LateUpdate()
		{
			if (originalFader.fadingOut)
			{
				alpha -= Time.deltaTime;

				if (alpha < 0f)
				{
					alpha = 0f;
				}
			}
			else
			{
				alpha += Time.deltaTime;

				if (alpha > 1f)
				{
					alpha = 1f;
				}
			}

			Color imageColor = image.color;
			imageColor.a = 1 - alpha;

			image.color = imageColor;
		}
	}
	[HarmonyPatch(typeof(ImgFade))]
	internal class SmootherImageFader
	{
		[HarmonyPatch("Main")]
		[HarmonyPostfix]
		static void Main(ref ImgFade __instance)
		{
			__instance.gameObject.AddComponent<SmoothImageFader>();
		}
	}
}
