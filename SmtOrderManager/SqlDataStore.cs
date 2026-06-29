using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SmtOrderManager
{
    public class SqlDataStore : IDataStore
    {
        private readonly string _connectionString;
        private AppDbContext Db() => new(_connectionString);

        public SqlDataStore(string connectionString)
        {
            _connectionString = connectionString;
            try
            {
                using var db = Db();
                db.Database.EnsureCreated();
                Log.Debug("Database ready");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize database with connection string {ConnectionString}", connectionString);
                throw;
            }
        }

        // ---------------------------------------------------------------
        // Components
        // ---------------------------------------------------------------

        public List<Component> GetAllComponents()
        {
            try
            {
                using var db = Db();
                return db.Components.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all components");
                return new List<Component>();
            }
        }

        public Component? GetComponent(int id)
        {
            try
            {
                using var db = Db();
                return db.Components.Find(id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve component with ID {Id}", id);
                return null;
            }
        }

        public void AddComponent(Component component)
        {
            try
            {
                using var db = Db();
                db.Add(component);
                db.SaveChanges();
                Log.Information("Added component: {Name} | {Description} | {Quantity} | (ID {Id})",
                    component.Name, component.Description, component.Quantity, component.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add component: {Name} | {Description} | {Quantity}",
                    component.Name, component.Description, component.Quantity);
                throw;
            }
        }

        public void UpdateComponent(Component component)
        {
            try
            {
                using var db = Db();
                var existing = db.Components.Find(component.Id);
                if (existing == null)
                {
                    Log.Warning("Update failed: component {Id} not found", component.Id);
                    return;
                }

                existing.Name = component.Name;
                existing.Description = component.Description;
                existing.Quantity = component.Quantity;
                db.SaveChanges();
                Log.Information("Updated component: {Name} | {Description} | {Quantity} | (ID {Id})",
                    component.Name, component.Description, component.Quantity, component.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update component with ID {Id}", component.Id);
                throw;
            }
        }

        public void DeleteComponent(int id)
        {
            try
            {
                using var db = Db();
                var existing = db.Components.Find(id);
                if (existing == null)
                {
                    Log.Warning("Delete failed: component {Id} not found", id);
                    return;
                }

                db.Remove(existing);
                db.SaveChanges();
                Log.Information("Deleted component ID {Id}", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete component with ID {Id}", id);
                throw;
            }
        }

        public List<Component> SearchComponents(string term)
        {
            try
            {
                using var db = Db();
                string lower = term.ToLower();
                return db.Components
                    .Where(c => c.Name.ToLower().Contains(lower) || c.Description.ToLower().Contains(lower))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search components with term {Term}", term);
                return new List<Component>();
            }
        }

        // ---------------------------------------------------------------
        // Boards
        // ---------------------------------------------------------------

        public List<Board> GetAllBoards()
        {
            try
            {
                using var db = Db();
                return db.Boards.Include(b => b.Components).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all boards");
                return new List<Board>();
            }
        }

        public Board? GetBoard(int id)
        {
            try
            {
                using var db = Db();
                return db.Boards.Include(b => b.Components)
                                .FirstOrDefault(b => b.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve board with ID {Id}", id);
                return null;
            }
        }

        public void AddBoard(Board board)
        {
            try
            {
                using var db = Db();
                db.Boards.Add(board);
                db.SaveChanges();
                Log.Information("Added board: {Name} | {Description} | {Length} | {Width} | (ID {Id})",
                    board.Name, board.Description, board.Length, board.Width, board.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add board: {Name} | {Description} | {Length} | {Width}",
                    board.Name, board.Description, board.Length, board.Width);
                throw;
            }
        }

        public void UpdateBoard(Board updated)
        {
            try
            {
                using var db = Db();
                var existing = db.Boards.Find(updated.Id);
                if (existing == null)
                {
                    Log.Warning("Update failed: board {Id} not found", updated.Id);
                    return;
                }

                existing.Name = updated.Name;
                existing.Description = updated.Description;
                existing.Length = updated.Length;
                existing.Width = updated.Width;
                db.SaveChanges();
                Log.Information("Updated board: {Name} | {Description} | {Length} | {Width} | (ID {Id})",
                    existing.Name, existing.Description, existing.Length, existing.Width, existing.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update board with ID {Id}", updated.Id);
                throw;
            }
        }

        public void DeleteBoard(int id)
        {
            try
            {
                using var db = Db();
                var board = db.Boards.Find(id);
                if (board == null)
                {
                    Log.Warning("Delete failed: board {Id} not found", id);
                    return;
                }

                db.Boards.Remove(board);
                db.SaveChanges();
                Log.Information("Deleted board ID {Id}", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete board with ID {Id}", id);
                throw;
            }
        }

        public List<Board> SearchBoards(string term)
        {
            try
            {
                using var db = Db();
                string lower = term.ToLower();
                return db.Boards
                    .Include(b => b.Components)
                    .Where(b => b.Name.ToLower().Contains(lower) ||
                                b.Description.ToLower().Contains(lower))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search boards with term {Term}", term);
                return new List<Board>();
            }
        }

        public void AssignComponentToBoard(int boardId, int componentId)
        {
            try
            {
                using var db = Db();
                var board = db.Boards.Include(b => b.Components)
                                     .FirstOrDefault(b => b.Id == boardId);
                var component = db.Components.Find(componentId);
                if (board == null || component == null)
                {
                    Log.Warning("Assign failed: board {BoardId} or component {ComponentId} not found", boardId, componentId);
                    return;
                }
                if (board.Components.Any(c => c.Id == componentId))
                {
                    Log.Debug("Component {ComponentId} already assigned to board {BoardId}; skipping", componentId, boardId);
                    return;
                }

                board.Components.Add(component);
                db.SaveChanges();
                Log.Information("Assigned component {ComponentId} to board {BoardId}", componentId, boardId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to assign component {ComponentId} to board {BoardId}", componentId, boardId);
                throw;
            }
        }

        public void RemoveComponentFromBoard(int boardId, int componentId)
        {
            try
            {
                using var db = Db();
                var board = db.Boards.Include(b => b.Components)
                                     .FirstOrDefault(b => b.Id == boardId);
                if (board == null)
                {
                    Log.Warning("Remove failed: board {BoardId} not found", boardId);
                    return;
                }
                var component = board.Components.FirstOrDefault(c => c.Id == componentId);
                if (component == null)
                {
                    Log.Warning("Remove failed: component {ComponentId} not assigned to board {BoardId}", componentId, boardId);
                    return;
                }

                board.Components.Remove(component);
                db.SaveChanges();
                Log.Information("Removed component {ComponentId} from board {BoardId}", componentId, boardId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove component {ComponentId} from board {BoardId}", componentId, boardId);
                throw;
            }
        }

        // ---------------------------------------------------------------
        // Orders
        // ---------------------------------------------------------------

        public List<Order> GetAllOrders()
        {
            try
            {
                using var db = Db();
                return db.Orders
                         .Include(o => o.Boards)
                         .ThenInclude(b => b.Components)
                         .ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all orders");
                return new List<Order>();
            }
        }

        public Order? GetOrder(int id)
        {
            try
            {
                using var db = Db();
                return db.Orders
                         .Include(o => o.Boards)
                         .ThenInclude(b => b.Components)
                         .FirstOrDefault(o => o.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve order with ID {Id}", id);
                return null;
            }
        }

        public void AddOrder(Order order)
        {
            try
            {
                using var db = Db();
                db.Orders.Add(order);
                db.SaveChanges();
                Log.Information("Added order: {Name} | {Description} | {OrderDate} | (ID {Id})",
                    order.Name, order.Description, order.OrderDate, order.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add order: {Name} | {Description} | {OrderDate}",
                    order.Name, order.Description, order.OrderDate);
                throw;
            }
        }

        public void UpdateOrder(Order updated)
        {
            try
            {
                using var db = Db();
                var existing = db.Orders.Find(updated.Id);
                if (existing == null)
                {
                    Log.Warning("Update failed: order {Id} not found", updated.Id);
                    return;
                }

                existing.Name = updated.Name;
                existing.Description = updated.Description;
                existing.OrderDate = updated.OrderDate;
                db.SaveChanges();
                Log.Information("Updated order: {Name} | {Description} | {OrderDate} | (ID {Id})",
                    existing.Name, existing.Description, existing.OrderDate, existing.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update order with ID {Id}", updated.Id);
                throw;
            }
        }

        public void DeleteOrder(int id)
        {
            try
            {
                using var db = Db();
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    Log.Warning("Delete failed: order {Id} not found", id);
                    return;
                }

                db.Orders.Remove(order);
                db.SaveChanges();
                Log.Information("Deleted order ID {Id}", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete order with ID {Id}", id);
                throw;
            }
        }

        public List<Order> SearchOrders(string term)
        {
            try
            {
                using var db = Db();
                string lower = term.ToLower();
                return db.Orders
                    .Where(o => o.Name.ToLower().Contains(lower) ||
                                o.Description.ToLower().Contains(lower))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search orders with term {Term}", term);
                return new List<Order>();
            }
        }

        public void AssignBoardToOrder(int orderId, int boardId)
        {
            try
            {
                using var db = Db();
                var order = db.Orders.Include(o => o.Boards)
                                     .FirstOrDefault(o => o.Id == orderId);
                var board = db.Boards.Find(boardId);
                if (order == null || board == null)
                {
                    Log.Warning("Assign failed: order {OrderId} or board {BoardId} not found", orderId, boardId);
                    return;
                }
                if (order.Boards.Any(b => b.Id == boardId))
                {
                    Log.Debug("Board {BoardId} already assigned to order {OrderId}; skipping", boardId, orderId);
                    return;
                }

                order.Boards.Add(board);
                db.SaveChanges();
                Log.Information("Assigned board {BoardId} to order {OrderId}", boardId, orderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to assign board {BoardId} to order {OrderId}", boardId, orderId);
                throw;
            }
        }

        public void RemoveBoardFromOrder(int orderId, int boardId)
        {
            try
            {
                using var db = Db();
                var order = db.Orders.Include(o => o.Boards)
                                     .FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Log.Warning("Remove failed: order {OrderId} not found", orderId);
                    return;
                }
                var board = order.Boards.FirstOrDefault(b => b.Id == boardId);
                if (board == null)
                {
                    Log.Warning("Remove failed: board {BoardId} not assigned to order {OrderId}", boardId, orderId);
                    return;
                }

                order.Boards.Remove(board);
                db.SaveChanges();
                Log.Information("Removed board {BoardId} from order {OrderId}", boardId, orderId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove board {BoardId} from order {OrderId}", boardId, orderId);
                throw;
            }
        }

        // ---------------------------------------------------------------
        // Download
        // ---------------------------------------------------------------

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public string? DownloadOrder(int orderId, string outputDirectory)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to download order {OrderId} to {OutputDirectory}", orderId, outputDirectory);
                return null;
            }
        }
    }
}
