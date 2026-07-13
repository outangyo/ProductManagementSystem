using System.Collections.Generic;

namespace ProductManagementSystem.API.DTOs;

public class PaginatedProductsDto
{
    public IEnumerable<ProductDto> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
