using HexaMod.ScriptableObjects;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Video;
using HarmonyLib;

namespace HexaMod.UI.Elements.Extended
{
	public class WYDMapButton : WYDTextButton
	{
		ModLevel level;

		RectTransform levelBackground;
		readonly Mask mask;
		readonly Graphic levelImage;
		readonly VideoPlayer levelVideo;

		bool? wasActive = null;

		public override void Update()
		{
			base.Update();

			bool active = Assets.loadedLevel != level;
			if (wasActive == null || wasActive != active)
			{
				wasActive = active;

				SetInteractable(Assets.loadedLevel != level)
					.SetTextAuto(!active ? level.levelNameReadable + "*" : level.levelNameReadable);
			}
		}

		public override void Shown()
		{
			base.Shown();

			if (levelVideo)
			{
				levelVideo.Play();
			}
		}

		public override void Hidden()
		{
			base.Hidden();

			if (levelVideo)
			{
				levelVideo.Stop();
			}
		}

		public WYDMapButton(ModLevel level) : base()
		{
			this.level = level;

			levelBackground = new GameObject("levelBackground", typeof(RectTransform)).GetComponent<RectTransform>();
			levelBackground.SetParent(button.transform);
			levelBackground.anchoredPosition = Vector2.zero;
			levelBackground.sizeDelta = button.GetComponent<RectTransform>().sizeDelta;

			mask = gameObject.AddComponent<Mask>();
			SetSpriteColor(Color.white);

			if (level.levelVideo)
			{
				levelVideo = levelBackground.gameObject.AddComponent<VideoPlayer>();
				levelVideo.isLooping = true;
				levelVideo.clip = level.levelVideo;

				levelImage = levelBackground.gameObject.AddComponent<RawImage>();

				Traverse buttonFields = Traverse.Create(button);
				buttonFields.Field<Graphic>("m_TargetGraphic").Value = levelImage;

				levelVideo.targetTexture = new RenderTexture((int)levelBackground.sizeDelta.x, (int)levelBackground.sizeDelta.y, 16);
				levelVideo.aspectRatio = VideoAspectRatio.Stretch;
				(levelImage as RawImage).texture = levelVideo.targetTexture;

				for (ushort i = 0; i < levelVideo.audioTrackCount; i++)
				{
					levelVideo.SetDirectAudioVolume(i, 0);
				}
			}
			else
			{
				levelImage = levelBackground.gameObject.AddComponent<Image>();
				button.image = levelImage as Image;
				SetSprite(level.levelSprite)
					.SetSpriteColor(Color.white);
			}

			levelBackground.SetSiblingIndex(0);

			this.SetName("mapButton")
				.SetTextAuto(level.levelNameReadable)
				.SetColors(new ColorBlock()
				{
					normalColor = new Color(0.7f, 0.7f, 0.7f, 1f),
					pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.8f),
					highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f),
					disabledColor = new Color(1f, 1f, 1f, 1f)
				});
		}
	}
}
