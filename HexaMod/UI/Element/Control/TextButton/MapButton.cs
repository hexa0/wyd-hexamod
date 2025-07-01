using HexaMod.ScriptableObjects;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Video;
using HarmonyLib;
using HexaMod.UI.Element.Utility;
using UnityEngine.EventSystems;

namespace HexaMod.UI.Element.Control.TextButton
{
	public class MapButton : WTextButton
	{
		readonly ModLevel level;

		readonly LinearCanvasGroupFader levelVideoBackgroundFader;
		readonly RectTransform levelVideoBackground;
		readonly RectTransform levelBackground;
		readonly Mask mask;
		readonly Graphic levelImage;
		readonly Graphic levelVideoImage;
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


		public override void MouseEnter(PointerEventData eventData)
		{
			base.MouseEnter(eventData);
			levelVideoBackgroundFader.fadeState = true;

			if (level.levelVideo)
			{
				Traverse buttonFields = Traverse.Create(button);
				buttonFields.Field<Graphic>("m_TargetGraphic").Value = levelImage;

				levelVideo.playbackSpeed = 1f;
			}
			else
			{
				button.image = levelImage as Image;
			}
		}

		public override void MouseLeave(PointerEventData eventData)
		{
			base.MouseLeave(eventData);
			levelVideoBackgroundFader.fadeState = false;
			button.image = levelImage as Image;

			if (level.levelVideo)
			{
				levelVideo.playbackSpeed = 0f;
			}
		}

		public override void Shown()
		{
			base.Shown();

			if (level.levelVideo)
			{
				levelVideo.Play();
				levelVideo.playbackSpeed = 0f;
			}
		}

		public override void Hidden()
		{
			base.Shown();

			if (level.levelVideo)
			{
				levelVideo.Stop();
				levelVideo.playbackSpeed = 0f;
			}
		}

		public MapButton(ModLevel level) : base()
		{
			this.level = level;

			levelVideoBackgroundFader = new LinearCanvasGroupFader()
				.SetParent(rectTransform)
				.ScaleWithParent()
				.SetChildrenCullingEnabled(false)
				.SetInitialFadeState(false)
				.SetFadeSpeed(6f);

			levelBackground = new GameObject("levelBackground", typeof(RectTransform)).GetComponent<RectTransform>();
			levelBackground.SetParent(button.transform);
			levelBackground.GetComponent<RectTransform>().ScaleWithParent();

			levelVideoBackground = new GameObject("levelVideoBackground", typeof(RectTransform)).GetComponent<RectTransform>();
			levelVideoBackground.SetParent(levelVideoBackgroundFader.rectTransform);
			levelVideoBackground.GetComponent<RectTransform>().ScaleWithParent();

			label.transform.SetAsLastSibling();

			mask = gameObject.AddComponent<Mask>();

			levelImage = levelBackground.gameObject.AddComponent<Image>();
			button.image = levelImage as Image;
			SetSprite(level.levelSprite)
				.SetSpriteColor(Color.white);

			if (level.levelVideo)
			{
				levelVideo = levelVideoBackground.gameObject.AddComponent<VideoPlayer>();
				levelVideo.isLooping = true;
				levelVideo.clip = level.levelVideo;

				levelVideoImage = levelVideoBackground.gameObject.AddComponent<RawImage>();
				levelVideoImage.raycastTarget = false;
				levelVideo.targetTexture = new RenderTexture(300, 150, 16);
				levelVideo.aspectRatio = VideoAspectRatio.Stretch;
				(levelVideoImage as RawImage).texture = levelVideo.targetTexture;

				for (ushort i = 0; i < levelVideo.audioTrackCount; i++)
				{
					levelVideo.SetDirectAudioVolume(i, 0);
				}
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
