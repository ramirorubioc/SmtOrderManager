namespace SmtOrderManager
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Quantity { get; set; }
        public List<Board> Boards { get; set; } = new();
    }

    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Length { get; set; }
        public double Width { get; set; }

        public List<Component> Components { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }

    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime OrderDate { get; set; }

        public List<Board> Boards { get; set; } = new();
    }
}