namespace Cafe1316.Application.DTOs;

//用于：
//POST /api/Order（创建订单）
//GET /api/Order/{id}（查看订单详情）
public class OrderAddressDto
{
    // ❌ 不需要Id（这不是Address表的记录）
    // ❌ 不需要UserId（订单已经有UserId了）
    // ❌ 不需要Type（Order有ShippingAddress和BillingAddress字段区分）
    // ❌ 不需要IsDefault（快照不需要这个）
    
    // 只需要地址详情
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "AU";
}