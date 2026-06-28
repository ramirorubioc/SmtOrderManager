using SmtOrderManager;

Console.WriteLine("SMT Order Manager started");

string dataFile = Path.Combine(AppContext.BaseDirectory, "data.json");
string downloadDir = Path.Combine(AppContext.BaseDirectory, "Downloads");
var store = new DataStore(dataFile);

RunMainMenu();

Console.WriteLine("SMT Order Manager stopped");

// ============================================================
// Main Menu
// ============================================================

void RunMainMenu()
{
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
            case "1": RunComponentMenu(); break;
            case "2": RunBoardMenu(); break;
            case "3": RunOrderMenu(); break;
            case "4": DownloadOrder(); break;
            case "0": return;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
}

// ============================================================
// Components
// ============================================================

void RunComponentMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("--- Components ---");
        Console.WriteLine("1. List all");
        Console.WriteLine("2. Add");
        Console.WriteLine("3. Edit");
        Console.WriteLine("4. Search");
        Console.WriteLine("5. Delete");
        Console.WriteLine("0. Back");

        switch (ReadInput("Choice: "))
        {
            case "1": ListComponents(); break;
            case "2": AddComponent(); break;
            case "3": EditComponent(); break;
            case "4": SearchComponents(); break;
            case "5": DeleteComponent(); break;
            case "0": return;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
}

void ListComponents()
{
    var components = store.GetAllComponents();
    if (components.Count == 0)
    {
        Console.WriteLine("No components found.");
        return;
    }
    foreach (var c in components)
        Console.WriteLine($"  [{c.Id}] {c.Package} - {c.Description}");
}

void AddComponent()
{
    var component = new Component
    {
        Package = ReadInput("Package: "),
        Description = ReadInput("Description: ")
    };
    store.AddComponent(component);
    Console.WriteLine($"Component added with ID {component.Id}.");
}

void EditComponent()
{
    int id = ReadInt("Component ID to edit: ");
    var component = store.GetComponent(id);
    if (component == null) { Console.WriteLine("Not found."); return; }

    Console.WriteLine($"Current: {component.Package} - {component.Description}");
    string pkg = ReadInput($"Package [{component.Package}]: ");
    string desc = ReadInput($"Description [{component.Description}]: ");

    component.Package = string.IsNullOrEmpty(pkg) ? component.Package : pkg;
    component.Description = string.IsNullOrEmpty(desc) ? component.Description : desc;
    store.UpdateComponent(component);
    Console.WriteLine("Component updated.");
}

void SearchComponents()
{
    string term = ReadInput("Search term: ");
    var results = store.SearchComponents(term);
    if (results.Count == 0) { Console.WriteLine("No matches."); return; }
    foreach (var c in results)
        Console.WriteLine($"  [{c.Id}] {c.Package} - {c.Description}");
}

void DeleteComponent()
{
    int id = ReadInt("Component ID to delete: ");
    if (store.GetComponent(id) == null) { Console.WriteLine("Not found."); return; }
    store.DeleteComponent(id);
    Console.WriteLine("Component deleted.");
}

// ============================================================
// Boards
// ============================================================

void RunBoardMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("--- Boards ---");
        Console.WriteLine("1. List all");
        Console.WriteLine("2. Add");
        Console.WriteLine("3. Edit");
        Console.WriteLine("4. Search");
        Console.WriteLine("5. Delete");
        Console.WriteLine("6. Assign component to board");
        Console.WriteLine("7. Remove component from board");
        Console.WriteLine("0. Back");

        switch (ReadInput("Choice: "))
        {
            case "1": ListBoards(); break;
            case "2": AddBoard(); break;
            case "3": EditBoard(); break;
            case "4": SearchBoards(); break;
            case "5": DeleteBoard(); break;
            case "6": AssignComponentToBoard(); break;
            case "7": RemoveComponentFromBoard(); break;
            case "0": return;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
}

