using Api.Contracts;

namespace Api.Tests;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_ComputesCeilingOfTotalItemsOverPageSize()
    {
        var result = new PagedResult<string>(["a", "b"], Page: 1, PageSize: 50, TotalItems: 120);

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithExactMultiple_DoesNotAddExtraPage()
    {
        var result = new PagedResult<string>([], Page: 1, PageSize: 50, TotalItems: 100);

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithNoItems_IsZero()
    {
        var result = new PagedResult<string>([], Page: 1, PageSize: 50, TotalItems: 0);

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void PageBeyondTotal_ReturnsEmptyItemsWithCorrectTotalPages()
    {
        var result = new PagedResult<string>([], Page: 99, PageSize: 50, TotalItems: 120);

        Assert.Empty(result.Items);
        Assert.Equal(99, result.Page);
        Assert.Equal(3, result.TotalPages);
    }
}
