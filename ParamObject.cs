// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.0

public sealed class ParamObject
{
	public string Name { get; }
	public string Value { get; }

	public ParamObject(string arg)
	{
		var a = arg.TrimStart('/').Split(':');
		if (a.Length != 2)
		{
			Program.ShowHelpInternal(true);
			System.Environment.Exit(1);
			return;
		}
		Name = a[0];
		Value = a[1];
	}
}
