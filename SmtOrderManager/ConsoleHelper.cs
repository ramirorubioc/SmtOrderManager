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
    }
}
