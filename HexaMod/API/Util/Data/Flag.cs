namespace HexaMod.API.Util.Data
{
	public class FlagHelper<FlagEnum> where FlagEnum : struct
	{
		public delegate ref FlagEnum GetRefDelegate();
		public readonly GetRefDelegate getRefDelegate;

		public bool IsSet(FlagEnum flag)
		{
			ref FlagEnum flags = ref getRefDelegate();

			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			return (flagsValue & flagValue) != 0;
		}

		public void Set(FlagEnum flag)
		{
			ref FlagEnum flags = ref getRefDelegate();

			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			flags = (FlagEnum)(object)(flagsValue | flagValue);
		}

		public void Unset(FlagEnum flag)
		{
			ref FlagEnum flags = ref getRefDelegate();

			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			flags = (FlagEnum)(object)(flagsValue & (~flagValue));
		}

		public FlagHelper(GetRefDelegate flagRefDelegate) {
			getRefDelegate = flagRefDelegate;
		}
	}
}
