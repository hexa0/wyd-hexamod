#if WINDOWS
using System.Runtime.InteropServices;

namespace VoiceChatHost.Windows.API
{
	public partial class ApplicationIcon
	{
		static IntPtr hIcon = IntPtr.Zero;

		public static void Set(string iconFileName)
		{
			try
			{
				IntPtr consoleHandle = GetConsoleWindow();

				if (consoleHandle != IntPtr.Zero)
				{
					string iconPath = Path.Combine(AppContext.BaseDirectory, iconFileName);
					if (!File.Exists(iconPath))
					{
						Console.WriteLine($"Icon file not found: {iconPath}");
						return;
					}

					if (hIcon != IntPtr.Zero)
					{
						DestroyIcon(hIcon);
					}

					hIcon = LoadImageW(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE | LR_DEFAULTSIZE);

					if (hIcon != IntPtr.Zero)
					{
						SendMessageW(consoleHandle, WM_SETICON, ICON_SMALL, hIcon);
						SendMessageW(consoleHandle, WM_SETICON, ICON_BIG, hIcon);

						try
						{
							IntPtr hInstance = GetModuleHandleW(null);
							if (hInstance != IntPtr.Zero)
							{
								SendMessageW(hInstance, WM_SETICON, ICON_SMALL, hIcon);
								SendMessageW(hInstance, WM_SETICON, ICON_BIG, hIcon);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Failed to set main icon:\n{ex.Message}");
						}
					}
					else
					{
						Console.WriteLine($"Failed to load icon from '{iconPath}'. Win32 Error: {Marshal.GetLastWin32Error()}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while setting console icon: {ex.Message}");
			}
		}

		[LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
		private static partial IntPtr GetModuleHandleW(string lpModuleName);

		[LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
		private static partial IntPtr GetConsoleWindow();

		[LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
		private static partial nint SendMessageW(nint hWnd, int Msg, int wParam, nint lParam);

		[LibraryImport("user32.dll", EntryPoint = "LoadImageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
		private static partial IntPtr LoadImageW(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

		[LibraryImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static partial bool DestroyIcon(IntPtr hIcon);

		private const int WM_SETICON = 0x0080;
		private const int ICON_SMALL = 0;
		private const int ICON_BIG = 1;

		private const uint IMAGE_ICON = 1;
		private const uint LR_LOADFROMFILE = 0x00000010;
		private const uint LR_DEFAULTSIZE = 0x00000040;
	}
}
#else
namespace VoiceChatHost.Windows.API
{
	public partial class ApplicationIcon
	{
		public static void Set(string iconFileName)
		{
			// do nothing on non-windows platforms
		}
	}
}
#endif