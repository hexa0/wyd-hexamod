namespace HexaMod.UI.Element.Label
{
	public class WActionLabel : WLabel
	{
		public ActionText action;

		public WActionLabel() : base()
		{
			action = gameObject.AddComponent<ActionText>();
		}
	}
}
