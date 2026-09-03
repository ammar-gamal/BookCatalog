using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Entities;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services;
using BookCatalog.API.Utilities.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace BookCatalog.UnitTests.Services;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IBookCopyRepository> _bookCopyRepositoryMock;
    private readonly Mock<IBaseRepository<User>> _userRepositoryMock;
    private readonly Mock<ILogger<LoanService>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly LoanService _sut;
    private readonly DateTimeOffset _fixedUtcNow = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    public LoanServiceTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _bookCopyRepositoryMock = new Mock<IBookCopyRepository>();
        _userRepositoryMock = new Mock<IBaseRepository<User>>();
        _loggerMock = new Mock<ILogger<LoanService>>();
        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(_fixedUtcNow);

        _sut = new LoanService(
            _loanRepositoryMock.Object,
            _bookCopyRepositoryMock.Object,
            _userRepositoryMock.Object,
            _timeProvider,
            _loggerMock.Object);
    }

    #region BorrowBookAsync Tests

    [Theory]
    [InlineData(0)]  // DueDate == utcNow
    [InlineData(-1)] // DueDate in the past (1 second before utcNow)
    public async Task BorrowBookAsync_WhenDueDateBeforeOrAtUtcNow_ReturnsValidationError(int secondsToAdd)
    {
        // Arrange
        var request = CreateValidBorrowBookRequestDto() with
        {
            DueDate = _fixedUtcNow.AddSeconds(secondsToAdd)
        };

        // Act
        var result = await _sut.BorrowBookAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);

        _loanRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookCopyNotExists_ReturnsNotFoundError()
    {
        // Arrange
        var request = CreateValidBorrowBookRequestDto();

        _bookCopyRepositoryMock
            .Setup(r => r.ExistsAsync(request.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.BorrowBookAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _loanRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenUserNotExists_ReturnsNotFoundError()
    {
        // Arrange
        var request = CreateValidBorrowBookRequestDto();

        _bookCopyRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(request.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.BorrowBookAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _loanRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookCopyHasActiveLoan_ReturnsConflictError()
    {
        // Arrange
        var request = CreateValidBorrowBookRequestDto();

        _bookCopyRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _loanRepositoryMock
            .Setup(r => r.BookCopyHasActiveLoanAsync(request.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.BorrowBookAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        _loanRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BorrowBookAsync_AllValid_CreatesLoanAndReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidBorrowBookRequestDto();
        int expectedLoanId = 42;

        _bookCopyRepositoryMock
            .Setup(r => r.ExistsAsync(request.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(request.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _loanRepositoryMock
            .Setup(r => r.BookCopyHasActiveLoanAsync(request.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _loanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((loan, _) => loan.Id = expectedLoanId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.BorrowBookAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(expectedLoanId);
        result.Data.UserId.Should().Be(request.UserId);
        result.Data.BookCopyId.Should().Be(request.BookCopyId);
        result.Data.DueDate.Should().Be(request.DueDate);
        result.Data.LoanDate.Should().Be(_fixedUtcNow);
        result.Data.ReturnedDate.Should().BeNull();

        _loanRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Loan>(l =>
                l.UserId == request.UserId &&
                l.BookCopyId == request.BookCopyId &&
                l.DueDate == request.DueDate &&
                l.LoanDate == _fixedUtcNow),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region ReturnBookAsync Tests

    [Fact]
    public async Task ReturnBookAsync_WhenLoanDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        int loanId = 999;

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _sut.ReturnBookAsync(loanId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenLoanAlreadyReturned_ReturnsConflictError()
    {
        // Arrange
        int loanId = 10;
        var existingLoan = new Loan
        {
            Id = loanId,
            UserId = 1,
            BookCopyId = 2,
            LoanDate = _fixedUtcNow.AddDays(-7),
            DueDate = _fixedUtcNow.AddDays(7),
            ReturnedDate = _fixedUtcNow.AddDays(-1)
        };

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        // Act
        var result = await _sut.ReturnBookAsync(loanId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReturnBookAsync_AllValid_MarksAsReturnedAndUpdateRepository()
    {
        // Arrange
        int loanId = 10;
        var existingLoan = new Loan
        {
            Id = loanId,
            UserId = 1,
            BookCopyId = 2,
            LoanDate = _fixedUtcNow.AddDays(-7),
            DueDate = _fixedUtcNow.AddDays(7),
            ReturnedDate = null
        };

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _loanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ReturnBookAsync(loanId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Loan>(l =>
                l.Id == loanId &&
                l.ReturnedDate == _fixedUtcNow),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private BorrowBookRequestDto CreateValidBorrowBookRequestDto() => new()
    {
        UserId = 1,
        BookCopyId = 5,
        DueDate = _fixedUtcNow.AddDays(14)
    };

    #endregion
}
