#nullable enable
using System;

public readonly struct HexCoord : IEquatable<HexCoord>
{
    readonly int q;
    readonly int r;

    public int Q => q;
    public int R => r;
    public int S => -q - r;

    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public bool Equals(HexCoord other)
        => q == other.q && r == other.r;

    public override bool Equals(object? obj)
        => obj is HexCoord other && Equals(other);

    public override int GetHashCode()
        => (q * 397) ^ r;

    public override string ToString()
        => $"({q}, {r})";

    public static bool operator ==(HexCoord left, HexCoord right)
        => left.Equals(right);

    public static bool operator !=(HexCoord left, HexCoord right)
        => !left.Equals(right);

    public static HexCoord operator +(HexCoord left, HexCoord right)
        => new(left.q + right.q, left.r + right.r);
}