void ListBoards()
{
    var boards = store.GetAllBoards();
    if (boards.Count == 0) { Console.WriteLine("No boards found."); return; }
    foreach (var b in boards)
    {
        Console.WriteLine($"  [{b.Id}] {b.Recipe} - Barcode: {b.Barcode} (Width: {b.Width} mm)");
        if (b.ComponentIds.Count > 0)
        {
            foreach (int cid in b.ComponentIds)
            {
                var c = store.GetComponent(cid);
                if (c != null) Console.WriteLine($"       -> Component: {c.Package}");
            }
        }
    }
}

void AddBoard()
{
    var board = new Board
    {
        Recipe = ReadInput("Recipe: "),
        Barcode = ReadInput("Barcode: "),
        Width = ReadDouble("Width (mm): ")
    };
    store.AddBoard(board);
    Console.WriteLine($"Board added with ID {board.Id}.");
}

void EditBoard()
{
    int id = ReadInt("Board ID to edit: ");
    var board = store.GetBoard(id);
    if (board == null) { Console.WriteLine("Not found."); return; }

    Console.WriteLine($"Current: {board.Recipe} - Barcode: {board.Barcode} (Width: {board.Width} mm)");
    string recipe = ReadInput($"Recipe [{board.Recipe}]: ");
    string barcode = ReadInput($"Barcode [{board.Barcode}]: ");
    string width = ReadInput($"Width [{board.Width}]: ");

    board.Recipe = string.IsNullOrEmpty(recipe) ? board.Recipe : recipe;
    board.Barcode = string.IsNullOrEmpty(barcode) ? board.Barcode : barcode;
    board.Width = string.IsNullOrEmpty(width) ? board.Width : double.Parse(width);
    store.UpdateBoard(board);
    Console.WriteLine("Board updated.");
}

void SearchBoards()
{
    string term = ReadInput("Search term: ");
    var results = store.SearchBoards(term);
    if (results.Count == 0) { Console.WriteLine("No matches."); return; }
    foreach (var b in results)
        Console.WriteLine($"  [{b.Id}] {b.Recipe} - Barcode: {b.Barcode} (Width: {b.Width} mm)");
}

void DeleteBoard()
{
    int id = ReadInt("Board ID to delete: ");
    if (store.GetBoard(id) == null) { Console.WriteLine("Not found."); return; }
    store.DeleteBoard(id);
    Console.WriteLine("Board deleted.");
}

void AssignComponentToBoard()
{
    ListBoards();
    int boardId = ReadInt("Board ID: ");
    ListComponents();
    int componentId = ReadInt("Component ID: ");
    store.AssignComponentToBoard(boardId, componentId);
    Console.WriteLine("Component assigned to board.");
}

void RemoveComponentFromBoard()
{
    ListBoards();
    int boardId = ReadInt("Board ID: ");
    int componentId = ReadInt("Component ID to remove: ");
    store.RemoveComponentFromBoard(boardId, componentId);
    Console.WriteLine("Component removed from board.");
}

// ============================================================
// Orders
// ============================================================

void RunOrderMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("--- Orders ---");
        Console.WriteLine("1. List all");
        Console.WriteLine("2. Add");
        Console.WriteLine("3. Edit");
        Console.WriteLine("4. Search");
        Console.WriteLine("5. Delete");
        Console.WriteLine("6. Assign board to order");
        Console.WriteLine("7. Remove board from order");
        Console.WriteLine("0. Back");

        switch (ReadInput("Choice: "))
        {
            case "1": ListOrders(); break;
            case "2": AddOrder(); break;
            case "3": EditOrder(); break;
            case "4": SearchOrders(); break;
            case "5": DeleteOrder(); break;
            case "6": AssignBoardToOrder(); break;
            case "7": RemoveBoardFromOrder(); break;
            case "0": return;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
}

