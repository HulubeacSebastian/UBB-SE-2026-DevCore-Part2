namespace KarmaBanking.App.Models.DTOs;

using System;

public class DepositResponseDto
{
    public decimal NewBalance { get; set; }

    public int TransactionId { get; set; }

    public DateTime Timestamp { get; set; }
}