namespace BookingService.Bll.DTOs;

/// <summary>
/// Запит на оплату
/// </summary>
public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}

/// <summary>
/// Запит на скасування бронювання
/// </summary>
public class CancelBookingRequest
{
    public string Reason { get; set; } = string.Empty;
}