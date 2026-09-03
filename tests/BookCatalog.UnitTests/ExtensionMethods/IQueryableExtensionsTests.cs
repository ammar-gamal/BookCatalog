using BookCatalog.API.Dtos.Common;
using BookCatalog.API.ExtensionMethods;
using FluentAssertions;

namespace BookCatalog.UnitTests.ExtensionMethods;

public class IQueryableExtensionsTests
{
    private readonly IQueryable<int> _source = Enumerable.Range(1, 25).AsQueryable();

    [Fact]
    public void ToPagedList_WhenSourceIsEmpty_ReturnsEmptyPagedList()
    {
        // Arrange
        var emptySource = Enumerable.Empty<List<int>>().AsQueryable();
        var parameters = new PaginationQueryParameters { PageIndex = 1, Limit = 10 };

        // Act
        var result = emptySource.ToPagedList(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.PageIndex.Should().Be(0);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void ToPagedList_WhenFirstPageRequested_ReturnsFirstPageAndHasNextButNoPrevious()
    {
        // Arrange
        var parameters = new PaginationQueryParameters { PageIndex = 1, Limit = 10 };

        // Act
        var result = _source.ToPagedList(parameters);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.PageIndex.Should().Be(1);
        result.Items.Should().Equal(Enumerable.Range(1, 10));
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void ToPagedList_WhenMiddlePageRequested_ReturnMiddlePageAndHasBothNextAndPrevious()
    {
        // Arrange
        var parameters = new PaginationQueryParameters { PageIndex = 2, Limit = 10 };

        // Act
        var result = _source.ToPagedList(parameters);

        // Assert
        result.PageIndex.Should().Be(2);
        result.Items.Should().Equal(Enumerable.Range(11, 10));
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void ToPagedList_WhenLastPageRequested_ReturnLastPageAndHasPreviousButNoNext()
    {
        // Arrange
        var parameters = new PaginationQueryParameters { PageIndex = 3, Limit = 10 };

        // Act
        var result = _source.ToPagedList(parameters);

        // Assert
        result.PageIndex.Should().Be(3);
        result.Items.Should().Equal(Enumerable.Range(21, 5));
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void ToPagedList_WhenPageIndexExceedsTotalPages_ReturnLastPage()
    {
        // Arrange
        var parameters = new PaginationQueryParameters { PageIndex = 999, Limit = 10 };

        // Act
        var result = _source.ToPagedList(parameters);

        // Assert
        result.PageIndex.Should().Be(3);
        result.Items.Should().Equal(Enumerable.Range(21, 5));
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }
}
