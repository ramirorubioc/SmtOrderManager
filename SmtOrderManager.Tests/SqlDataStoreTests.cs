using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace SmtOrderManager.Tests;

public class SqlDataStoreTests : IDisposable
{
    private readonly string _dbPath;
    private readonly SqlDataStore _store;

    public SqlDataStoreTests()
    {
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
        _dbPath = Path.Combine(Path.GetTempPath(), $"smt_test_{Guid.NewGuid():N}.db");
        _store = new SqlDataStore($"Data Source={_dbPath}");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [Fact]
    public void AddComponent_AssignsId_AndCanBeRetrieved()
    {
        var component = new Component { Name = "0805 Resistor", Description = "10k Ohm", Quantity = 100 };

        _store.AddComponent(component);

        Assert.True(component.Id > 0);
        var found = _store.GetComponent(component.Id);
        Assert.NotNull(found);
        Assert.Equal("0805 Resistor", found.Name);
        Assert.Equal("10k Ohm", found.Description);
        Assert.Equal(100, found.Quantity);
    }

    [Fact]
    public void UpdateComponent_ChangesValues()
    {
        var component = new Component { Name = "0402 Cap", Description = "100nF", Quantity = 50 };
        _store.AddComponent(component);

        component.Name = "0805 Cap";
        component.Description = "100nF Ceramic";
        component.Quantity = 200;
        _store.UpdateComponent(component);

        var found = _store.GetComponent(component.Id);
        Assert.NotNull(found);
        Assert.Equal("0805 Cap", found.Name);
        Assert.Equal("100nF Ceramic", found.Description);
        Assert.Equal(200, found.Quantity);
    }

    [Fact]
    public void DeleteComponent_RemovesIt()
    {
        var component = new Component { Name = "LED 0603", Description = "Red", Quantity = 10 };
        _store.AddComponent(component);

        _store.DeleteComponent(component.Id);

        Assert.Null(_store.GetComponent(component.Id));
    }

    [Fact]
    public void SearchComponents_FindsByNameOrDescription()
    {
        _store.AddComponent(new Component { Name = "0805 Resistor", Description = "SMD 10k", Quantity = 100 });
        _store.AddComponent(new Component { Name = "0402 Capacitor", Description = "Ceramic", Quantity = 50 });

        Assert.Single(_store.SearchComponents("resistor"));
        Assert.Single(_store.SearchComponents("ceramic"));
        Assert.Empty(_store.SearchComponents("inductor"));
    }

    [Fact]
    public void AssignComponentToBoard_CreatesRelationship()
    {
        var component = new Component { Name = "IC QFP-48", Description = "MCU", Quantity = 1 };
        _store.AddComponent(component);

        var board = new Board { Name = "MainBoard", Description = "MB-001", Length = 100, Width = 50 };
        _store.AddBoard(board);

        _store.AssignComponentToBoard(board.Id, component.Id);

        var loaded = _store.GetBoard(board.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Components);
        Assert.Equal(component.Id, loaded.Components[0].Id);
    }

    [Fact]
    public void RemoveComponentFromBoard_RemovesRelationship()
    {
        var component = new Component { Name = "Diode", Description = "1N4148", Quantity = 5 };
        _store.AddComponent(component);

        var board = new Board { Name = "PowerBoard", Description = "PB-001", Length = 60, Width = 40 };
        _store.AddBoard(board);

        _store.AssignComponentToBoard(board.Id, component.Id);
        _store.RemoveComponentFromBoard(board.Id, component.Id);

        var loaded = _store.GetBoard(board.Id);
        Assert.NotNull(loaded);
        Assert.Empty(loaded.Components);
    }

    [Fact]
    public void AssignBoardToOrder_CreatesRelationship()
    {
        var board = new Board { Name = "ControlBoard", Description = "CB-001", Length = 120, Width = 80 };
        _store.AddBoard(board);

        var order = new Order { Name = "Order1", Description = "Test batch", OrderDate = DateTime.Today };
        _store.AddOrder(order);

        _store.AssignBoardToOrder(order.Id, board.Id);

        var loaded = _store.GetOrder(order.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Boards);
        Assert.Equal(board.Id, loaded.Boards[0].Id);
    }

    [Fact]
    public void DownloadOrder_CreatesJsonFile()
    {
        var component = new Component { Name = "QFP-48 IC", Description = "MCU", Quantity = 1 };
        _store.AddComponent(component);

        var board = new Board { Name = "ControlBoard", Description = "CB-001", Length = 120, Width = 80 };
        _store.AddBoard(board);
        _store.AssignComponentToBoard(board.Id, component.Id);

        var order = new Order { Name = "ProdOrder", Description = "Production run", OrderDate = DateTime.Today };
        _store.AddOrder(order);
        _store.AssignBoardToOrder(order.Id, board.Id);

        string outDir = Path.Combine(Path.GetTempPath(), $"SmtTest_{Guid.NewGuid():N}");
        try
        {
            string? path = _store.DownloadOrder(order.Id, outDir);

            Assert.NotNull(path);
            Assert.True(File.Exists(path));

            string json = File.ReadAllText(path);
            Assert.Contains("ProdOrder", json);
            Assert.Contains("ControlBoard", json);
            Assert.Contains("QFP-48 IC", json);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        }
    }
}
