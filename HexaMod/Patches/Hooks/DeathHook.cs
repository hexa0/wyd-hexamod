using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using HexaMapAssemblies;
using HexaMod.UI;
using static HexaMod.UI.Util.Menu;
using HexaMod.Util;
using UnityEngine;
using UnityStandardAssets.Effects;
using HexaMod.Patches.Fixes;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Hooks
{
	enum DeathCause
	{
		Unknown,
		Cooked,
		CookedStove,
		CookedGrill,
		CookedCandle,
		Frozen,
		FrozenFridge,
		Drowned,
		DrownedTub,
		DrownedPool,
		DrownedToilet,
		Exploded,
		ExplodedOutlet,
		ExplodedCar,
		ExplodedToaster,
		Sickness,
		SicknessBleach,
		SicknessSoap,
	}

	public class DeathManager : Photon.MonoBehaviour
	{
		BabyStats stats;
		ItemTargeting itemTargetting;
		BabyChallengeManager babyChallengeManager;

		void Awake()
		{
			stats = GetComponent<BabyStats>();
			itemTargetting = GetComponent<ItemTargeting>();

			GameObject babyChallengeManagerObject = GameObject.Find("BabyChallengeManager");

			if (babyChallengeManagerObject)
			{
				babyChallengeManager = babyChallengeManagerObject.GetComponent<BabyChallengeManager>();
			}
		}

		public bool isDead = false;

		internal static ExplosionPhysicsForce lastExplosion;
		internal static float lastExplosionTime = 0f;
		internal static readonly System.Random random = new System.Random();
		internal static readonly string deathMessagePrefix = "<color=red>☠️</color> ";

		static readonly Dictionary<DeathCause, string[]> deathMessages = new Dictionary<DeathCause, string[]>()
		{
			[DeathCause.Unknown] = new string[] {
				"PLAYER has died",
				"PLAYER met their demise",
				"PLAYER has ceased to exist",
				"PLAYER has fallen",
				"PLAYER has fallen asleep (totally)",
				"hmm.. i wonder where PLAYER went?",
				"PLAYER died of limga",
				"PLAYER has mysteriously disappeared",
				"PLAYER died of death",
				"PLAYER disrespect button --------------->",
				"PLAYER flatlined",
				"PLAYER forgot to breath",
				"PLAYER saw everything in the nan zone and died from too much knowledge",
				"*Drops PLAYER* \"Please don't be cracked\"..... PLAYER:",
				"PLAYER learned how babies are UN-made™️",
				"PLAYER pronounced gif incorrectly",
				"PLAYER pressed ctrl + r + enter",
				"PLAYER was aborted",
				"PLAYER opened osoobe.exe",
				"PLAYER forgot to install counter strike source",
				"PLAYER didn't drink enough water",
				"TW: Emotional.... PLAYER has FUCKING died",
				"PLAYER has undefined",
				"PLAYER's health went below 0",
				"PLAYER forgot to use PlayIntegriddyFix on their iPhone",
				"PLAYER used 44100hz instead of 48000hz",
				"PLAYER got woken up to the sounds of HELL SOUND EFFECTS-1hour | WARNING | DISTURBING SOUNDS | Cries, Agony, Demons ,Whips ,Eternal Fire at 3am in the morning you guys",
				"ummm guys i think PLAYER might of died... /srs",
				"GameObject.DestroyImmediate(PLAYER)",
				"PLAYER had a skill issue",
				"PLAYER [object Object]",
			},
		};

		DeathCause DetermineDeathCause()
		{
			return DeathCause.Unknown;
		}

		public void TriggerDeath()
		{
			if (isDead || !photonView.isMine)
			{
				return;
			}

			isDead = true;

			stats.mainCam.GetComponent<Camera>().enabled = false;
			stats.mainCam.GetComponent<AudioListener>().enabled = false;

			string chatColor = MainUI.GetCurrentShirtColorHex();

			if (!chatColor.StartsWith("#"))
			{
				chatColor = $"#{chatColor}";
			}

			DeathCause deathCause = DetermineDeathCause();

			string[] randomMessages = deathMessages.ContainsKey(deathCause) ? deathMessages[deathCause] : deathMessages[DeathCause.Unknown];

			HexaMod.textChat.SendUnformattedChatMessage(
				deathMessagePrefix + randomMessages[random.Next(0, randomMessages.Length - 1)].Replace(
					"PLAYER",
					$"<b><color=\"{chatColor}\">{PhotonNetwork.playerName}</color></b>"
				)
			);

			photonView.RPC("Died", PhotonTargets.All);
		}

		[PunRPC]
		public IEnumerator Died(PhotonMessageInfo info)
		{
			if (photonView.owner != info.sender)
			{
				if (PhotonNetwork.isMasterClient)
				{
					PhotonNetwork.CloseConnection(info.sender); // kick players who try to send a dead event for another player
				}
			}
			else
			{
				WinManager.lastPlayerWon = photonView.owner;

				MakeDead();

				if (photonView.isMine && babyChallengeManager && !babyChallengeManager.isComplete)
				{
					Menus.inGame.menuController.ChangeToMenu(12);
				}
				else
				{
					HexaMod.gameStateController.BabyWins();

					if (photonView.isMine)
					{
						stats.deathCam.GetComponent<Camera>().enabled = true;
					}
				}

				MakeThrowable();

				if (photonView.isMine)
				{
					yield return new WaitForEndOfFrame();

					DoExplosionForce();
				}
			}
		}

		void MakeDead()
		{
			isDead = true;
			stats.dead = true;
			stats.controlScript.enabled = false;
			itemTargetting.enabled = false;
			stats.babyModel.SendMessage("Death");
			stats.headBone.SendMessage("Death");


			if (stats.yellSound)
			{
				stats.yellSound.loop = false;
				stats.yellSound.Stop();
			}

			if (stats.healthObj)
			{
				stats.healthObj.enabled = false;
			}

			if (photonView.isMine)
			{
				itemTargetting.reticle.gameObject.SetActive(false);
				itemTargetting.textComp.gameObject.SetActive(false);
				itemTargetting.enabled = false;
			}
		}

		void MakeThrowable()
		{
			gameObject.transform.SetParent(null);
			gameObject.name = "Dead Baby";
			Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
			rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidBody.isKinematic = false;
			rigidBody.useGravity = true;
			DestroyImmediate(gameObject.GetComponent<NetworkMovement>());
			DestroyImmediate(gameObject.GetComponent<CharacterReplication>());
			gameObject.GetComponent<CharacterController>().enabled = false;
			NetworkMovementRB networkMovement = gameObject.AddComponent<NetworkMovementRB>();
			networkMovement.timer = 5f;
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.isTrigger = false;
			collider.center = new Vector3(0f, -0.13f, -0.3f);
			collider.size = new Vector3(0.5f, 0.2f, 1.2f);
			gameObject.AddComponent<PickUpFactory>();
			gameObject.tag = "Untagged";
		}

		void DoExplosionForce()
		{
			Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

			if ((Time.time - lastExplosionTime) < 1f)
			{
				float multiplier = lastExplosion.GetComponent<ParticleSystemMultiplier>().multiplier;
				float radius = 20f * multiplier;
				Vector3 underBabyExplosionPosition = lastExplosion.transform.position;
				underBabyExplosionPosition.y = rigidBody.position.y - 5f;
				rigidBody.AddExplosionForce(lastExplosion.explosionForce * 1.5f * multiplier, underBabyExplosionPosition, radius, 1f * multiplier, ForceMode.Impulse);
			}
		}
	}

	[HarmonyPatch]
	internal class DeathHook
	{
		[HarmonyPatch(typeof(FirstPersonController), "Start")]
		[HarmonyPostfix]
		static void Start(ref FirstPersonController __instance)
		{
			__instance.gameObject.AddComponent<DeathManager>();
			GameObject deathCam = __instance.Find("Camera").gameObject;
			deathCam.GetComponent<Camera>().enabled = false;
			deathCam.GetComponent<AudioListener>().enabled = false;
			deathCam.SetActive(true);
			deathCam.name = "DeathCam";
		}

		[HarmonyPatch(typeof(BabyStats), "Dead")]
		[HarmonyPrefix]
		static bool Dead(ref BabyStats __instance)
		{
			GameMode gameMode = GameModes.gameModes[HexaMod.networkManager.curGameMode];

			if (gameMode.babiesCanDie)
			{
				DeathManager deathHandler = __instance.GetComponent<DeathManager>();
				deathHandler.TriggerDeath();
			}

			return false;
		}

		[HarmonyPatch(typeof(BabyStats), "ShockedRPC")]
		[HarmonyPrefix]
		static bool ShockedRPC(ref BabyStats __instance)
		{
			__instance.heat = 200f;
			__instance.cooking = true;
			__instance.Dead();

			return false;
		}

		[HarmonyPatch(typeof(BabyStats), "Shocked")]
		[HarmonyPrefix]
		static bool Shocked(ref BabyStats __instance)
		{
			__instance.GetComponent<PhotonView>().RPC("ShockedRPC", PhotonTargets.All);
			return false;
		}

		[HarmonyPatch(typeof(ExplosionPhysicsForce), "Start")]
		[HarmonyPrefix]
		static void ExplosionPhysicsForceStart(ref ExplosionPhysicsForce __instance)
		{
			DeathManager.lastExplosion = __instance;
			DeathManager.lastExplosionTime = Time.time;
		}
	}
}