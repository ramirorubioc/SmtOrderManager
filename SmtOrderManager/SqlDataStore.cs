using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmtOrderManager
{
    internal class SqlDataStore : IDataStore
    {
        private readonly string _connectionString;
        private AppDbContext Db() => new(_connectionString);

        public SqlDataStore(string connectionString)
        {
            _connectionString = connectionString;
            using var db = Db();
            db.Database.EnsureCreated();
            Log.Debug("Database ready");
        }
        public List<Component> GetAllComponents()
        {
            using var db = Db();
            return db.Components.ToList();
        }
        public Component GetComponent(int id)
        {
            using var db = Db();
            return db.Components.Find(id);
        }
        public void AddComponent(Component component)
        {
            using var db = Db();
            db.Add(component);
            db.SaveChanges();
            Log.Information("Added component: {Name} | {Description} | {Quantity} | (ID {Id})", component.Name, component.Description, component.Quantity, component.Id);
        }
        public void UpdateComponent(Component component)
        {
            using var db = Db();
            var existing = db.Components.Find(component.Id);
            if (existing == null) return;
            existing.Name = component.Name;
            existing.Description = component.Description;
            existing.Quantity = component.Quantity;
            db.SaveChanges();
            Log.Information("Updated component: {Name} | {Description} | {Quantity} | (ID {Id})", component.Name, component.Description, component.Quantity, component.Id);
        }
        public void DeleteComponent(int id)
        {
            using var db = Db();
            var existing = db.Components.Find(id);
            if (existing == null) return;
            db.Remove(existing);
            db.SaveChanges();
            Log.Information("Deleted component ID {Id}", id);
        }
        public List<Component> SearchComponents(string term)
        {
            using var db = Db();
            string lower = term.ToLower();
            return db.Components
                .Where(c => c.Name.ToLower().Contains(lower) || c.Description.ToLower().Contains(lower))
                .ToList();
        }

        public List<Board> GetAllBoards()
        {
            using var db = Db();
            return db.Boards.Include(b => b.Components).ToList();
        }

        public Board? GetBoard(int id)
        {
            using var db = Db();
            return db.Boards.Include(b => b.Components)
                            .FirstOrDefault(b => b.Id == id);
        }

        public void AddBoard(Board board)
        {
            using var db = Db();
            db.Boards.Add(board);
            db.SaveChanges();
            Log.Information("Added board: {Name} | {Description} | {Length} | {Width} | (ID {Id})", board.Name, board.Description, board.Length, board.Width, board.Id);
        }

        public void UpdateBoard(Board updated)
        {
            using var db = Db();
            var existing = db.Boards.Find(updated.Id);
            if (existing == null) return;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Length = updated.Length;
            existing.Width = updated.Width;
            db.SaveChanges();
            Log.Information("Updated board: {Name} | {Description | {Lenght} | {Width} | (ID {Id})", existing.Name, existing.Description, existing.Length, existing.Width, existing.Id);
        }

        public void DeleteBoard(int id)
        {
            using var db = Db();
            var board = db.Boards.Find(id);
            if (board == null) return;
            db.Boards.Remove(board);
            db.SaveChanges();
            Log.Information("Deleted board ID {Id}", id);
        }

        public List<Board> SearchBoards(string term)
        {
            using var db = Db();
            string lower = term.ToLower();
            return db.Boards
                .Include(b => b.Components)
                .Where(b => b.Name.ToLower().Contains(lower) ||
                            b.Description.ToLower().Contains(lower))
                .ToList();
        }
        public void AssignComponentToBoard(int boardId, int componentId)
        {
            using var db = Db();
            var board = db.Boards.Include(b => b.Components)
                                 .FirstOrDefault(b => b.Id == boardId);
            var component = db.Components.Find(componentId);
            if (board == null || component == null) return;
            if (board.Components.Any(c => c.Id == componentId)) return;

            board.Components.Add(component);
            db.SaveChanges();
            Log.Information("Assigned component {ComponentId} to board {BoardId}", componentId, boardId);
        }
        public void RemoveComponentFromBoard(int boardId, int componentId)
        {
            using var db = Db();
            var board = db.Boards.Include(b => b.Components)
                                 .FirstOrDefault(b => b.Id == boardId);
            if (board == null) return;
            var component = board.Components.FirstOrDefault(c => c.Id == componentId);
            if (component == null) return;

            board.Components.Remove(component);
            db.SaveChanges();
            Log.Information("Removed component {ComponentId} from board {BoardId}", componentId, boardId);
        }
        public List<Order> GetAllOrders()
        {
            using var db = Db();
            return db.Orders
                     .Include(o => o.Boards)
                     .ThenInclude(b => b.Components)
                     .ToList();
        }

        public Order? GetOrder(int id)
        {
            using var db = Db();
            return db.Orders
                     .Include(o => o.Boards)
                     .ThenInclude(b => b.Components)
                     .FirstOrDefault(o => o.Id == id);
        }

        public void AddOrder(Order order)
        {
            using var db = Db();
            db.Orders.Add(order);
            db.SaveChanges();
            Log.Information("Added order: {Name} (ID {Id})", order.Name, order.Id);
        }

        public void UpdateOrder(Order updated)
        {
            using var db = Db();
            var existing = db.Orders.Find(updated.Id);
            if (existing == null) return;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.OrderDate = updated.OrderDate;
            db.SaveChanges();
            Log.Information("Updated order: {Name} (ID {Id})", updated.Name, updated.Id);
        }

        public void DeleteOrder(int id)
        {
            using var db = Db();
            var order = db.Orders.Find(id);
            if (order == null) return;
            db.Orders.Remove(order);
            db.SaveChanges();
            Log.Information("Deleted order ID {Id}", id);
        }

        public List<Order> SearchOrders(string term)
        {
            using var db = Db();
            string lower = term.ToLower();
            return db.Orders
                .Where(o => o.Name.ToLower().Contains(lower) ||
                            o.Description.ToLower().Contains(lower))
                .ToList();
        }
        public void AssignBoardToOrder(int orderId, int boardId)
        {
            using var db = Db();
            var order = db.Orders.Include(o => o.Boards)
                                 .FirstOrDefault(o => o.Id == orderId);
            var board = db.Boards.Find(boardId);
            if (order == null || board == null) return;
            if (order.Boards.Any(b => b.Id == boardId)) return;

            order.Boards.Add(board);
            db.SaveChanges();
            Log.Information("Assigned board {BoardId} to order {OrderId}", boardId, orderId);
        }
        public void RemoveBoardFromOrder(int orderId, int boardId)
        {
            using var db = Db();
            var order = db.Orders.Include(o => o.Boards)
                                 .FirstOrDefault(o => o.Id == orderId);
            if (order == null) return;
            var board = order.Boards.FirstOrDefault(b => b.Id == boardId);
            if (board == null) return;

            order.Boards.Remove(board);
            db.SaveChanges();
            Log.Information("Removed board {BoardId} from order {OrderId}", boardId, orderId);
        }
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

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
                    Components = b.Components.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Quantity
                    }).ToList()
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
