using Try_application.Database.Entities;

public class Bookmark
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int BookId { get; set; }
    public DateTime DateAdded { get; set; }

    public Product Book { get; set; } // Navigation
}