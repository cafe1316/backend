using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.DTOs;

public class CategoryWithSubsDto
{
    public int Id { get; set; }=0;
    public string Name { get; set; }= string.Empty;
    public string Slug { get; set; }= string.Empty;
    public List<SubcategoryDto> Subcategories { get; set; }= new();
}

public class SubcategoryDto
{
    public int Id { get; set; }=0;
    public string Slug { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
}