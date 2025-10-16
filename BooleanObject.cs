// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.1

using System;

public sealed class BooleanObject
{
	public bool Value { get; }

	public BooleanObject(string arg)
	{
		if (arg.ToLower() is "true" or "t")
		{
			Value = true;
		}
		else if (arg.ToLower() is "false" or "f")
		{
			Value = false;
		}
		else
		{
			Program.ShowHelpInternal(true);
			Environment.Exit(1);
			return;
		}
	}
}
