using UnityEngine;

namespace HexaMod.API.UI.Interface.Label
{
	public interface IShadow<Self>
	{
		Self SetShadowEnabled(bool isShadowEnabled);
		Self SetShadowColor(Color color);
		Self SetShadowDistance(Vector2 distance);
	}
}
