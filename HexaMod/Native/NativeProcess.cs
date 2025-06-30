using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HexaMod;

/// <summary>
/// A simple helper for the native Win32 APIs to start processes and handle their output.
/// </summary>
public static class NativeProcess
{
	// Win32 Structs
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct STARTUPINFO
	{
		public int cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public int dwX;
		public int dwY;
		public int dwXSize;
		public int dwYSize;
		public int dwXCountChars;
		public int dwYCountChars;
		public int dwFillAttribute;
		public int dwFlags;
		public short wShowWindow;
		public short cbReserved2;
		public IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;
		public IntPtr hThread;
		public int dwProcessId;
		public int dwThreadId;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SECURITY_ATTRIBUTES
	{
		public int nLength;
		public IntPtr lpSecurityDescriptor;
		[MarshalAs(UnmanagedType.Bool)]
		public bool bInheritHandle;
	}

	// Win32 Constants
	private const int CREATE_NO_WINDOW = 0x08000000;
	private const int NORMAL_PRIORITY_CLASS = 0x00000020;
	private const int STARTF_USESTDHANDLES = 0x00000100;
	private const int HANDLE_FLAG_INHERIT = 1;
	private const uint INFINITE = 0xFFFFFFFF;

	// Win32 Function Imports (P/Invoke)
	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern bool CreateProcess(
		string lpApplicationName,
		string lpCommandLine,
		IntPtr lpProcessAttributes,
		IntPtr lpThreadAttributes,
		bool bInheritHandles,
		int dwCreationFlags,
		IntPtr lpEnvironment,
		string lpCurrentDirectory,
		ref STARTUPINFO lpStartupInfo,
		out PROCESS_INFORMATION lpProcessInformation
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetHandleInformation(IntPtr hObject, int dwMask, int dwFlags);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool CloseHandle(IntPtr hObject);

	// Helper class for passing state to the pipe reader thread
	private class PipeReaderState
	{
		public IntPtr PipeHandle { get; set; }
		public Action<string> Callback { get; set; }
	}

	/// <summary>
	/// Sanitizes a single object and wraps it in quotes if necessary for the command line.
	/// </summary>
	/// <param name="inputArgument">The object to sanitize. It will be converted using .ToString().</param>
	/// <returns>A command-line-safe string.</returns>
	public static string SanitizeArgument(object inputArgument)
	{
		if (inputArgument == null) return "\"\"";
		string argument = inputArgument.ToString();

		if (argument.Length > 0 && argument.IndexOfAny(new[] { ' ', '\t', '"' }) == -1)
		{
			return argument;
		}

		var stringBuilder = new StringBuilder();
		stringBuilder.Append('"');

		for (int i = 0; i < argument.Length; ++i)
		{
			int backslashCount = 0;
			while (i < argument.Length && argument[i] == '\\')
			{
				backslashCount++;
				i++;
			}

			if (i == argument.Length)
			{
				stringBuilder.Append('\\', backslashCount * 2);
			}
			else if (argument[i] == '"')
			{
				stringBuilder.Append('\\', backslashCount * 2 + 1);
				stringBuilder.Append('"');
			}
			else
			{
				stringBuilder.Append('\\', backslashCount);
				stringBuilder.Append(argument[i]);
			}
		}

		stringBuilder.Append('"');
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Builds a command-line-safe arguments string from multiple objects.
	/// </summary>
	/// <param name="argumentsToBuild">A variable number of objects to be converted and sanitized.</param>
	/// <returns>A single string with all arguments sanitized and joined by spaces.</returns>
	public static string BuildArguments(params object[] argumentsToBuild)
	{
		if (argumentsToBuild == null || argumentsToBuild.Length == 0)
		{
			return "";
		}

		var sb = new StringBuilder();
		for (int i = 0; i < argumentsToBuild.Length; i++)
		{
			sb.Append(SanitizeArgument(argumentsToBuild[i]));
			if (i < argumentsToBuild.Length - 1)
			{
				sb.Append(" ");
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Starts a process using native Win32 APIs as a workaround for the issues with Process.Start
	/// </summary>
	public static void StartProcess(string executablePath, string arguments = null, string workingDirectory = null, Action<string> standardOutputDataReceived = null, Action<string> standardErrorDataReceived = null)
	{
		IntPtr stdoutReadPipeHandle = IntPtr.Zero, stdoutWritePipeHandle = IntPtr.Zero;
		IntPtr stderrReadPipeHandle = IntPtr.Zero, stderrWritePipeHandle = IntPtr.Zero;
		PROCESS_INFORMATION processInformation = new PROCESS_INFORMATION();

		string commandLine = string.IsNullOrEmpty(arguments)
			? SanitizeArgument(executablePath)
			: $"{SanitizeArgument(executablePath)} {arguments}";

		Exception thrownException = null;

		try
		{
			SECURITY_ATTRIBUTES securityAttributes = new SECURITY_ATTRIBUTES();
			securityAttributes.nLength = Marshal.SizeOf(securityAttributes);
			securityAttributes.bInheritHandle = true;

			// Create pipes for stdout and stderr, making the write handles inheritable.
			if (!CreatePipe(out stdoutReadPipeHandle, out stdoutWritePipeHandle, ref securityAttributes, 0)) throw new System.ComponentModel.Win32Exception();
			if (!CreatePipe(out stderrReadPipeHandle, out stderrWritePipeHandle, ref securityAttributes, 0)) throw new System.ComponentModel.Win32Exception();

			// Ensure the read handles for the pipes are NOT inherited by the child process.
			if (!SetHandleInformation(stdoutReadPipeHandle, HANDLE_FLAG_INHERIT, 0)) throw new System.ComponentModel.Win32Exception();
			if (!SetHandleInformation(stderrReadPipeHandle, HANDLE_FLAG_INHERIT, 0)) throw new System.ComponentModel.Win32Exception();

			STARTUPINFO startupInfo = new STARTUPINFO();
			startupInfo.cb = Marshal.SizeOf(startupInfo);
			startupInfo.dwFlags = STARTF_USESTDHANDLES;
			startupInfo.hStdInput = IntPtr.Zero;
			startupInfo.hStdOutput = stdoutWritePipeHandle;
			startupInfo.hStdError = stderrWritePipeHandle;

			Mod.Debug($"Starting process: {commandLine}");

			int creationFlags = CREATE_NO_WINDOW | NORMAL_PRIORITY_CLASS;
			bool inheritHandles = true; // Must be true for pipe redirection to work.

			bool success = CreateProcess(
				null,
				commandLine,
				IntPtr.Zero,
				IntPtr.Zero,
				inheritHandles,
				creationFlags,
				IntPtr.Zero,
				workingDirectory,
				ref startupInfo,
				out processInformation
			);

			if (!success)
			{
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
			}

			// In the parent process, close the write ends of the pipes. This is crucial.
			// If we don't, the reader threads will never detect the end of the stream.
			CloseHandle(stdoutWritePipeHandle); stdoutWritePipeHandle = IntPtr.Zero;
			CloseHandle(stderrWritePipeHandle); stderrWritePipeHandle = IntPtr.Zero;

			// Start reader threads to asynchronously capture output.
			if (standardOutputDataReceived != null)
			{
				var state = new PipeReaderState { PipeHandle = stdoutReadPipeHandle, Callback = standardOutputDataReceived };
				ThreadPool.QueueUserWorkItem(ReadPipeThread, state);
				stdoutReadPipeHandle = IntPtr.Zero; // Ownership of the handle is passed to the new thread.
			}

			if (standardErrorDataReceived != null)
			{
				var state = new PipeReaderState { PipeHandle = stderrReadPipeHandle, Callback = standardErrorDataReceived };
				ThreadPool.QueueUserWorkItem(ReadPipeThread, state);
				stderrReadPipeHandle = IntPtr.Zero; // Ownership of the handle is passed to the new thread.
			}

			// The main thread handle of the new process is not needed.
			CloseHandle(processInformation.hProcess);

			// Start a background thread to wait for the process to exit and then close its handle.
			ThreadPool.QueueUserWorkItem(state => {
				var hProcess = (IntPtr)state;
				WaitForSingleObject(hProcess, INFINITE);
				CloseHandle(hProcess);
			}, processInformation.hProcess);
		}
		catch (System.ComponentModel.Win32Exception ex)
		{
			thrownException = ex;
		}
		catch (Exception ex)
		{
			thrownException = ex;
		}
		finally
		{
			if (stdoutReadPipeHandle != IntPtr.Zero) CloseHandle(stdoutReadPipeHandle);
			if (stdoutWritePipeHandle != IntPtr.Zero) CloseHandle(stdoutWritePipeHandle);
			if (stderrReadPipeHandle != IntPtr.Zero) CloseHandle(stderrReadPipeHandle);
			if (stderrWritePipeHandle != IntPtr.Zero) CloseHandle(stderrWritePipeHandle);

			if (thrownException != null)
			{
				Mod.Fatal($"Failed to start process: {commandLine}");
				throw thrownException; // Re-throw the exception to the caller.
			}
		}
	}

	/// <summary>
	/// Executed by the pipe reader threads.
	/// </summary>
	private static void ReadPipeThread(object state)
	{
		var readerState = (PipeReaderState)state;
		IntPtr hPipe = readerState.PipeHandle;
		Action<string> callback = readerState.Callback;

		const int bufferSize = 4096;
		byte[] buffer = new byte[bufferSize];
		var encoding = Encoding.Default;

		try
		{
			while (ReadFile(hPipe, buffer, bufferSize, out int bytesRead, IntPtr.Zero) && bytesRead > 0)
			{
				string output = encoding.GetString(buffer, 0, bytesRead);
				try
				{
					callback(output);
				}
				catch (Exception ex)
				{
					Mod.Error($"Exception in output callback: {ex.Message}");
				}
			}
		}
		catch (Exception ex)
		{
			Mod.Warn($"Exception while reading pipe: {ex.Message}");
		}
		finally
		{
			CloseHandle(hPipe);
		}
	}
}