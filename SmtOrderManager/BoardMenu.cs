using static SmtOrderManager.ConsoleHelper;

namespace SmtOrderManager
{
    public class BoardMenu
    {
        private readonly IDataStore _store;

        public BoardMenu(IDataStore store)
        {
            _store = store;
        }

        public void Run()
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

        public void ListBoards()
        {
            var boards = _store.GetAllBoards();
            if (boards.Count == 0) { Console.WriteLine("No boards found."); return; }
            foreach (var b in boards)
            {
                Console.WriteLine($"  [{b.Id}] {b.Name} - {b.Description} - (Length: {b.Length} mm) - (Width: {b.Width} mm)");
                foreach (var c in b.Components)
                {
                    if (c != null) Console.WriteLine($"       -> [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
                }
            }
        }

        private void AddBoard()
        {
            var board = new Board
            {
                Name = ReadInput("Name: "),
                Description = ReadInput("Description: "),
                Length = ReadDouble("Length (mm): "),
                Width = ReadDouble("Width (mm): ")
            };
            _store.AddBoard(board);
            Console.WriteLine($"Board added with ID {board.Id}.");
        }

        private void EditBoard()
        {
            int id = ReadInt("Board ID to edit: ");
            var board = _store.GetBoard(id);
            if (board == null) { Console.WriteLine("Not found."); return; }

            Console.WriteLine($"Current: {board.Name} - {board.Description} - (Length: {board.Length} mm) - (Width: {board.Width} mm)");
            string name = ReadInput($"Name [{board.Name}]: ");
            string description = ReadInput($"Barcode [{board.Description}]: ");
            string length = ReadInput($"Length [{board.Length}]: ");
            string width = ReadInput($"Width [{board.Width}]: ");

            board.Name = string.IsNullOrEmpty(name) ? board.Name : name;
            board.Description = string.IsNullOrEmpty(description) ? board.Description : description;
            board.Length = string.IsNullOrEmpty(length) ? board.Length : double.Parse(length);
            board.Width = string.IsNullOrEmpty(width) ? board.Width : double.Parse(width);
            _store.UpdateBoard(board);
            Console.WriteLine("Board updated.");
        }

        private void SearchBoards()
        {
            string term = ReadInput("Search term: ");
            var results = _store.SearchBoards(term);
            if (results.Count == 0) { Console.WriteLine("No matches."); return; }
            foreach (var b in results)
                Console.WriteLine($"  [{b.Id}] {b.Name} - {b.Description} - (Length: {b.Length} mm) - (Width: {b.Width} mm)");
        }

        private void DeleteBoard()
        {
            int id = ReadInt("Board ID to delete: ");
            if (_store.GetBoard(id) == null) { Console.WriteLine("Not found."); return; }
            _store.DeleteBoard(id);
            Console.WriteLine("Board deleted.");
        }

        private void AssignComponentToBoard()
        {
            ListBoards();
            int boardId = ReadInt("Board ID: ");
            ListComponents();
            int componentId = ReadInt("Component ID: ");
            _store.AssignComponentToBoard(boardId, componentId);
            Console.WriteLine("Component assigned to board.");
        }

        private void RemoveComponentFromBoard()
        {
            ListBoards();
            int boardId = ReadInt("Board ID: ");
            int componentId = ReadInt("Component ID to remove: ");
            _store.RemoveComponentFromBoard(boardId, componentId);
            Console.WriteLine("Component removed from board.");
        }

        private void ListComponents()
        {
            var components = _store.GetAllComponents();
            if (components.Count == 0)
            {
                Console.WriteLine("No components found.");
                return;
            }
            foreach (var c in components)
                Console.WriteLine($"  [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
        }
    }
}
