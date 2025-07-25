using System.Collections;
using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.c3e31544-6769-48b7-a59d-57c7cc0f79f3")]
	public class DrawerFactory : MigratableMonoBehavior
	{
		void Start()
		{
			gameObject.tag = "Open";

			var drawer = gameObject.AddComponent<Drawer>();
			drawer.open = length;
			drawer.direction = transform.up;

			if (blockerDrawer != null)
			{
				StartCoroutine(AssignCorrespondingBlocker());
			}
		}

		IEnumerator AssignCorrespondingBlocker()
		{
			// done with a delay so the other factories are done adding the Drawer components
			yield return new WaitForSeconds(1f);
			gameObject.GetComponent<Drawer>().dependentD = blockerDrawer.GetComponent<Drawer>();

			Destroy(this);
		}

		public DrawerFactory blockerDrawer;
		public float length = 0.75f;
	}
}
