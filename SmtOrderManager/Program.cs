using SmtOrderManager;

Console.WriteLine("=== SMT Order Manager ===");

string dataFile = Path.Combine(AppContext.BaseDirectory, "data.json");
var store = new DataStore(dataFile);

Console.WriteLine("DataStore initialized. JSON persistence active.");
Console.WriteLine();

// Quick smoke test
var testComponent = new Component { Package = "0805 Resistor", Description = "10k Ohm" };
store.AddComponent(testComponent);
Console.WriteLine($"Added component: {testComponent.Package} (ID {testComponent.Id})");

var found = store.GetComponent(testComponent.Id);
Console.WriteLine($"Retrieved component: {found?.Package} - {found?.Description}");

store.DeleteComponent(testComponent.Id);
Console.WriteLine($"Deleted component ID {testComponent.Id}");

var all = store.GetAllComponents();
Console.WriteLine($"Components remaining: {all.Count}");

Console.WriteLine();
Console.WriteLine("DataStore CRUD operations working. UI coming next.");