using UnityEngine.UI;

namespace HexaMod.UI.Element.Control.TextInputField
{
	public class WSensitiveTextInputField : WTextInputField
	{
		public bool revealOnFocus = true;

		public override void Update()
		{
			base.Update();

			SetContentType((field.isFocused && revealOnFocus) ? InputField.ContentType.Standard : InputField.ContentType.Password);
		}
	}
}
