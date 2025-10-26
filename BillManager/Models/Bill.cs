using System;

namespace BillManager.Models;

public sealed class Bill
{
    public Bill(string description, decimal price, int items, DateTime createdAt)
    {
        Description = description;
        Price = price;
        Items = items;
        CreatedAt = createdAt;
    }

    public string Description { get; }

    public decimal Price { get; }

    public int Items { get; }

    public DateTime CreatedAt { get; }
}

