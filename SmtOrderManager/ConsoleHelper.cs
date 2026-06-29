namespace SmtOrderManager
{
    public static class ConsoleHelper
    {
        public static string ReadInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static int ReadInt(string prompt)
        {
            Console.Write(prompt);
            int.TryParse(Console.ReadLine()?.Trim(), out int value);
            return value;
        }

        public static double ReadDouble(string prompt)
        {
            Console.Write(prompt);
            double.TryParse(Console.ReadLine()?.Trim(), out double value);
            return value;
        }

        public static DateTime ReadDate(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(input)) return DateTime.Today;
            DateTime.TryParse(input, out DateTime value);
            return value;
        }

        public static string ReadRequiredInput(string prompt)
        {
            while (true)
            {
                string value = ReadInput(prompt);
                if (!string.IsNullOrWhiteSpace(value)) return value;
                Console.WriteLine("  This field is required.");
            }
        }

        public static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine()?.Trim(), out int value) && value > 0) return value;
                Console.WriteLine("  Please enter a valid positive integer.");
            }
        }

        public static double ReadPositiveDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine()?.Trim(), out double value) && value > 0) return value;
                Console.WriteLine("  Please enter a valid positive number.");
            }
        }

        public static DateTime ReadValidDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrEmpty(input)) return DateTime.Today;
                if (DateTime.TryParse(input, out DateTime value)) return value;
                Console.WriteLine("  Please enter a valid date (yyyy-MM-dd).");
            }
        }
    }
}