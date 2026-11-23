using System;

[Flags]
public enum ZoneType
{
    None = 0,
    Glue = 1 << 0,
    Ice = 1 << 1,
}