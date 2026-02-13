namespace Cafe1316.Application.DTOs;

public class CreateCheckoutIntentDto
{
    public OrderAddressDto ShippingAddress { get; set; } = null!;
    public OrderAddressDto? BillingAddress { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
}