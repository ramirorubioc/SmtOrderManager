using System.Text.Json;

namespace SmtOrderManager
{
    public class DataStore
    {
        private readonly string _filePath;
        private StoreData _data = new StoreData();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public DataStore(string filePath)
        {
            _filePath = filePath;
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _data = new StoreData();
                return;
            }

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _data = new StoreData();
                return;
            }

            _data = JsonSerializer.Deserialize<StoreData>(json, JsonOptions) ?? new StoreData();
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_data, JsonOptions);
            File.WriteAllText(_filePath, json);
        }

        private int GetNextId()
        {
            return _data.NextId++;
        }

        // ---- Components ----

        public List<Component> GetAllComponents() => _data.Components;

        public Component? GetComponent(int id) => _data.Components.Find(c => c.Id == id);

        public void AddComponent(Component component)
        {
            component.Id = GetNextId();
            _data.Components.Add(component);
            Save();
        }

        public void UpdateComponent(Component updated)
        {
            var existing = GetComponent(updated.Id);
            if (existing == null) return;

            existing.Package = updated.Package;
            existing.Description = updated.Description;
            Save();
        }

        public void DeleteComponent(int id)
        {
            var component = GetComponent(id);
            if (component == null) return;

            _data.Components.RemoveAll(c => c.Id == id);

            foreach (var board in _data.Boards)
                board.ComponentIds.Remove(id);

            Save();
        }

        public List<Component> SearchComponents(string term)
        {
            string lower = term.ToLower();
            return _data.Components.FindAll(c =>
                c.Package.ToLower().Contains(lower) ||
                c.Description.ToLower().Contains(lower));
        }

        // ---- Boards ----

        public List<Board> GetAllBoards() => _data.Boards;

        public Board? GetBoard(int id) => _data.Boards.Find(b => b.Id == id);

        public void AddBoard(Board board)
        {
            board.Id = GetNextId();
            _data.Boards.Add(board);
            Save();
        }

        public void UpdateBoard(Board updated)
        {
            var existing = GetBoard(updated.Id);
            if (existing == null) return;

            existing.Recipe = updated.Recipe;
            existing.Barcode = updated.Barcode;
            existing.Width = updated.Width;
            Save();
        }

        public void DeleteBoard(int id)
        {
            var board = GetBoard(id);
            if (board == null) return;

            _data.Boards.RemoveAll(b => b.Id == id);

            foreach (var order in _data.Orders)
                order.BoardIds.Remove(id);

            Save();
        }

        public List<Board> SearchBoards(string term)
        {
            string lower = term.ToLower();
            return _data.Boards.FindAll(b =>
                b.Recipe.ToLower().Contains(lower) ||
                b.Barcode.ToLower().Contains(lower));
        }

        public void AssignComponentToBoard(int boardId, int componentId)
        {
            var board = GetBoard(boardId);
            if (board == null || GetComponent(componentId) == null) return;
            if (board.ComponentIds.Contains(componentId)) return;

            board.ComponentIds.Add(componentId);
            Save();
        }

        public void RemoveComponentFromBoard(int boardId, int componentId)
        {
            var board = GetBoard(boardId);
            if (board == null) return;

            board.ComponentIds.Remove(componentId);
            Save();
        }

        // ---- Orders ----

        public List<Order> GetAllOrders() => _data.Orders;

        public Order? GetOrder(int id) => _data.Orders.Find(o => o.Id == id);

        public void AddOrder(Order order)
        {
            order.Id = GetNextId();
            _data.Orders.Add(order);
            Save();
        }

        public void UpdateOrder(Order updated)
        {
            var existing = GetOrder(updated.Id);
            if (existing == null) return;

            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.OrderDate = updated.OrderDate;
            Save();
        }

        public void DeleteOrder(int id)
        {
            var order = GetOrder(id);
            if (order == null) return;

            _data.Orders.RemoveAll(o => o.Id == id);
            Save();
        }

        public List<Order> SearchOrders(string term)
        {
            string lower = term.ToLower();
            return _data.Orders.FindAll(o =>
                o.Name.ToLower().Contains(lower) ||
                o.Description.ToLower().Contains(lower));
        }

        public void AssignBoardToOrder(int orderId, int boardId)
        {
            var order = GetOrder(orderId);
            if (order == null || GetBoard(boardId) == null) return;
            if (order.BoardIds.Contains(boardId)) return;

            order.BoardIds.Add(boardId);
            Save();
        }

        public void RemoveBoardFromOrder(int orderId, int boardId)
        {
            var order = GetOrder(orderId);
            if (order == null) return;

            order.BoardIds.Remove(boardId);
            Save();
        }

        // ---- Download (simulate sending to production line) ----

        public string? DownloadOrder(int orderId, string outputDirectory)
        {
            var order = GetOrder(orderId);
            if (order == null) return null;

            Directory.CreateDirectory(outputDirectory);

            var manifest = new
            {
                order.Id,
                order.Name,
                order.Description,
                order.OrderDate,
                Boards = order.BoardIds
                    .Select(bid => GetBoard(bid))
                    .Where(b => b != null)
                    .Select(b => new
                    {
                        b.Id,
                        b.Recipe,
                        b.Barcode,
                        b.Width,
                        Components = b.ComponentIds
                            .Select(cid => GetComponent(cid))
                            .Where(c => c != null)
                            .ToList()
                    })
                    .ToList()
            };

            string fileName = $"Order_{order.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(outputDirectory, fileName);
            string json = JsonSerializer.Serialize(manifest, JsonOptions);
            File.WriteAllText(filePath, json);

            return filePath;
        }
    }
}