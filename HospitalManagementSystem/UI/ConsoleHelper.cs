namespace HospitalManagementSystem.UI;

public static class ConsoleHelper
{
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 60));
        Console.WriteLine($"  {title.ToUpper()}");
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();
    }

    public static void PrintSubHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"── {title} ──");
        Console.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✔ {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✖ {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    public static void PrintSeparator() =>
        Console.WriteLine(new string('─', 60));

    public static string ReadInput(string prompt, bool required = true)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            if (!required && string.IsNullOrEmpty(input)) return string.Empty;
            if (!string.IsNullOrEmpty(input)) return input;
            PrintError("This field is required.");
        }
    }

    public static string ReadOptionalInput(string prompt)
    {
        Console.Write($"  {prompt}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                return value;
            PrintError($"Please enter a valid number between {min} and {max}.");
        }
    }

    public static decimal ReadDecimal(string prompt, decimal min = 0)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal value) && value >= min)
                return value;
            PrintError($"Please enter a valid decimal number >= {min}.");
        }
    }

    public static DateTime ReadDate(string prompt, string format = "dd/MM/yyyy")
    {
        while (true)
        {
            Console.Write($"  {prompt} ({format}): ");
            if (DateTime.TryParseExact(Console.ReadLine(), format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime date))
                return date;
            PrintError($"Invalid date. Use format {format}.");
        }
    }

    public static DateTime ReadDateTime(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt} (dd/MM/yyyy HH:mm): ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dt))
                return dt;
            PrintError("Invalid date/time. Use format dd/MM/yyyy HH:mm.");
        }
    }

    public static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        Console.WriteLine($"  {prompt}:");
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"    {i + 1}. {values[i]}");

        while (true)
        {
            Console.Write("  Choose (number): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= values.Length)
                return values[choice - 1];
            PrintError("Invalid selection.");
        }
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"  {prompt} (y/n): ");
        return Console.ReadLine()?.Trim().ToLower() == "y";
    }

    public static void PressAnyKey()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
        Console.WriteLine();
    }
}
