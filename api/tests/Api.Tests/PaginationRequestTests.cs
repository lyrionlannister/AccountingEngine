using Api.Contracts;

namespace Api.Tests;

public class PaginationRequestTests
{
    [Fact]
    public void Validate_WithNoValues_ReturnsDefaults()
    {
        var (page, pageSize) = PaginationRequest.Validate(null, null);

        Assert.Equal(1, page);
        Assert.Equal(50, pageSize);
    }

    [Fact]
    public void Validate_WithValidValues_ReturnsThem()
    {
        var (page, pageSize) = PaginationRequest.Validate(3, 20);

        Assert.Equal(3, page);
        Assert.Equal(20, pageSize);
    }

    [Fact]
    public void Validate_WithPageZero_ThrowsInvalidPaginationException()
    {
        Assert.Throws<InvalidPaginationException>(() => PaginationRequest.Validate(0, 50));
    }

    [Fact]
    public void Validate_WithPageSizeZero_ThrowsInvalidPaginationException()
    {
        Assert.Throws<InvalidPaginationException>(() => PaginationRequest.Validate(1, 0));
    }

    [Fact]
    public void Validate_WithPageSizeOverMax_ThrowsInvalidPaginationException()
    {
        Assert.Throws<InvalidPaginationException>(() => PaginationRequest.Validate(1, 101));
    }

    [Fact]
    public void Validate_WithPageSizeAtMax_ReturnsIt()
    {
        var (_, pageSize) = PaginationRequest.Validate(1, 100);

        Assert.Equal(100, pageSize);
    }
}
