﻿using Try_application.Database.Entities;

public class CartItem
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime DateAdded { get; set; }

}