void ListOrders()
{
    var orders = store.GetAllOrders();
    if (orders.Count == 0) { Console.WriteLine("No orders found."); return; }
    foreach (var o in orders)
    {
        Console.WriteLine($"  [{o.Id}] {o.Name} - {o.Description} (Date: {o.OrderDate:yyyy-MM-dd})");
        if (o.BoardIds.Count > 0)
        {
            foreach (int bid in o.BoardIds)
            {
                var b = store.GetBoard(bid);
                if (b != null) Console.WriteLine($"       -> Board: {b.Recipe} (Width: {b.Width} mm)");
            }
        }
    }
}

void AddOrder()
{
    var order = new Order
    {
        Name = ReadInput("Name: "),
        Description = ReadInput("Description: "),
        OrderDate = ReadDate("Order date (yyyy-MM-dd) [today]: ")
    };
    store.AddOrder(order);
    Console.WriteLine($"Order added with ID {order.Id}.");
}

void EditOrder()
{
    int id = ReadInt("Order ID to edit: ");
    var order = store.GetOrder(id);
    if (order == null) { Console.WriteLine("Not found."); return; }

    Console.WriteLine($"Current: {order.Name} - {order.Description} (Date: {order.OrderDate:yyyy-MM-dd})");
    string name = ReadInput($"Name [{order.Name}]: ");
    string desc = ReadInput($"Description [{order.Description}]: ");
    string date = ReadInput($"Date [{order.OrderDate:yyyy-MM-dd}]: ");

    order.Name = string.IsNullOrEmpty(name) ? order.Name : name;
    order.Description = string.IsNullOrEmpty(desc) ? order.Description : desc;
    order.OrderDate = string.IsNullOrEmpty(date) ? order.OrderDate : DateTime.Parse(date);
    store.UpdateOrder(order);
    Console.WriteLine("Order updated.");
}

void SearchOrders()
{
    string term = ReadInput("Search term: ");
    var results = store.SearchOrders(term);
    if (results.Count == 0) { Console.WriteLine("No matches."); return; }
    foreach (var o in results)
        Console.WriteLine($"  [{o.Id}] {o.Name} - {o.Description} (Date: {o.OrderDate:yyyy-MM-dd})");
}

void DeleteOrder()
{
    int id = ReadInt("Order ID to delete: ");
    if (store.GetOrder(id) == null) { Console.WriteLine("Not found."); return; }
    store.DeleteOrder(id);
    Console.WriteLine("Order deleted.");
}

void AssignBoardToOrder()
{
    ListOrders();
    int orderId = ReadInt("Order ID: ");
    ListBoards();
    int boardId = ReadInt("Board ID: ");
    store.AssignBoardToOrder(orderId, boardId);
    Console.WriteLine("Board assigned to order.");
}

void RemoveBoardFromOrder()
{
    ListOrders();
    int orderId = ReadInt("Order ID: ");
    int boardId = ReadInt("Board ID to remove: ");
    store.RemoveBoardFromOrder(orderId, boardId);
    Console.WriteLine("Board removed from order.");
}

// ============================================================
// Download Order
// ============================================================

void DownloadOrder()
{
    ListOrders();
    int id = ReadInt("Order ID to download: ");
    string path = store.DownloadOrder(id, downloadDir);
    if (path == null)
        Console.WriteLine("Order not found.");
    else
        Console.WriteLine($"Order downloaded to: {path}");
}

// ============================================================
// Input Helpers
// ============================================================

string ReadInput(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim() ?? "";
}

int ReadInt(string prompt)
{
    Console.Write(prompt);
    int.TryParse(Console.ReadLine()?.Trim(), out int value);
    return value;
}

double ReadDouble(string prompt)
{
    Console.Write(prompt);
    double.TryParse(Console.ReadLine()?.Trim(), out double value);
    return value;
}

DateTime ReadDate(string prompt)
{
    Console.Write(prompt);
    string input = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(input)) return DateTime.Today;
    DateTime.TryParse(input, out DateTime value);
    return value;
}
