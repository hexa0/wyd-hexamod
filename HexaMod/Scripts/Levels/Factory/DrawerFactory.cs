using System.Collections;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class DrawerFactory : MonoBehaviour
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
		}

		public DrawerFactory blockerDrawer;
		public float length = 0.75f;
	}
}
