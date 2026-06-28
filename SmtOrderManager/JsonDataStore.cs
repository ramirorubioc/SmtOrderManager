using Serilog;
using System.ComponentModel;
using System.Text.Json;

namespace SmtOrderManager
{
    public class JsonDataStore : IDataStore
    {
        private class StoreData
        {
            public List<JsonComponent> Components { get; set; } = new();
            public List<JsonBoard> Boards { get; set; } = new();
            public List<JsonOrder> Orders { get; set; } = new();
            public int NextId { get; set; } = 1;
        }

        private class JsonComponent
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int Quantity { get; set; }
        }

        private class JsonBoard
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public double Length { get; set; }
            public double Width { get; set; }
            public List<int> ComponentIds { get; set; } = new();
        }

        private class JsonOrder
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public DateTime OrderDate { get; set; }
            public List<int> BoardIds { get; set; } = new();
        }

        private readonly string _filePath;
        private StoreData _data = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public JsonDataStore(string filePath)
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
            Log.Debug("Loaded data from {FilePath}", _filePath);
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_data, JsonOptions);
            File.WriteAllText(_filePath, json);
        }

        private int GetNextId() => _data.NextId++;

        private Component ToModel(JsonComponent c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Quantity = c.Quantity
        };

        private Board ToModel(JsonBoard b) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            Length = b.Length,
            Width = b.Width,
            Components = b.ComponentIds
                .Select(id => _data.Components.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .Select(c => ToModel(c!))
                .ToList()
        };

        private Order ToModel(JsonOrder o) => new()
        {
            Id = o.Id,
            Name = o.Name,
            Description = o.Description,
            OrderDate = o.OrderDate,
            Boards = o.BoardIds
                .Select(id => _data.Boards.FirstOrDefault(b => b.Id == id))
                .Where(b => b != null)
                .Select(b => ToModel(b!))
                .ToList()
        };

        public List<Component> GetAllComponents()
            => _data.Components.Select(ToModel).ToList();

        public Component? GetComponent(int id)
        {
            var c = _data.Components.Find(c => c.Id == id);
            return c == null ? null : ToModel(c);
        }

        public void AddComponent(Component component)
        {
            var dto = new JsonComponent
            {
                Id = GetNextId(),
                Name = component.Name,
                Description = component.Description,
                Quantity = component.Quantity
            };
            component.Id = dto.Id;
            _data.Components.Add(dto);
            Save();
            Log.Information("Added component:  {Name} | {Description} | {Quantity} | (ID {Id})", dto.Name, dto.Description, dto.Quantity, dto.Id);
        }

        public void UpdateComponent(Component updated)
        {
            var existing = _data.Components.Find(c => c.Id == updated.Id);
            if (existing == null) return;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Quantity = updated.Quantity;
            Save();
            Log.Information("Updated component: {Name} | {Description} | {Quantity} | (ID {Id})", existing.Name, existing.Description, existing.Quantity, existing.Id);
        }

        public void DeleteComponent(int id)
        {
            var component = _data.Components.Find(c => c.Id == id);
            if (component == null) return;
            _data.Components.RemoveAll(c => c.Id == id);
            foreach (var board in _data.Boards)
                board.ComponentIds.Remove(id);
            Save();
            Log.Information("Deleted component: (ID {Id})", id);
        }

        public List<Component> SearchComponents(string term)
        {
            string lower = term.ToLower();
            return _data.Components
                .Where(c => c.Name.ToLower().Contains(lower) || c.Description.ToLower().Contains(lower))
                .Select(ToModel)
                .ToList();
        }

        public List<Board> GetAllBoards()
            => _data.Boards.Select(ToModel).ToList();

        public Board? GetBoard(int id)
        {
            var b = _data.Boards.Find(b => b.Id == id);
            return b == null ? null : ToModel(b);
        }

        public void AddBoard(Board board)
        {
            var dto = new JsonBoard
            {
                Id = GetNextId(),
                Name = board.Name,
                Description = board.Description,
                Length = board.Length,
                Width = board.Width
            };
            board.Id = dto.Id;
            _data.Boards.Add(dto);
            Save();
            Log.Information("Added board: {Name} | {Description | {Lenght} | {Width} | (ID {Id})", dto.Name, dto.Description, dto.Length, dto.Width, dto.Id);
        }

        public void UpdateBoard(Board updated)
        {
            var existing = _data.Boards.Find(b => b.Id == updated.Id);
            if (existing == null) return;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Length = updated.Length;
            existing.Width = updated.Width;
            Save();
            Log.Information("Updated board: {Name} | {Description | {Lenght} | {Width} | (ID {Id})", existing.Name, existing.Description, existing.Length, existing.Width, existing.Id);
        }

        public void DeleteBoard(int id)
        {
            var board = _data.Boards.Find(b => b.Id == id);
            if (board == null) return;
            _data.Boards.RemoveAll(b => b.Id == id);
            foreach (var order in _data.Orders)
                order.BoardIds.Remove(id);
            Save();
            Log.Information("Deleted board: (ID {Id})",  board.Id);
        }

        public List<Board> SearchBoards(string term)
        {
            string lower = term.ToLower();
            return _data.Boards
                .Where(b => b.Name.ToLower().Contains(lower) ||
                            b.Description.ToLower().Contains(lower)).Select(ToModel)
                .ToList();
        }

        public void AssignComponentToBoard(int boardId, int componentId)
        {
            var board = _data.Boards.Find(b => b.Id == boardId);
            if (board == null || _data.Components.All(c => c.Id != componentId)) return;
            if (!board.ComponentIds.Contains(componentId))
            {
                board.ComponentIds.Add(componentId);
                Save();
                Log.Information("Assigned component {ComponentId} to board {BoardId}", componentId, boardId);
            }
        }

        public void RemoveComponentFromBoard(int boardId, int componentId)
        {
            var board = _data.Boards.Find(b => b.Id == boardId);
            if (board == null) return;
            board.ComponentIds.Remove(componentId);
            Save();
            Log.Information("Removed component {ComponentId} from board {BoardId}", componentId, boardId);
        }
        public List<Order> GetAllOrders()
            => _data.Orders.Select(ToModel).ToList();

        public Order? GetOrder(int id)
        {
            var o = _data.Orders.Find(o => o.Id == id);
            return o == null ? null : ToModel(o);
        }

        public void AddOrder(Order order)
        {
            var dto = new JsonOrder
            {
                Id = GetNextId(),
                Name = order.Name,
                Description = order.Description,
                OrderDate = order.OrderDate
            };
            order.Id = dto.Id;
            _data.Orders.Add(dto);
            Save();
            Log.Information("Added order: {Name} (ID {Id})", dto.Name, dto.Id);
        }

        public void UpdateOrder(Order updated)
        {
            var existing = _data.Orders.Find(o => o.Id == updated.Id);
            if (existing == null) return;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.OrderDate = updated.OrderDate;
            Save();
            Log.Information("Updated order: {Name} (ID {Id})", updated.Name, updated.Id);
        }

        public void DeleteOrder(int id)
        {
            var order = _data.Orders.Find(o => o.Id == id);
            if (order == null) return;
            _data.Orders.RemoveAll(o => o.Id == id);
            Save();
            Log.Information("Deleted order: {Name} (ID {Id})", order.Name, id);
        }

        public List<Order> SearchOrders(string term)
        {
            string lower = term.ToLower();
            return _data.Orders
                .Where(o => o.Name.ToLower().Contains(lower) || o.Description.ToLower().Contains(lower))
                .Select(ToModel)
                .ToList();
        }

        public void AssignBoardToOrder(int orderId, int boardId)
        {
            var order = _data.Orders.Find(o => o.Id == orderId);
            if (order == null || _data.Boards.All(b => b.Id != boardId)) return;
            if (!order.BoardIds.Contains(boardId))
            {
                order.BoardIds.Add(boardId);
                Save();
                Log.Information("Assigned board {BoardId} to order {OrderId}", boardId, orderId);
            }
        }

        public void RemoveBoardFromOrder(int orderId, int boardId)
        {
            var order = _data.Orders.Find(o => o.Id == orderId);
            if (order == null) return;
            order.BoardIds.Remove(boardId);
            Save();
            Log.Information("Removed board {BoardId} from order {OrderId}", boardId, orderId);
        }
        public string? DownloadOrder(int orderId, string outputDirectory)
        {
            var order = GetOrder(orderId);
            if (order == null)
            {
                Log.Warning("Download failed: order {OrderId} not found", orderId);
                return null;
            }

            Directory.CreateDirectory(outputDirectory);

            var manifest = new
            {
                order.Id,
                order.Name,
                order.Description,
                order.OrderDate,
                Boards = order.Boards.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Description,
                    b.Length,
                    b.Width,
                    Components = b.Components.Select(c => new { c.Id, c.Name, c.Description, c.Quantity }).ToList()
                }).ToList()
            };

            string fileName = $"Order_{order.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(outputDirectory, fileName);
            File.WriteAllText(filePath, JsonSerializer.Serialize(manifest, JsonOptions));

            Log.Information("Downloaded order {OrderId} to {FilePath}", orderId, filePath);
            return filePath;
        }
    }
}