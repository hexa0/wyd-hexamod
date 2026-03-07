using System;
using System.Runtime.InteropServices;

public static class Wine
{
	[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CharSet = CharSet.Ansi)]
	private static extern IntPtr wine_get_version();
	private static IsWineStatus isWine = IsWineStatus.NotChecked;

	private enum IsWineStatus
	{
		NotChecked,
		IsWine,
		IsNotWine
	}

	public static bool IsWine
	{
		get { return CheckForWine(); }
	}

	private static bool CheckForWine()
	{
		// cache the result
		if (isWine == IsWineStatus.NotChecked)
		{
			try
			{
				if (wine_get_version() != IntPtr.Zero)
				{
					isWine = IsWineStatus.IsWine;
				}
				else
				{
					isWine = IsWineStatus.IsNotWine;
				}
			}
			catch (EntryPointNotFoundException)
			{
				isWine = IsWineStatus.IsNotWine;

			}
			catch (DllNotFoundException)
			{
				isWine = IsWineStatus.IsNotWine;
			}
			catch (Exception)
			{
				isWine = IsWineStatus.IsNotWine;
			}
		}

		return isWine == IsWineStatus.IsWine;
	}
}