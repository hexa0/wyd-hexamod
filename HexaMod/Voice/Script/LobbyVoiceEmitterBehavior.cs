using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Voice.Script
{
	public class LobbyVoiceEmitterBehavior : MonoBehaviour
	{
		GameObject[] emitters;
		GameObject[] dadIndicators;
		GameObject[] babyIndicators;
		PlayerNames playerNames;

		void Start()
		{
			playerNames = GetComponent<PlayerNames>();

			Refresh();
		}

		void MakeIndicators()
		{
			if (dadIndicators != null)
			{
				foreach (var item in dadIndicators)
				{
					Destroy(item);
				}
			}

			if (babyIndicators != null)
			{
				foreach (var item in babyIndicators)
				{
					Destroy(item);
				}
			}

			dadIndicators = new GameObject[playerNames.daddyNames.Length];
			babyIndicators = new GameObject[playerNames.babyNames.Length];

			GameObject onNameItem(GameObject gameObject)
			{
				var kickPlayer = gameObject.Find("KickPlayer");
				var speakingIndicator = Instantiate(kickPlayer, gameObject.transform);
				speakingIndicator.name = "Speaking";

				speakingIndicator.GetComponentInChildren<Button>().enabled = false;
				speakingIndicator.GetComponentInChildren<Image>().enabled = false;

				if (!gameObject.GetComponent<PlayerNameLobby>().hideButton)
				{
					speakingIndicator.transform.localPosition = new Vector2(PhotonNetwork.isMasterClient ? 20f : 120f, speakingIndicator.transform.localPosition.y);
				}
				else
				{
					speakingIndicator.transform.localPosition = new Vector2(PhotonNetwork.isMasterClient ? 70f : 120f, speakingIndicator.transform.localPosition.y);
				}

				speakingIndicator.GetComponentInChildren<Text>(true).text = "*";
				speakingIndicator.SetActive(false);

				return speakingIndicator;
			}

			int i = 0;
			foreach (var item in playerNames.daddyNames)
			{
				dadIndicators[i] = onNameItem(item.gameObject);
				i++;
			}

			i = 0;
			foreach (var item in playerNames.babyNames)
			{
				babyIndicators[i] = onNameItem(item.gameObject);
				i++;
			}
		}

		void OnDisable()
		{
			if (dadIndicators != null)
			{
				foreach (var item in dadIndicators)
				{
					Destroy(item);
				}

				dadIndicators = null;
			}

			if (babyIndicators != null)
			{
				foreach (var item in babyIndicators)
				{
					Destroy(item);
				}

				babyIndicators = null;
			}

			if (emitters != null)
			{
				foreach (var emitter in emitters)
				{
					Destroy(emitter);
				}

				emitters = null;
			}
		}

		void OnEnable()
		{
			Refresh();
		}

		public void Refresh()
		{
			if (playerNames == null) { return; }

			if (emitters != null)
			{
				foreach (var emitter in emitters)
				{
					Destroy(emitter);
				}

				emitters = null;
			}

			int players = playerNames.daddyPlayerIds.Count + playerNames.babyPlayerIds.Count;
			emitters = new GameObject[players];

			for (int playerI = 0; playerI < players; playerI++)
			{
				emitters[playerI] = new GameObject($"voice {playerI}");

				AudioSource voiceSource = emitters[playerI].AddComponent<AudioSource>();
				voiceSource.spatialBlend = 0f;
				voiceSource.spatialize = false;
				voiceSource.spread = 1f;
				voiceSource.bypassEffects = true;
				voiceSource.loop = true;
				voiceSource.volume = 1f;

				VoiceEmitter voiceEmitter = emitters[playerI].AddComponent<VoiceEmitter>();

				voiceEmitter.enabled = false;
				voiceSource.enabled = false;

				voiceSource.enabled = true;
				voiceEmitter.enabled = true;
			}

			int playerIndex = 0;

			MakeIndicators();

			int i = 0;
			playerNames.daddyPlayerIds.ForEach(player => {
				VoiceEmitter emitter = emitters[playerIndex].GetComponent<VoiceEmitter>();
				emitter.player = player;
				emitter.speakingObject = dadIndicators[i];
				playerIndex++;
				i++;
			});

			i = 0;
			playerNames.babyPlayerIds.ForEach(player => {
				VoiceEmitter emitter = emitters[playerIndex].GetComponent<VoiceEmitter>();
				emitter.player = player;
				emitter.speakingObject = babyIndicators[i];
				playerIndex++;
				i++;
			});
		}
	}
}
