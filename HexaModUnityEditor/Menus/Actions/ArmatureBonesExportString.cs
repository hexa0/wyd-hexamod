using UnityEditor;
using UnityEngine;

namespace HexaModEditor.Menus.Actions
{
	public class ArmatureBonesExportString
	{
		[MenuItem("Assets/Print Bone Strings")]
		public static void PrintBoneStrings()
		{
			Transform[] bones = Selection.activeTransform.GetComponent<SkinnedMeshRenderer>().bones;
			for (int i = 0; i < bones.Length; i++)
			{
				Debug.Log("[" + i + "] " + bones[i].name);
			}
		}
	}
}
