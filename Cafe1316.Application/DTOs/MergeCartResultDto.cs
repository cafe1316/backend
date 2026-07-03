namespace Cafe1316.Application.DTOs;

public class MergeCartResultDto
{
    public CartDto Cart { get; set; } = new();
    public List<RejectedCartItemDto> RejectedItems { get; set; } = new();
}

public class RejectedCartItemDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int RequestedQuantity { get; set; }
    public int? AvailableQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
