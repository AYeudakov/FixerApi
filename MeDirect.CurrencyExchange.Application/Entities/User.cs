﻿using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618
namespace CurrencyExchange.Application.Entities;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}