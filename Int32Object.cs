// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.1

using System;

public sealed class Int32Object
{
	public int Value { get; }

	public Int32Object(string arg)
	{
		if (int.TryParse(arg, out var res))
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
