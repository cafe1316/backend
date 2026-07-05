using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Services;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Cafe1316.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_productRepository.Object);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetProductsAsync_InvalidPagination_ThrowsBadRequestException(int page, int pageSize)
    {
        var filters = new ProductFilterParams { Page = page, PageSize = pageSize };

        Func<Task> act = () => _sut.GetProductsAsync(filters);

        await act.Should().ThrowAsync<BadRequestException>();
        _productRepository.Verify(
            repository => repository.GetProductsAsync(
                It.IsAny<ProductFilterParams>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task GetFeaturedProductsAsync_InvalidLimit_ThrowsBadRequestException(int limit)
    {
        Func<Task> act = () => _sut.GetFeaturedProductsAsync(limit);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*between 1 and 50*");
        _productRepository.Verify(
            repository => repository.GetFeaturedProductsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProductsAsync_NegativePrice_ThrowsBadRequestException()
    {
        var filters = new ProductFilterParams { MinPrice = -1 };

        Func<Task> act = () => _sut.GetProductsAsync(filters);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public async Task GetProductsAsync_MinimumPriceAboveMaximum_ThrowsBadRequestException()
    {
        var filters = new ProductFilterParams { MinPrice = 20, MaxPrice = 10 };

        Func<Task> act = () => _sut.GetProductsAsync(filters);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*cannot be greater*");
    }

    [Fact]
    public async Task GetProductsAsync_SearchTermTooLong_ThrowsBadRequestException()
    {
        var filters = new ProductFilterParams { SearchTerm = new string('a', 101) };

        Func<Task> act = () => _sut.GetProductsAsync(filters);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*100 characters*");
    }

    [Fact]
    public async Task GetProductsAsync_ValidFilters_ReturnsMappedPage()
    {
        var filters = new ProductFilterParams { Page = 2, PageSize = 10 };
        _productRepository
            .Setup(repository => repository.GetProductsAsync(filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 25));

        var result = await _sut.GetProductsAsync(filters);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        _productRepository.Verify(
            repository => repository.GetProductsAsync(filters, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
