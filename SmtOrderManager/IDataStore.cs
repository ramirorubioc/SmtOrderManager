namespace SmtOrderManager
{
    public interface IDataStore
    {
        // Components
        List<Component> GetAllComponents();
        Component? GetComponent(int id);
        void AddComponent(Component component);
        void UpdateComponent(Component component);
        void DeleteComponent(int id);
        List<Component> SearchComponents(string term);

        // Boards
        List<Board> GetAllBoards();
        Board? GetBoard(int id);
        void AddBoard(Board board);
        void UpdateBoard(Board board);
        void DeleteBoard(int id);
        List<Board> SearchBoards(string term);
        void AssignComponentToBoard(int boardId, int componentId);
        void RemoveComponentFromBoard(int boardId, int componentId);

        // Orders
        List<Order> GetAllOrders();
        Order? GetOrder(int id);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
        List<Order> SearchOrders(string term);
        void AssignBoardToOrder(int orderId, int boardId);
        void RemoveBoardFromOrder(int orderId, int boardId);

        // Download
        string? DownloadOrder(int orderId, string outputDirectory);
    }
}