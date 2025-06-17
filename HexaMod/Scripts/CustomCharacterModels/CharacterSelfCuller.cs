using UnityEngine;

namespace HexaMapAssemblies
{
	public class CharacterSelfCuller : MonoBehaviour
	{
		public void Cull()
		{
			gameObject.layer = 12;
		}
	}
}
