// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.0

public sealed class ColorObject
{
	public byte A { get; }
	public byte R { get; }
	public byte G { get; }
	public byte B { get; }
	public uint NativeColorRef { get; }

	public ColorObject(string hexColor)
	{
		if (hexColor.Length is not 6 and not 8)
		{
			Program.ShowHelpInternal(true);
			System.Environment.Exit(1);
			return;
		}
		try
		{
			if (hexColor.Length == 6)
			{
				A = 255;
				R = ParseInternal(hexColor, 0, 2);
				G = ParseInternal(hexColor, 2, 2);
				B = ParseInternal(hexColor, 4, 2);
				NativeColorRef = (uint)((B << 16) | (G << 8) | R);
			}
			else
			{
				A = ParseInternal(hexColor, 0, 2);
				R = ParseInternal(hexColor, 2, 2);
				G = ParseInternal(hexColor, 4, 2);
				B = ParseInternal(hexColor, 6, 2);
				NativeColorRef = (uint)((A << 24) | (B << 16) | (G << 8) | R);
			}
		}
		catch
		{
			Program.ShowHelpInternal(true);
			System.Environment.Exit(1);
			return;
		}
	}

	private byte ParseInternal(string s, int si, int len)
	{
		return new ByteObject(s.Substring(si, len), System.Globalization.NumberStyles.HexNumber).Value;
	}
}
