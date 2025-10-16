// Copyright (c) 2025 rk0exn All rights reserved.
// DWM_Run v1.0

public sealed class BooleanObject(string arg)
{
	public bool Value { get; } = string.Equals(arg, "TRUE", System.StringComparison.OrdinalIgnoreCase);
}
