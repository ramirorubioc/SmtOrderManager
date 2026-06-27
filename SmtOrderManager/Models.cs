namespace SmtOrderManager
{
    public class Component
    {
        public int Id { get; set; }
        public string Package { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class Board
    {
        public int Id { get; set; }
        public string Recipe { get; set; } = "";
        public string Description { get; set; } = "";
        public double Width { get; set; }
        public List<int> ComponentIds { get; set; } = new List<int>();
    }

    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public List<int> BoardIds { get; set; } = new List<int>();
    }

    public class StoreData
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<Component> Components { get; set; } = new List<Component>();
        public int NextId { get; set; } = 1;
    }
}
