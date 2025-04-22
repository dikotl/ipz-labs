// var a = new Float8(sign: 1, exponent: 3, mantissa: 0);   // -1.0
// var b = new Float16(sign: 0, exponent: 15, mantissa: 0); // +1.0
// result ~= 0.000030517578

var a = new Float8(sign: 0, exponent: 3, mantissa: 8);     // +1.5
var b = new Float16(sign: 0, exponent: 16, mantissa: 256); // +2.25

// var a = new Float8(sign: 0, exponent: 2, mantissa: 0);     // +0.5
// var b = new Float16(sign: 0, exponent: 16, mantissa: 512); // +3.5

// var a = new Float8(sign: 1, exponent: 3, mantissa: 8);     // -1.5
// var b = new Float16(sign: 1, exponent: 16, mantissa: 512); // -3.0

// var a = new Float8(sign: 0, exponent: 1, mantissa: 0);   // -0.25
// var b = new Float16(sign: 0, exponent: 13, mantissa: 0); // -0.25

// var a = new Float8(sign: 0, exponent: 7, mantissa: 15);    // +15.875
// var b = new Float16(sign: 0, exponent: 12, mantissa: 128); // +0.125

var result = Add(a, b);

Console.WriteLine("\n==== Input data ====");
PrintFloat8(a, $"A (8-bit, {(float)a})");
PrintFloat16(b, $"B (16-bit, {(float)b})");

Console.WriteLine("\n==== Addition result ====");
PrintFloat16(result, $"Result (16-bit, {(float)result})");

static void PrintFloat8(Float8 a, string label)
{
    Console.WriteLine($"{label}: sign={a.Sign}, exponent={a.UnbiasedExponent}, mantissa=1.{Convert.ToString(a.Mantissa, 2).PadLeft(4, '0')}");
}

static void PrintFloat16(Float16 a, string label)
{
    Console.WriteLine($"{label}: sign={a.Sign}, exponent={a.UnbiasedExponent}, mantissa=1.{Convert.ToString(a.Mantissa, 2).PadLeft(10, '0')}");
}

static Float16 Add(Float8 a, Float16 b)
{
    int expA = a.UnbiasedExponent;
    int expB = b.UnbiasedExponent;

    int mantA = a.FullMantissa << 6; // Convert to 10-bit.
    int mantB = b.FullMantissa;

    int resultExp;
    int resultMant;
    int resultSign;

    // Align exponents.
    if (expA > expB)
    {
        mantB >>= expA - expB;
        resultExp = expA;
    }
    else if (expB > expA)
    {
        mantA >>= expB - expA;
        resultExp = expB;
    }
    else
    {
        resultExp = expA;
    }

    // Add/Sub mantissa.
    if (a.Sign == b.Sign)
    {
        // Signs are the same, add mantissas
        resultMant = mantA + mantB;
        resultSign = a.Sign;
    }
    else
    {
        if (mantA >= mantB)
        {
            resultMant = mantA - mantB;
            resultSign = a.Sign;
        }
        else
        {
            resultMant = mantB - mantA;
            resultSign = b.Sign;
        }
    }

    if (resultMant == 0)
    {
        return new Float16(0, 0, 0);
    }

    // Normalize result.
    while (resultMant >= (1 << 11)) // Overflow.
    {
        resultMant >>= 1;
        ++resultExp;
    }
    while (resultMant > 0 && resultMant < (1 << 10)) // IDK.
    {
        resultMant <<= 1;
        --resultExp;
    }

    int finalMant = 0;
    int finalSign = 0;
    int finalExp = 0;

    // Throw away 1, is result > 0.
    if (resultMant > 0)
    {
        finalSign = resultSign;
        finalMant = resultMant & 0b1111111111;
        finalExp = resultExp + 15;
    }

    return new Float16(finalSign, finalExp, finalMant);
}

struct Float8(int sign, int exponent, int mantissa)
{
    public byte Bits = (byte)((sign << 7)
                     | ((exponent & 0b111) << 4)
                     | (mantissa & 0b1111));

    public int Sign => (Bits & 0b1000_0000) >> 7;
    public int Exponent => (Bits & 0b111_0000) >> 4;
    public int Mantissa => Bits & 0b1111;

    private const int Bias = 3;
    private const int MantissaBits = 4;

    public int UnbiasedExponent => Exponent - Bias;
    public int FullMantissa => (1 << MantissaBits) | Mantissa;

    public override string ToString() => $"Float8({(float)this})";

    public static explicit operator float(Float8 x)
    {
        int exp = x.Exponent - 3;
        float mantissa = 1.0f + (x.Mantissa / 16.0f); // 4 біти мантиси
        return (x.Sign == 1 ? -1 : 1) * mantissa * (float)Math.Pow(2, exp);
    }
}

struct Float16(int sign, int exponent, int mantissa)
{
    public ushort Bits = (ushort)((sign << 15)
                       | ((exponent & 0b111_111) << 10)
                       | (mantissa & 0b11_1111_1111));

    public int Sign => (Bits & 0b1000_0000_0000_0000) >> 15;
    public int Exponent => (Bits & 0b111_1110_0000_0000) >> 10;
    public int Mantissa => Bits & 0b11_1111_1111;

    private const int Bias = 15;
    private const int MantissaBits = 10;

    public int UnbiasedExponent => Exponent - Bias;
    public int FullMantissa => (1 << MantissaBits) | Mantissa;

    public override string ToString() => $"Float16({(float)this})";

    public static explicit operator float(Float16 x)
    {
        int exponent = x.Exponent - 15;
        float mantissa = 1.0f + (x.Mantissa / 1024.0f); // 10 бітів мантиси
        return (x.Sign == 1 ? -1 : 1) * mantissa * (float)Math.Pow(2, exponent);
    }
}
