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
            var components = _store.GetAllComponents();
            if (components.Count == 0)
            {
                Console.WriteLine("No components found.");
                return;
            }
            foreach (var c in components)
                Console.WriteLine($"  [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
        }

        private void AddComponent()
        {
            var component = new Component
            {
                Name = ReadInput("Name: "),
                Description = ReadInput("Description: "),
                Quantity = ReadInt("Quantity: ")
            };
            _store.AddComponent(component);
            Console.WriteLine($"Component added with ID {component.Id}.");
        }

        private void EditComponent()
        {
            int id = ReadInt("Component ID to edit: ");
            var component = _store.GetComponent(id);
            if (component == null) { Console.WriteLine("Not found."); return; }

            Console.WriteLine($"Current: {component.Name} - {component.Description} - {component.Quantity}");
            string nme = ReadInput($"Name [{component.Name}]: ");
            string desc = ReadInput($"Description [{component.Description}]: ");
            int qnt = ReadInt($"Description [{component.Quantity}]: ");

            component.Name = string.IsNullOrEmpty(nme) ? component.Name : nme;
            component.Description = string.IsNullOrEmpty(desc) ? component.Description : desc;
            component.Quantity = qnt;
            _store.UpdateComponent(component);
            Console.WriteLine("Component updated.");
        }

        private void SearchComponents()
        {
            string term = ReadInput("Search term: ");
            var results = _store.SearchComponents(term);
            if (results.Count == 0) { Console.WriteLine("No matches."); return; }
            foreach (var c in results)
                Console.WriteLine($"  [{c.Id}] {c.Name} - {c.Description} - {c.Quantity}");
        }

        private void DeleteComponent()
        {
            int id = ReadInt("Component ID to delete: ");
            if (_store.GetComponent(id) == null) { Console.WriteLine("Not found."); return; }
            _store.DeleteComponent(id);
            Console.WriteLine("Component deleted.");
        }
    }
}
