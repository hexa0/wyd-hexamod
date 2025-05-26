using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	public class NaNFixBehavior : MonoBehaviour
	{
		public FirstPersonController firstPersonController;
		public Vector3 cameraPosition;
		public Quaternion cameraRotation;
		public Vector3 characterPosition;
		public Quaternion characterRotation;
		void Start()
		{
			// firstPersonController.enabled = false;
			var privateFields = Traverse.Create(firstPersonController);
			firstPersonController.transform.position = characterPosition;
			firstPersonController.transform.rotation = characterRotation;
			var m_Camera = privateFields.Field<Camera>("m_Camera");
			m_Camera.Value.transform.position = cameraPosition;
			m_Camera.Value.transform.rotation = cameraRotation;
			CharacterController readOnlyShit = firstPersonController.GetComponent<CharacterController>();
			readOnlyShit.center = readOnlyShit.center; // why does this work????
			StartCoroutine(Wait());
		}

		void Update()
		{
			FixedUpdate();
		}

		void FixedUpdate()
		{
			var privateFields = Traverse.Create(firstPersonController);
			var m_Camera = privateFields.Field<Camera>("m_Camera");
			firstPersonController.transform.position = characterPosition;
			firstPersonController.transform.rotation = characterRotation;
			m_Camera.Value.transform.position = cameraPosition;
			m_Camera.Value.transform.rotation = cameraRotation;
			CharacterController readOnlyShit = firstPersonController.GetComponent<CharacterController>();
			readOnlyShit.center = readOnlyShit.center; // why does this work????
		}

		IEnumerator Wait()
		{

			yield return new WaitForSeconds(0.1f);
			// firstPersonController.enabled = true;
			Destroy(this);
			NanFix.fixingNaN = false;
		}
	}
}
