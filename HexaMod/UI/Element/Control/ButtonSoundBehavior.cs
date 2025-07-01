using UnityEngine.EventSystems;
using UnityEngine;

namespace HexaMod.UI.Element.Control
{
	public enum UISound
	{
		Forward,
		Back,
		Hover,
		Yes,
		No,
		None
	}

	public class ButtonSoundBehavior : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, IPointerDownHandler
	{
		static readonly AudioClip interact01 = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/UI/Interact01.wav");
		static readonly AudioClip interact02 = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/UI/Interact02.wav");
		static readonly AudioClip yes = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/UI/Yes.wav");
		static readonly AudioClip no = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/UI/No.wav");
		static readonly AudioClip hover = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/UI/Hover.wav");

		public UISound ButtonDownSound = UISound.Forward;
		public UISound ButtonUpSound = UISound.Forward;
		public UISound HoverSound = UISound.Hover;

		public float lastEnableTime = 0f;

		public void Play(UISound sound, bool down = true)
		{
			switch (sound)
			{
				case UISound.Forward:
					HexaMenus.uiAudioSource.PlayOneShot(down ? interact01 : interact02);
					break;
				case UISound.Back:
					HexaMenus.uiAudioSource.PlayOneShot(down ? interact02 : interact01);
					break;
				case UISound.Yes:
					HexaMenus.uiAudioSource.PlayOneShot(yes);
					break;
				case UISound.No:
					HexaMenus.uiAudioSource.PlayOneShot(no);
					break;
				case UISound.Hover:
					HexaMenus.uiAudioSource.PlayOneShot(hover, 0.5f);
					break;
			}
		}

		public void OnEnable()
		{
			lastEnableTime = Time.time;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			Play(ButtonDownSound, true);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Play(ButtonUpSound, false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Time.time - lastEnableTime < 0.1f) { return; };
			Play(HoverSound);
		}
	}
}
