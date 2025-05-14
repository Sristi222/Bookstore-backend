namespace Try_application.Database.Entities
{
    public class Banner
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }  // ✅ NEW for activation

        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
