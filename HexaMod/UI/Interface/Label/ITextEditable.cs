using static UnityEngine.UI.InputField;

namespace HexaMod.UI.Interface.Label
{
	public interface ITextEditable<Self> : IText<Self>
	{
		Self SetTextReplacementCharacter(string replacementCharacter);
		Self SetTextContentType(ContentType contentType);
	}
}
