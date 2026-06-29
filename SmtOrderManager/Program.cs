using Serilog;
using SmtOrderManager;
using SQLitePCL;
using static SmtOrderManager.ConsoleHelper;

raw.SetProvider(new SQLite3Provider_e_sqlite3());

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/smt-order-manager.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("SMT Order Manager started");

    string downloadDir = Path.Combine(AppContext.BaseDirectory, "Downloads");
    IDataStore store = new SqlDataStore("Data Source=smtordermanager.db");

    var componentMenu = new ComponentMenu(store);
    var boardMenu = new BoardMenu(store);
    var orderMenu = new OrderMenu(store, downloadDir);

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== SMT Order Manager ===");
        Console.WriteLine("1. Components");
        Console.WriteLine("2. Boards");
        Console.WriteLine("3. Orders");
        Console.WriteLine("4. Download Order");
        Console.WriteLine("0. Exit");

        switch (ReadInput("Choice: "))
        {
            case "1": componentMenu.Run(); break;
            case "2": boardMenu.Run(); break;
            case "3": orderMenu.Run(); break;
            case "4": orderMenu.DownloadOrder(); break;
            case "0": goto exit;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }

    exit:
    Log.Information("SMT Order Manager stopped");
}
catch (Exception ex)
{
    Log.Fatal(ex, "SMT Order Manager terminated unexpectedly");
    Console.WriteLine("A fatal error occurred and the application must close. See logs for details.");
}
finally
{
    Log.CloseAndFlush();
}
