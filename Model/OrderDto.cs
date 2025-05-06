namespace Try_application.Model
{
    public class OrderDto
    {
        public string UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
