using System;
using System.Text;

public struct BinaryNumber
{
    private bool[] _bits;

    public readonly int Length => _bits.Length;
    public readonly bool Empty => _bits.Length == 0;

    public readonly uint Bits
    {
        get
        {
            var bits = 0u;

            for (var (i, zeroes) = (0, 0u); i < _bits.Length; ++i)
            {
                if (_bits[i]) (bits, zeroes) = (bits + zeroes + 1, 0);
                else ++zeroes;
            }

            return bits == 0 ? 1 : bits;
        }
    }

    public bool this[int i]
    {
        readonly get
        {
            return _bits[i];
        }
        set
        {
            if (i > Bits)
            {
                Array.Resize(ref _bits, i);
            }
            _bits[i] = value;
        }
    }

    public BinaryNumber()
    {
        _bits = [];
    }

    public BinaryNumber(int bits)
    {
        _bits = bits > 0 ? new bool[bits] : [];
    }

    public BinaryNumber(uint number)
    {
        _bits = new bool[sizeof(uint) * 8];

        for (var i = 0; i < Length; ++i)
        {
            _bits[i] = (number & 1) == 1;
            number >>= 1;
        }
    }

    public BinaryNumber(string number, int maxBits = -1)
    {
        int length = number.Length;

        if (maxBits >= 0 && length > maxBits)
        {
            throw new ArgumentException(
                    $"overflow, number has more bits than expected: {length} > {maxBits}",
                    nameof(number));
        }

        _bits = length > 0 ? new bool[length] : [];

        for (int i = length - 1; i >= 0; --i)
        {
            _bits[length - i - 1] = number[i] switch
            {
                '0' => false,
                '1' => true,
                _ => throw new ArgumentException(
                        $"illegal character: '{number[i]}'",
                        nameof(number)),
            };
        }

        // TODO: maybe it's worth to trim `_bits`
    }

    public override string ToString()
    {
        if (_bits == null) return "0";

        StringBuilder buf = new((int)Bits);

        for (var (i, zeroes) = (0, 0); i < Length; ++i)
        {
            if (_bits[i])
            {
                buf.Insert(0, "0", zeroes);
                buf.Insert(0, '1');
                zeroes = 0;
            }
            else
            {
                ++zeroes;
            }
        }

        if (buf.Length == 0) return "0";

        return buf.ToString();
    }

    public static BinaryNumber operator +(BinaryNumber a, BinaryNumber b)
    {
        if (a._bits == null) return b;
        if (b._bits == null) return a;

        var lowest = int.Min(a.Length, b.Length);
        var digits = int.Max(a.Length, b.Length);
        var number = new BinaryNumber(bits: digits + 1 /* overflow bit */);
        var carry = false;

        for (var i = 0; i < lowest; ++i)
        {
            if (a._bits[i] == b._bits[i])
            {
                number._bits[i] = carry;
                carry = a._bits[i];
            }
            else
            {
                number._bits[i] = !carry;
            }
        }

        number._bits[digits] = carry;
        return number;
    }

    public static explicit operator ushort(BinaryNumber x)
    {
        if (x.Bits > sizeof(ushort) * 8) throw new ArgumentOutOfRangeException(
            nameof(x),
            $"Binary number can't be stored in ushort: '{x}', it has {x.Bits} bits");
        ushort result = 0;
        for (var i = 0; i < x.Length; ++i) if (x._bits[i]) result |= (ushort)(1u << i);
        return result;
    }

    public static explicit operator uint(BinaryNumber x)
    {
        if (x.Bits > sizeof(uint) * 8) throw new ArgumentOutOfRangeException(
            nameof(x),
            $"Binary number can't be stored in uint: '{x}', it has {x.Bits} bits");
        uint result = 0;
        for (var i = 0; i < x.Length; ++i)
            if (x._bits[i]) result |= 1u << i;
        return result;
    }

    public static explicit operator ulong(BinaryNumber x)
    {
        if (x.Bits > sizeof(ulong) * 8) throw new ArgumentOutOfRangeException(
            nameof(x),
            $"Binary number can't be stored in ulong: '{x}', it has {x.Bits} bits");
        ulong result = 0;
        for (var i = 0; i < x.Length; ++i)
            if (x._bits[i]) result |= 1ul << i;
        return result;
    }
}
