using Serilog;
using static SmtOrderManager.ConsoleHelper;

namespace SmtOrderManager
{
    public class OrderMenu
    {
        private readonly IDataStore _store;
        private readonly string _downloadDir;

        public OrderMenu(IDataStore store, string downloadDir)
        {
            _store = store;
            _downloadDir = downloadDir;
        }

        public void Run()
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

        public void DownloadOrder()
        {
            try
            {
                ListOrders();
                int id = ReadInt("Order ID to download: ");
                string? path = _store.DownloadOrder(id, _downloadDir);
                if (path == null)
                    Console.WriteLine("Order not found, or the download failed. See logs for details.");
                else
                    Console.WriteLine($"Order downloaded to: {path}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to download order");
                Console.WriteLine("An error occurred while downloading the order. See logs for details.");
            }
        }

        private void ListOrders()
        {
            try
            {
                var orders = _store.GetAllOrders();
                if (orders.Count == 0) { Console.WriteLine("No orders found."); return; }
                foreach (var o in orders)
                {
                    Console.WriteLine($"  [{o.Id}] {o.Name} - {o.Description} (Date: {o.OrderDate:yyyy-MM-dd})");
                    foreach (var b in o.Boards)
                        if (b != null) Console.WriteLine($"       -> [{b.Id}] {b.Name} - {b.Description} - (Length: {b.Length} mm) - (Width: {b.Width} mm)");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to list orders");
                Console.WriteLine("An error occurred while listing orders. See logs for details.");
            }
        }

        private void AddOrder()
        {
            try
            {
                var order = new Order
                {
                    Name = ReadRequiredInput("Name: "),
                    Description = ReadRequiredInput("Description: "),
                    OrderDate = ReadValidDate("Order date (yyyy-MM-dd) [today]: ")
                };
                _store.AddOrder(order);
                Console.WriteLine($"Order added with ID {order.Id}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add order");
                Console.WriteLine("An error occurred while adding the order. See logs for details.");
            }
        }

        private void EditOrder()
        {
            try
            {
                int id = ReadInt("Order ID to edit: ");
                var order = _store.GetOrder(id);
                if (order == null) { Console.WriteLine("Not found."); return; }

                Console.WriteLine($"Current: {order.Name} - {order.Description} (Date: {order.OrderDate:yyyy-MM-dd})");
                Console.WriteLine("(Press Enter to keep current value)");
                string name = ReadInput($"Name [{order.Name}]: ");
                string desc = ReadInput($"Description [{order.Description}]: ");
                string date = ReadInput($"Date [{order.OrderDate:yyyy-MM-dd}]: ");

                order.Name = string.IsNullOrEmpty(name) ? order.Name : name;
                order.Description = string.IsNullOrEmpty(desc) ? order.Description : desc;
                order.OrderDate = (DateTime.TryParse(date, out DateTime parsed)) ? parsed : order.OrderDate;

                _store.UpdateOrder(order);
                Console.WriteLine("Order updated.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to edit order");
                Console.WriteLine("An error occurred while editing the order. See logs for details.");
            }
        }

        private void SearchOrders()
        {
            try
            {
                string term = ReadInput("Search term: ");
                var results = _store.SearchOrders(term);
                if (results.Count == 0) { Console.WriteLine("No matches."); return; }
                foreach (var o in results)
                    Console.WriteLine($"  [{o.Id}] {o.Name} - {o.Description} (Date: {o.OrderDate:yyyy-MM-dd})");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search orders");
                Console.WriteLine("An error occurred while searching orders. See logs for details.");
            }
        }

        private void DeleteOrder()
        {
            try
            {
                int id = ReadInt("Order ID to delete: ");
                if (_store.GetOrder(id) == null) { Console.WriteLine("Not found."); return; }
                _store.DeleteOrder(id);
                Console.WriteLine("Order deleted.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete order");
                Console.WriteLine("An error occurred while deleting the order. See logs for details.");
            }
        }

        private void AssignBoardToOrder()
        {
            try
            {
                ListOrders();
                int orderId = ReadInt("Order ID: ");
                ListBoards();
                int boardId = ReadInt("Board ID: ");
                _store.AssignBoardToOrder(orderId, boardId);
                Console.WriteLine("Board assigned to order.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to assign board to order");
                Console.WriteLine("An error occurred while assigning the board. See logs for details.");
            }
        }

        private void RemoveBoardFromOrder()
        {
            try
            {
                ListOrders();
                int orderId = ReadInt("Order ID: ");
                int boardId = ReadInt("Board ID to remove: ");
                _store.RemoveBoardFromOrder(orderId, boardId);
                Console.WriteLine("Board removed from order.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove board from order");
                Console.WriteLine("An error occurred while removing the board. See logs for details.");
            }
        }

        private void ListBoards()
        {
            try
            {
                var boards = _store.GetAllBoards();
                if (boards.Count == 0) { Console.WriteLine("No boards found."); return; }
                foreach (var b in boards)
                {
                    Console.WriteLine($"  [{b.Id}] {b.Name} - {b.Description} - (Length: {b.Length} mm) - (Width: {b.Width} mm)");
                    foreach (var c in b.Components)
                        if (c != null) Console.WriteLine($"       -> [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to list boards");
                Console.WriteLine("An error occurred while listing boards. See logs for details.");
            }
        }
    }
}
