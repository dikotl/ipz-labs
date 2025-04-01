using System;
using System.Text;

Console.WriteLine(
    """
    1. Convert floating-point number's fractional part to binary representation
    2. Additional integer notation
    """);
Console.Write("Select task: ");

int task;

while (!int.TryParse(Console.ReadLine(), out task))
{ }

switch (task)
{
    case 1:
        Task1();
        break;
    case 2:
        Task2();
        break;
    default:
        Console.WriteLine($"Invalid task {task}");
        break;
}

static void Task1()
{
    Console.Write("Decimal number: ");
    var x = double.Parse(Console.ReadLine());

    Console.Write("Precision: ");
    var precision = int.Parse(Console.ReadLine());

    try
    {
        Console.WriteLine(DecToBin(x, precision));
    }
    catch (ArgumentException e)
    {
        Console.WriteLine($"Error! {e.Message}");
    }
}

static void Task2()
{
    const int ExpectedBits = 7;
    string input;
    bool isNeg;

    while (true)
    {
        Console.Write("Binary number: ");
        input = Console.ReadLine();
        isNeg = input[0] == '-';
        input = input.TrimStart('-');

        if (input.Length == ExpectedBits) break;

        Console.WriteLine($"Error! Expected exactly {ExpectedBits} bits");
    }

    var binary = new BinaryNumber(input, maxBits: ExpectedBits);

    if (isNeg)
    {
        // Reverse bits.
        for (int i = 0; i < ExpectedBits; ++i)
        {
            binary[i] = !binary[i];
        }

        binary += new BinaryNumber(number: 1);
    }

    var s = binary.ToString().PadLeft(ExpectedBits, '0');

    Console.WriteLine($"{(isNeg ? 1 : 0)}|{s}");
}

static string DecToBin(double x, int precision)
{
    if (x > 1 || x <= 0)
        throw new ArgumentOutOfRangeException(
            nameof(x),
            "value must be in range 0..1");

    if (precision <= 0)
        throw new ArgumentOutOfRangeException(
            nameof(precision),
            "value must be in range 1..");

    StringBuilder buf = new(precision);
    buf.Append("0.");

    for (int i = 0; i < precision; ++i)
    {
        x *= 2;
        buf.Append((int)(x %= 2));
    }

    return buf.ToString();
}
