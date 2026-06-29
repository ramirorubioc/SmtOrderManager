using Serilog;
using static SmtOrderManager.ConsoleHelper;

namespace SmtOrderManager
{
    public class ComponentMenu
    {
        private readonly IDataStore _store;

        public ComponentMenu(IDataStore store)
        {
            _store = store;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("--- Components ---");
                Console.WriteLine("1. List all");
                Console.WriteLine("2. Add");
                Console.WriteLine("3. Edit");
                Console.WriteLine("4. Search");
                Console.WriteLine("5. Delete");
                Console.WriteLine("0. Back");

                switch (ReadInput("Choice: "))
                {
                    case "1": ListComponents(); break;
                    case "2": AddComponent(); break;
                    case "3": EditComponent(); break;
                    case "4": SearchComponents(); break;
                    case "5": DeleteComponent(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        public void ListComponents()
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

        private void AddComponent()
        {
            try
            {
                var component = new Component
                {
                    Name = ReadRequiredInput("Name: "),
                    Description = ReadRequiredInput("Description: "),
                    Quantity = ReadPositiveInt("Quantity: ")
                };
                _store.AddComponent(component);
                Console.WriteLine($"Component added with ID {component.Id}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add component");
                Console.WriteLine("An error occurred while adding the component. See logs for details.");
            }
        }

        private void EditComponent()
        {
            try
            {
                int id = ReadInt("Component ID to edit: ");
                var component = _store.GetComponent(id);
                if (component == null) { Console.WriteLine("Not found."); return; }

                Console.WriteLine($"Current: {component.Name} - {component.Description} - {component.Quantity}");
                Console.WriteLine("(Press Enter to keep current value)");
                string name = ReadInput($"Name [{component.Name}]: ");
                string desc = ReadInput($"Description [{component.Description}]: ");
                string qtyInput = ReadInput($"Quantity [{component.Quantity}]: ");

                component.Name = string.IsNullOrEmpty(name) ? component.Name : name;
                component.Description = string.IsNullOrEmpty(desc) ? component.Description : desc;
                component.Quantity = (int.TryParse(qtyInput, out int qty) && qty > 0) ? qty : component.Quantity;

                _store.UpdateComponent(component);
                Console.WriteLine("Component updated.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to edit component");
                Console.WriteLine("An error occurred while editing the component. See logs for details.");
            }
        }

        private void SearchComponents()
        {
            try
            {
                string term = ReadInput("Search term: ");
                var results = _store.SearchComponents(term);
                if (results.Count == 0) { Console.WriteLine("No matches."); return; }
                foreach (var c in results)
                    Console.WriteLine($"  [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search components");
                Console.WriteLine("An error occurred while searching components. See logs for details.");
            }
        }

        private void DeleteComponent()
        {
            try
            {
                int id = ReadInt("Component ID to delete: ");
                if (_store.GetComponent(id) == null) { Console.WriteLine("Not found."); return; }
                _store.DeleteComponent(id);
                Console.WriteLine("Component deleted.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete component");
                Console.WriteLine("An error occurred while deleting the component. See logs for details.");
            }
        }
    }
}
