using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	internal class FloatyMessage
	{
		public GameObject item;
		public GivePills baby;
	}

	internal class Floaty : MonoBehaviour
	{
		static float height = -1f;
		public bool hasFloaty = false;

		void Update()
		{
			if (hasFloaty)
			{
				if (transform.position.y < height)
				{
					float depth = height - transform.position.y;

					CharacterController controller = GetComponent<CharacterController>();
					controller.center = controller.center;
					controller.Move(Vector3.up * depth);

					FirstPersonController character = GetComponent<FirstPersonController>();
					Traverse characterFields = Traverse.Create(character);
					var m_MoveDir = characterFields.Field<Vector3>("m_MoveDir");
					Vector3 moveDir = m_MoveDir.Value;
					moveDir.y = 0f;
					m_MoveDir.Value = moveDir;
				}

				GetComponent<BabyStats>().drownness = 0f;
			}
		}

		public IEnumerator ApplyFloaty(FloatyMessage message)
		{
			yield return new WaitForEndOfFrame();

			message.item.transform.SetParent(message.baby.transform); // this was also set incorrectly for others due to race conditions
			message.item.transform.localPosition = new Vector3(0f, 0f, -0.26f);
			message.item.transform.localRotation = Quaternion.identity;
			message.item.transform.localScale = new Vector3(20f, 20f, 20f);
		}
	}
	[HarmonyPatch]
	internal class FloatyFix
	{
		[HarmonyPatch(typeof(BabyStats), "Start")]
		[HarmonyPostfix]
		static void Start(ref BabyStats __instance)
		{
			__instance.gameObject.AddComponent<Floaty>();
		}

		[HarmonyPatch(typeof(BabyStats), "Update")]
		[HarmonyPrefix]
		static void Update(ref BabyStats __instance)
		{
			Floaty floaty = __instance.GetComponent<Floaty>();

			if (__instance.hasFloaty == true)
			{
				__instance.hasFloaty = false;
				floaty.hasFloaty = true;
			}

			if (floaty.hasFloaty)
			{
				Traverse fields = Traverse.Create(__instance);
				var drowning = fields.Field<bool>("drowning");

				drowning.Value = false;
			}
		}

		[HarmonyPatch(typeof(GivePills), "RPCUseInteract")]
		[HarmonyPrefix]
		static bool RPCUseInteract(string input1, string input2, ref GivePills __instance)
		{
			GameObject item = GameObject.Find(input1);
			GameObject player = GameObject.Find(input2);

			if (item && item.tag == "Floaty" && input2.Substring(0, 3) == "Dad")
			{
				Floaty floaty = __instance.GetComponent<Floaty>();
				floaty.hasFloaty = true;


				if (PhotonNetwork.player.ID == player.GetComponent<PhotonView>().ownerId)
				{
					player.SendMessage("DropItem");
				}

				__instance.GetComponent<Floaty>().hasFloaty = true;
				Object.Destroy(item.GetComponent<Rigidbody>());
				Object.Destroy(item.GetComponent<NetworkMovementRB>());
				Object.Destroy(item.GetComponent<Fork>());

				foreach (var collider in item.GetComponents<Collider>()) // this originally failed because it expected a box collider
				{
					Object.Destroy(collider);
				}

				item.layer = 2;

				floaty.SendMessage("ApplyFloaty", new FloatyMessage()
				{
					baby = __instance,
					item = item
				});

				return false;
			}

			return true;
		}
	}
}