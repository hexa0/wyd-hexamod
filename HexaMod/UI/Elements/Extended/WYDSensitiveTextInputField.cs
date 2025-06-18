using UnityEngine.UI;

namespace HexaMod.UI.Elements.Extended
{
	public class WYDSensitiveTextInputField : WYDTextInputField
	{
		public override void Update()
		{
			base.Update();

			SetContentType(field.isFocused ? InputField.ContentType.Standard : InputField.ContentType.Password);
		}
	}
}
