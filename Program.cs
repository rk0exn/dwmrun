// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.4
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal static class Program
{
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref uint pvAttribute, int cbAttribute);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumChildWindows(nint hwndParent, EnumWindowsProc lpEnumFunc, nint lParam);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, nint lParam);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWindow(nint hWnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWindowVisible(nint hWnd);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private static extern int SetWindowTheme(nint hwnd, string pszSubAppName, string pszSubIdList);

	private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

	static void Main(string[] args)
	{
		Console.WriteLine("DWM_Run v1.4 Copyright (c) 2025 rk0exn All rights reserved.\n");
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
		HashSet<nint> visited = [];

		Console.WriteLine($"[PID={pid}] プロセス内の全ウィンドウを検索中...");

		foreach (ProcessThread thread in p.Threads)
		{
			EnumThreadWindows((uint)thread.Id, (hwnd, lparam) =>
			{
				if (!visited.Contains(hwnd))
				{
					ApplyDwmRecursive(hwnd, dark, color, visited);
				}
				return true;
			}, 0);
		}

		EnumWindows((hwnd, lparam) =>
		{
			GetWindowThreadProcessId(hwnd, out uint windowPid);
			if (windowPid == pid && !visited.Contains(hwnd))
			{
				ApplyDwmRecursive(hwnd, dark, color, visited);
			}
			return true;
		}, 0);

		Console.WriteLine($"適用完了: {visited.Count} 個のウィンドウを処理しました。");
	}

	private static void ApplyDwmRecursive(nint hwnd, bool dark, uint color, HashSet<nint> visited)
	{
		Queue<nint> queue = new();
		queue.Enqueue(hwnd);

		while (queue.Count > 0)
		{
			nint current = queue.Dequeue();

			if (visited.Contains(current)) continue;

			visited.Add(current);
			int useDark = dark ? 1 : 0;
			int hr1 = DwmSetWindowAttribute(current, 20, ref useDark, sizeof(int));
			int hr2 = DwmSetWindowAttribute(current, 35, ref color, sizeof(uint));
			int hr3 = SetWindowTheme(current, dark ? "DarkMode_Explorer" : "Explorer", null);

			if (hr1 == 0 && hr2 == 0 && hr3 == 0)
			{
				Console.WriteLine($"  - 適用: HWND=0x{current:X} dark={dark} Native_COLORREF={color:X}");
			}
			else
			{
				Console.WriteLine($"  - 適用失敗: HWND=0x{current:X} dark={dark} Native_COLORREF={color:X}");
			}

			EnumChildWindows(current, (child, lparam) =>
			{
				queue.Enqueue(child);
				return true;
			}, 0);
		}
	}

	private static bool IsValidParameterName(ParamObject paramObject)
	{
		return paramObject.Name.ToLower() is "pid" or "dark" or "color";
	}

	private static bool CheckArgs(params string[] args)
	{
		if (args.Length == 0) return false;

		Regex r = new("^/[A-Za-z]+:[a-zA-Z0-9]+$", RegexOptions.Compiled);
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
		Console.WriteLine("  /dark:<true(t) | false(f)>  タイトルバーのテーマを変更します。");
		Console.WriteLine("  /color:<RGB値(6桁) | ARGB値(8桁)>  タイトルバーの色を変更します。この値を指定するとdarkは無視されます。");
		Console.WriteLine("※colorにFFFFFFFFを設定するか、pidとdarkのみ指定するとデフォルト色にリセットされます。");
	}
}
