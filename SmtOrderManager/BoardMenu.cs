using Serilog;
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

        private void AddBoard()
        {
            try
            {
                var board = new Board
                {
                    Name = ReadRequiredInput("Name: "),
                    Description = ReadRequiredInput("Description: "),
                    Length = ReadPositiveDouble("Length (mm): "),
                    Width = ReadPositiveDouble("Width (mm): ")
                };
                _store.AddBoard(board);
                Console.WriteLine($"Board added with ID {board.Id}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add board");
                Console.WriteLine("An error occurred while adding the board. See logs for details.");
            }
        }

        private void EditBoard()
        {
            try
            {
                int id = ReadInt("Board ID to edit: ");
                var board = _store.GetBoard(id);
                if (board == null) { Console.WriteLine("Not found."); return; }

                Console.WriteLine($"Current: {board.Name} - {board.Description} - (Length: {board.Length} mm) - (Width: {board.Width} mm)");
                Console.WriteLine("(Press Enter to keep current value)");
                string name = ReadInput($"Name [{board.Name}]: ");
                string description = ReadInput($"Description [{board.Description}]: ");
                string length = ReadInput($"Length [{board.Length}]: ");
                string width = ReadInput($"Width [{board.Width}]: ");

                board.Name = string.IsNullOrEmpty(name) ? board.Name : name;
                board.Description = string.IsNullOrEmpty(description) ? board.Description : description;
                board.Length = (double.TryParse(length, out double l) && l > 0) ? l : board.Length;
                board.Width = (double.TryParse(width, out double w) && w > 0) ? w : board.Width;

                _store.UpdateBoard(board);
                Console.WriteLine("Board updated.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to edit board");
                Console.WriteLine("An error occurred while editing the board. See logs for details.");
            }
        }

        private void SearchBoards()
        {
            try
            {
                string term = ReadInput("Search term: ");
                var results = _store.SearchBoards(term);
                if (results.Count == 0) { Console.WriteLine("No matches."); return; }
                foreach (var b in results)
                    Console.WriteLine($"  [{b.Id}] {b.Name} - {b.Description} - (Length: {b.Length} mm) - (Width: {b.Width} mm)");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search boards");
                Console.WriteLine("An error occurred while searching boards. See logs for details.");
            }
        }

        private void DeleteBoard()
        {
            try
            {
                int id = ReadInt("Board ID to delete: ");
                if (_store.GetBoard(id) == null) { Console.WriteLine("Not found."); return; }
                _store.DeleteBoard(id);
                Console.WriteLine("Board deleted.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete board");
                Console.WriteLine("An error occurred while deleting the board. See logs for details.");
            }
        }

        private void AssignComponentToBoard()
        {
            try
            {
                ListBoards();
                int boardId = ReadInt("Board ID: ");
                ListComponents();
                int componentId = ReadInt("Component ID: ");
                _store.AssignComponentToBoard(boardId, componentId);
                Console.WriteLine("Component assigned to board.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to assign component to board");
                Console.WriteLine("An error occurred while assigning the component. See logs for details.");
            }
        }

        private void RemoveComponentFromBoard()
        {
            try
            {
                ListBoards();
                int boardId = ReadInt("Board ID: ");
                int componentId = ReadInt("Component ID to remove: ");
                _store.RemoveComponentFromBoard(boardId, componentId);
                Console.WriteLine("Component removed from board.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove component from board");
                Console.WriteLine("An error occurred while removing the component. See logs for details.");
            }
        }

        private void ListComponents()
        {
            try
            {
                var components = _store.GetAllComponents();
                if (components.Count == 0) { Console.WriteLine("No components found."); return; }
                foreach (var c in components)
                    Console.WriteLine($"  [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to list components");
                Console.WriteLine("An error occurred while listing components. See logs for details.");
            }
        }
    }
}
