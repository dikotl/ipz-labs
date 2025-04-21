Console.WriteLine("Type first binary number");
bool[] a = StringToBinary(Console.ReadLine()!);

Console.WriteLine("Type second binary number");
bool[] b = StringToBinary(Console.ReadLine()!);

bool[] extended = [.. new bool[b.Length], .. a];
bool[] result = new bool[2 * b.Length];

Console.WriteLine($"""

# Initial state
  B          {BinaryToString(b)}
  A          {BinaryToString(extended)}
  Result = 0 {BinaryToString(result)}

""");

for (int i = b.Length - 1; i >= 0; --i)
{
    Shr1(extended);

    if (b[i])
    {
        // Carry is always 0 because of bits shift.
        (result, _) = BinaryAdd(result, extended);
    }

    Console.WriteLine($"""
    # Step {b.Length - i}
      B      <<  {(b[i] ? '1' : '0')}|{BinaryToString(b[..i])}
      A      >>  {BinaryToString(extended)}
      Result + A {BinaryToString(result)}

    """);
}

Console.WriteLine($"Final result: {BinaryToString(result)}");
Console.WriteLine($"Final result is decimal: {BinaryToInt(result)}");

var aValue = BinaryToInt(a);
var bValue = BinaryToInt(b);
Console.WriteLine($"{aValue} * {bValue} = {aValue * bValue}");


static (bool[] sum, bool carry) BinaryAdd(bool[] a, bool[] b)
{
    var length = int.Max(a.Length, b.Length);
    var result = new bool[a.Length];
    var carry = false;

    for (var i = 0; i < length; ++i)
    {
        var A = i < a.Length && a[i];
        var B = i < b.Length && b[i];

        result[i] = A ^ B ^ carry;
        carry = (A && B) || (A && carry) || (B && carry);
    }

    return (result, carry);
}

static void Shr1(bool[] a)
{
    for (int i = 1; i < a.Length; ++i)
    {
        a[i - 1] = a[i];
    }
    a[^1] = false;
}

static bool[] StringToBinary(string number)
{
    var length = number.Length;
    var bits = length > 0 ? new bool[length] : [];

    for (int i = length - 1; i >= 0; --i)
    {
        bits[length - i - 1] = number[i] switch
        {
            '0' => false,
            '1' => true,
            _ => throw new ArgumentException(
                    $"illegal character: '{number[i]}'",
                    nameof(number)),
        };
    }

    return bits;
}

static string BinaryToString(bool[] a)
{
    System.Text.StringBuilder buf = new(a.Length);

    for (var i = 0; i < a.Length; ++i)
    {
        buf.Insert(0, a[i] ? '1' : '0');
    }

    return buf.ToString();
}

static int BinaryToInt(bool[] a)
{
    if (a.Length > sizeof(ushort) * 8) throw new ArgumentOutOfRangeException(
            nameof(a),
            $"Binary number can't be stored in int");

    ushort result = 0;

    for (var i = 0; i < a.Length; ++i)
    {
        if (a[i])
        {
            result |= (ushort)(1u << i);
        }
    }

    return result;
}
