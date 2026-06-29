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
            ListOrders();
            int id = ReadInt("Order ID to download: ");
            string? path = _store.DownloadOrder(id, _downloadDir);
            if (path == null)
                Console.WriteLine("Order not found.");
            else
                Console.WriteLine($"Order downloaded to: {path}");
        }

        private void ListOrders()
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

        private void AddOrder()
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

        private void EditOrder()
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

        private void SearchOrders()
        {
            string term = ReadInput("Search term: ");
            var results = _store.SearchOrders(term);
            if (results.Count == 0) { Console.WriteLine("No matches."); return; }
            foreach (var o in results)
                Console.WriteLine($"  [{o.Id}] {o.Name} - {o.Description} (Date: {o.OrderDate:yyyy-MM-dd})");
        }

        private void DeleteOrder()
        {
            int id = ReadInt("Order ID to delete: ");
            if (_store.GetOrder(id) == null) { Console.WriteLine("Not found."); return; }
            _store.DeleteOrder(id);
            Console.WriteLine("Order deleted.");
        }

        private void AssignBoardToOrder()
        {
            ListOrders();
            int orderId = ReadInt("Order ID: ");
            ListBoards();
            int boardId = ReadInt("Board ID: ");
            _store.AssignBoardToOrder(orderId, boardId);
            Console.WriteLine("Board assigned to order.");
        }

        private void RemoveBoardFromOrder()
        {
            ListOrders();
            int orderId = ReadInt("Order ID: ");
            int boardId = ReadInt("Board ID to remove: ");
            _store.RemoveBoardFromOrder(orderId, boardId);
            Console.WriteLine("Board removed from order.");
        }

        private void ListBoards()
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
    }
}