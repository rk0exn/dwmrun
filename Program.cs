// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal static class Program
{
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref uint pvAttribute, int cbAttribute);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumChildWindows(nint hwndParent, EnumWindowsProc lpEnumFunc, nint lParam);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWindow(nint hWnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWindowVisible(nint hWnd);

	private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

	static void Main(string[] args)
	{
		Console.WriteLine("DWM_Run v1.0 Copyright (c) 2025 rk0exn All rights reserved.\n");
		if (args.Length < 1)
		{
			ShowHelp();
			Environment.Exit(1);
			return;
		}
		if (!CheckArgs(args))
		{
			ShowHelp(true);
			Environment.Exit(1);
			return;
		}

		List<ParamObject> list = [];
		foreach (var a in args)
		{
			var obj = new ParamObject(a);
			if (!IsValidParameterName(obj))
			{
				ShowHelp(true);
				Environment.Exit(1);
				return;
			}
			list.Add(obj);
		}

		var pidParam = list.Find(p => p.Name.Equals("pid", StringComparison.OrdinalIgnoreCase));
		if (pidParam == null)
		{
			ShowHelp(true);
			Environment.Exit(1);
			return;
		}

		int pid = new Int32Object(pidParam.Value).Value;
		bool dark = true;
		uint color = 0xffffffff;

		var darkParam = list.Find(p => p.Name.Equals("dark", StringComparison.OrdinalIgnoreCase));
		if (darkParam != null)
		{
			dark = new BooleanObject(darkParam.Value).Value;
		}

		var colorParam = list.Find(p => p.Name.Equals("color", StringComparison.OrdinalIgnoreCase));
		if (colorParam != null)
		{
			color = new ColorObject(colorParam.Value).NativeColorRef;
		}

		Execute(pid, dark, color);
	}

	private static void Execute(int pid, bool dark, uint color)
	{
		Process p = Process.GetProcessById(pid);
		if ((nint)p.MainWindowHandle != 0)
		{
			var handle = p.MainWindowHandle;
			Console.WriteLine($"[PID={pid}] メインウィンドウ: 0x{handle:X}");
			HashSet<nint> visited = [];
			ApplyDwmRecursive(handle, dark, color, visited);
		}
		else
		{
			Console.WriteLine($"PID {pid} のメインウィンドウが見つかりません。");
		}
	}

	private static void ApplyDwmRecursive(nint hwnd, bool dark, uint color, HashSet<nint> visited)
	{
		if (!IsWindow(hwnd) || visited.Contains(hwnd)) return;

		visited.Add(hwnd);
		int useDark = dark ? 1 : 0;
		int hr1 = DwmSetWindowAttribute(hwnd, 20, ref useDark, sizeof(int));
		int hr2 = DwmSetWindowAttribute(hwnd, 35, ref color, sizeof(uint));

		if (hr1 == 0 && hr2 == 0)
		{
			Console.WriteLine($"  - 適用: HWND=0x{hwnd:X} dark={dark}");
		}

		EnumChildWindows(hwnd, (child, lparam) =>
		{
			ApplyDwmRecursive(child, dark, color, visited);
			return true;
		}, 0);
	}

	private static bool IsValidParameterName(ParamObject paramObject)
	{
		return paramObject.Name.ToLower() is "pid" or "dark" or "color";
	}

	private static bool CheckArgs(params string[] args)
	{
		if (args.Length == 0) return false;

		System.Text.RegularExpressions.Regex r = new("^/[A-Za-z]+:[a-zA-Z0-9]+$", System.Text.RegularExpressions.RegexOptions.Compiled);
		foreach (var arg in args)
		{
			if (!r.IsMatch(arg)) return false;
		}
		return true;
	}

	internal static void ShowHelpInternal(bool argumentsError = false) => ShowHelp(argumentsError);

	private static void ShowHelp(bool argumentsError = false)
	{
		if (argumentsError) Console.WriteLine("引数エラー: パラメータの形式が正しくありません。\n");
		Console.WriteLine("使用方法:");
		Console.WriteLine("  appname.exe /pid:1234 [/dark:true] [/color:00495F]");
		Console.WriteLine();
		Console.WriteLine("引数:");
		Console.WriteLine("  /pid:<プロセスID>  対象のプロセスIDを指定します。これはメインウィンドウにのみ反映されます。");
		Console.WriteLine("  /dark:<true|false>  タイトルバーのテーマを変更します。");
		Console.WriteLine("  /color:<RGB値|ARGB値>  タイトルバーの色を変更します。この値を指定するとdarkは無視されます。");
		Console.WriteLine("※colorにFFFFFFFFを設定するか、pidとdarkのみ指定するとデフォルト色にリセットされます。");
	}
}
