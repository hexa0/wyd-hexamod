using UnityEngine;

namespace HexaMod.UI.Interface.Label
{
	public interface IText<Self>
	{
		// Text
		Self SetText(string text);
		string GetText();

		// Font
		Self SetTextFont(Font font);
		Self SetTextFontSize(object fontSize);
		Self SetTextFontResizeMinSize(int minFontSize);
		Self SetTextFontResizeMaxSize(int maxFontSize);

		// Alignment
		Self SetTextAligment(TextAnchor alignment);

		// Features
		Self SetRichTextEnabled(bool richTextEnabled);
		Self SetResizeForBestFitEnabled(bool dynamicResizeEnabled);
	}
}
