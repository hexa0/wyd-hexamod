using HarmonyLib;
using HexaMod.Patches.Hooks;
using HexaMod.UI.Util;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches.Fixes
{
	internal class DeathCamTarget : MonoBehaviour
	{
		public Transform target;

		void Update()
		{
			if (target != null)
			{
				transform.position = target.position;
			}

			transform.rotation = Quaternion.identity;
		}
	}

	internal class HexaDeathCam : MonoBehaviour
	{
		Camera camera;
		AudioListener listener;
		DeathCam deathCam;
		Transform target;
		PhotonPlayer player;
		Text startSpectatingText;
		bool didInit = false;

		void Init()
		{
			if (didInit) { return; }

			player = transform.parent.GetComponent<PhotonView>().owner;
			camera = GetComponent<Camera>();
			listener = GetComponent<AudioListener>();
			deathCam = GetComponent<DeathCam>();
			startSpectatingText = GameObject.Find("PressSpaceToSpectate").GetComponent<Text>();

			didInit = true;
		}

		void Awake()
		{
			Init();
		}

		void LateUpdate()
		{
			if (HexaMod.gameStateController.gameOver)
			{
				deathCam.spectateMode = false;

				if (player == WinManager.lastPlayerWon)
				{
					camera.enabled = true;
					listener.enabled = true;
				}
				else
				{
					camera.enabled = false;
					listener.enabled = false;
				}
			}

			if (!deathCam.target)
			{
				return;
			}

			if (!camera.enabled)
			{
				return;
			}

			if (!target)
			{
				GameObject newTarget = new GameObject
				{
					name = $"DeathCamTarget {player.ID}"
				};

				newTarget.transform.SetPositionAndRotation(deathCam.target.position, deathCam.target.rotation);

				DeathCamTarget deathCamTarget = newTarget.AddComponent<DeathCamTarget>();
				deathCamTarget.target = deathCam.target;

				target = newTarget.transform;
				transform.SetParent(target, true);
			}

			bool menuOpen = Menu.Menus.AnyMenuOpen();

			if (Input.GetKeyDown(KeyCode.Space) && !menuOpen)
			{
				deathCam.spectateMode = !deathCam.spectateMode;

				if (deathCam.spectateMode)
				{
					deathCam.transform.SetParent(null, true);

					if (deathCam.mouseLook)
					{
						Traverse fields = Traverse.Create(deathCam.mouseLook);

						Traverse<Vector2> mouseAbsolute = fields.Field<Vector2>("_mouseAbsolute");

						deathCam.mouseLook.targetDirection = deathCam.transform.localRotation.eulerAngles;
						mouseAbsolute.Value = Vector2.zero;

						deathCam.mouseLook.smoothing = Vector2.one;

						if (deathCam.mouseLook.characterBody)
						{
							deathCam.mouseLook.targetCharacterDirection = deathCam.mouseLook.characterBody.transform.localRotation.eulerAngles;
						}
					}
				}
				else
				{
					deathCam.transform.SetParent(deathCam.target, true);
				}
			}

			startSpectatingText.enabled = !deathCam.spectateMode && !menuOpen;
			if (deathCam.mouseLook)
			{
				deathCam.mouseLook.enabled = deathCam.spectateMode && !menuOpen;
			}

			if (!deathCam.spectateMode)
			{
				deathCam.transform.LookAt(deathCam.target);

				if (!(deathCam.transform.position.y <= deathCam.target.position.y + 1f))
				{
					float y = deathCam.transform.position.y - Time.smoothDeltaTime * 0.05f;
					Vector3 position = deathCam.transform.position;
					position.y = y;
					deathCam.transform.position = position;
				}

				deathCam.transform.RotateAround(deathCam.target.position, Vector3.up, 2f * Time.deltaTime);
			}
			else
			{
				deathCam.speed = (!deathCam.boost) ? deathCam.normSpeed : deathCam.boostSpeed;
				deathCam.transform.position += deathCam.transform.forward * deathCam.speed * deathCam.yAxis * Time.deltaTime + deathCam.transform.right * deathCam.speed * deathCam.xAxis * Time.deltaTime;
			}
		}
	}

	[HarmonyPatch(typeof(DeathCam))]
	internal class DeathCamFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref DeathCam __instance)
		{
			__instance.gameObject.AddComponent<HexaDeathCam>();
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool Update()
		{
			return false;
		}
	}
}
