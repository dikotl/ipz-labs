using System;
using System.Diagnostics;

// Max digits: 7.
while (true)
{
    try
    {
        var a = new BinaryNumber(Input("Input first 7-bit binary number"), maxBits: 7);
        var b = new BinaryNumber(Input("Input second 7-bit binary number"), maxBits: 7);
        var c = a + b;
        Console.WriteLine($"Sum: {c}");
        Console.WriteLine($"{(ulong)a} + {(ulong)b} = {(ulong)c}");
        break;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Error! {ex.Message}");
    }
}

static string Input(string prompt)
{
    Console.Write($"{prompt}: ");
    return Console.ReadLine();
}


static void Tests()
{
    {
        BinaryNumber x;

        for (var i = 0u; i <= 1_000_000; ++i)
        {
            x = new BinaryNumber(number: i);

            Debug.Assert((uint)x == i, $"i = {i}, but x = {(uint)x}");
        }

        x = new BinaryNumber(number: uint.MaxValue);

        Debug.Assert((uint)x == uint.MaxValue, $"i = {uint.MaxValue}, but x = {(uint)x}");
        Console.WriteLine("Test 1 passed");
    }

    {
        for (var i = 0u; i < 1_000_000; ++i)
            for (var j = 0u; j < 1_000_000; ++j)
            {
                var a = new BinaryNumber(number: i);
                var b = new BinaryNumber(number: j);
                var c = a + b;

                Debug.Assert((uint)c == (i + j), $"{i} + {j} == {i + j}, but {(uint)a} + {(uint)b} == {(uint)c}");
            }

        Console.WriteLine("Test 2 passed");
    }
}

static void UnitTests()
{
    {
        var i = 3u;
        var j = 3u;
        var a = new BinaryNumber(number: i);
        var b = new BinaryNumber(number: j);
        var c = a + b;

        Debug.Assert((uint)c == (i + j), $"{i} + {j} == {i + j}, but {(uint)a} + {(uint)b} == {(uint)c}");
    }
    {
        var a = new BinaryNumber(number: 0b1);
        var b = new BinaryNumber("1");
        Debug.Assert((uint)a == 0b1, $"{(uint)a} != {0b1}");
    }
    {
        var a = new BinaryNumber(number: 0b10);
        var b = new BinaryNumber("10");
        Debug.Assert((uint)a == 0b10, $"{(uint)a} != {0b10}");
    }
    {
        var a = new BinaryNumber("111");
        var b = new BinaryNumber("001");
        var c = a + b;

        Debug.Assert((uint)a == 0b111, $"{(uint)a} != {0b111}");
        Debug.Assert((uint)b == 0b001, $"{(uint)b} != {0b001}");
        Debug.Assert((uint)c == 0b111 + 0b001, $"{(uint)c} != {0b111 + 0b001}");
    }
    {
        var a = new BinaryNumber("1");
        var b = new BinaryNumber("1");
        var c = a + b;

        Debug.Assert((uint)a == 0b1, $"{(uint)a} != {0b1}");
        Debug.Assert((uint)b == 0b1, $"{(uint)b} != {0b1}");
        Debug.Assert((uint)c == 0b1 + 0b1, $"{(uint)c} != {0b1 + 0b1}");
    }
    {
        var a = new BinaryNumber("10");
        var b = new BinaryNumber("01");
        var c = a + b;

        Debug.Assert((uint)a == 0b10, $"{(uint)a} != {0b10}");
        Debug.Assert((uint)b == 0b01, $"{(uint)b} != {0b01}");
        Debug.Assert((uint)c == 0b10 + 0b01, $"{(uint)c} != {0b10 + 0b01}");
    }
    {
        var a = new BinaryNumber();
        var b = new BinaryNumber(number: 3);
        var c = a + b;

        System.Console.WriteLine(b);
        System.Console.WriteLine((uint)b);
        Debug.Assert((uint)c == 3, $"{(uint)c} != {3}");
    }
}


/*
readonly struct BinaryNumber
{
    private readonly bool[]? _bits = null;

    public readonly int Length => _bits?.Length ?? 0;
    public readonly bool Empty => _bits == null || _bits.Length == 0;

    public readonly uint Bits
    {
        get
        {
            if (_bits == null) return 0;

            var bits = 0u;

            for (var (i, zeroes) = (0, 0u); i < _bits.Length; ++i)
            {
                if (_bits[i]) (bits, zeroes) = (bits + zeroes + 1, 0);
                else ++zeroes;
            }

            return bits == 0 ? 1 : bits;
        }
    }

    public BinaryNumber()
    { }

    public BinaryNumber(int bits)
    {
        if (bits > 0) _bits = new bool[bits];
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

        if (length > 0)
        {
            _bits = new bool[length];
        }

        if (maxBits >= 0 && length > maxBits)
        {
            throw new ArgumentException(
                    $"overflow, number has more bits than expected: {length} > {maxBits}",
                    nameof(number));
        }

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
        var number = new BinaryNumber(bits: digits + 1); // overflow bit
        var overflow = false;

        for (var i = 0; i < lowest; ++i)
        {
            if (a._bits[i] == b._bits[i])
            {
                number._bits[i] = overflow;
                overflow = a._bits[i];
            }
            else
            {
                number._bits[i] = !overflow;
            }
        }

        number._bits[digits] = overflow;
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

*/
