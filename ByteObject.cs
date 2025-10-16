// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.1

using System;
using System.Globalization;

public sealed class ByteObject
{
	public byte Value { get; }

	public ByteObject(string arg, NumberStyles style)
	{
		if (byte.TryParse(arg, style, null, out var res))
		{
			Value = res;
		}
		else
		{
			Program.ShowHelpInternal(true);
			Environment.Exit(1);
			return;
		}
	}
}
