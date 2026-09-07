using BookCatalog.API.Dtos.Author;
using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.BookCopy;
using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Dtos.User;
using BookCatalog.API.Entities.Enums;
using BookCatalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BookCatalog.IntegrationTests.Workflows;


public class WorkflowTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };
    public WorkflowTests(BookCatalogWebApplicationFactory factory) : base(factory)
    { }

    [Fact]
    public async Task Should_CompleteFullBookLifeCycle_FromAuthorCreationToBookReturning()
    {
        var ct = TestContext.Current.CancellationToken;

        //Creating Author/Book/BookCopy
        var result = await CreateAuthorWithBookWithCopy(ct);
        var bookCopyDto = result.CopyDto;

        //Creating User
        var createUserRequest = new CreateUserRequestDto()
        {
            Email = "ammar@example.com",
            Name = "Ammar"
        };
        var userDto = await PostAsync<CreateUserRequestDto, UserDto>
                                    ("/api/users", createUserRequest, ct);


        //Borrowing a book
        var borrowRequest = new BorrowBookRequestDto()
        {
            BookCopyId = bookCopyDto.Id,
            UserId = userDto.Id,
            DueDate = DateTimeOffset.UtcNow.AddDays(2)
        };
        var borrowBookResponseDto = await PostAsync<BorrowBookRequestDto, BorrowBookResponseDto>
                                            ("/api/loans", borrowRequest, ct);

        //Return a book
        var returnResponse = await _client.PatchAsync($"/api/loans/{borrowBookResponseDto.Id}", null, ct);
        returnResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        //Get the loan
        var loanResponse = await _client.GetAsync($"/api/loans/{borrowBookResponseDto.Id}", ct);
        loanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loanResponseDto = await loanResponse.Content.ReadFromJsonAsync<LoanDto>(ct);

        loanResponseDto.Should().NotBeNull();
        loanResponseDto.BookCopyId.Should().Be(borrowRequest.BookCopyId);
        loanResponseDto.UserId.Should().Be(borrowRequest.UserId);
        loanResponseDto.UserEmail.Should().Be(userDto.Email);
        loanResponseDto.BookTitle.Should().Be(result.BookDto.Title);
        loanResponseDto.ReturnedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_PreventDuplicateBorrowing_WhenBookIsAlreadyInActiveLoanUntilReturned()
    {
        var ct = TestContext.Current.CancellationToken;

        //Creating Author
        var result = await CreateAuthorWithBookWithCopy(ct);
        var bookCopyDto = result.CopyDto;

        //Creating Two Users
        var createUserRequest1 = new CreateUserRequestDto()
        {
            Email = "user1@example.com",
            Name = "user1"
        };
        var userDto1 = await PostAsync<CreateUserRequestDto, UserDto>
                                    ("/api/users", createUserRequest1, ct);
        var createUserRequest2 = new CreateUserRequestDto()
        {
            Email = "user2@example.com",
            Name = "user2"
        };
        var userDto2 = await PostAsync<CreateUserRequestDto, UserDto>
                                    ("/api/users", createUserRequest2, ct);


        //User1 Borrowing a book
        var borrowRequest1 = new BorrowBookRequestDto()
        {
            BookCopyId = bookCopyDto.Id,
            UserId = userDto1.Id,
            DueDate = DateTimeOffset.UtcNow.AddDays(2)
        };
        var borrowBookResponseDto1 = await PostAsync<BorrowBookRequestDto, BorrowBookResponseDto>
                                            ("/api/loans", borrowRequest1, ct);


        //User2 trying to borrow an unavailable book
        var borrowRequest2 = new BorrowBookRequestDto()
        {
            BookCopyId = bookCopyDto.Id,
            UserId = userDto2.Id,
            DueDate = DateTimeOffset.UtcNow.AddDays(2)
        };
        await PostAsync<BorrowBookRequestDto>
                   ("/api/loans", borrowRequest2, ct, HttpStatusCode.Conflict);

        //User1 return the book
        var returnResponse = await _client.PatchAsync($"/api/loans/{borrowBookResponseDto1.Id}", null, ct);
        returnResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        //User2 loan the book
        var borrowBookResponseDto2 = await PostAsync<BorrowBookRequestDto, BorrowBookResponseDto>
                                          ("/api/loans", borrowRequest2, ct);

        //Get the loan
        var loanResponse = await _client.GetAsync($"/api/loans/{borrowBookResponseDto2.Id}", ct);
        loanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loanResponseDto = await loanResponse.Content.ReadFromJsonAsync<LoanDto>(ct);

        loanResponseDto.Should().NotBeNull();
        loanResponseDto.BookCopyId.Should().Be(borrowRequest2.BookCopyId);
        loanResponseDto.UserId.Should().Be(borrowRequest2.UserId);
        loanResponseDto.UserEmail.Should().Be(userDto2.Email);
        loanResponseDto.BookTitle.Should().Be(result.BookDto.Title);
        loanResponseDto.ReturnedDate.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenAttemptingToReturnAnAlreadyReturnedBook()
    {
        var ct = TestContext.Current.CancellationToken;

        // Creating Author / Book / BookCopy
        var result = await CreateAuthorWithBookWithCopy(ct);
        var bookCopyDto = result.CopyDto;

        // Creating User
        var createUserRequest = new CreateUserRequestDto
        {
            Email = "return_twice_user@example.com",
            Name = "ReturnTwiceUser"
        };
        var userDto = await PostAsync<CreateUserRequestDto, UserDto>("/api/users", createUserRequest, ct);

        // Borrowing the book copy
        var borrowRequest = new BorrowBookRequestDto
        {
            BookCopyId = bookCopyDto.Id,
            UserId = userDto.Id,
            DueDate = DateTimeOffset.UtcNow.AddDays(2)
        };
        var borrowResponse = await PostAsync<BorrowBookRequestDto, BorrowBookResponseDto>("/api/loans", borrowRequest, ct);

        // First return attempt -> Expect 204 NoContent
        var firstReturnResponse = await _client.PatchAsync($"/api/loans/{borrowResponse.Id}", null, ct);
        firstReturnResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Second return attempt (Double Return) -> Expect 409 Conflict
        var secondReturnResponse = await _client.PatchAsync($"/api/loans/{borrowResponse.Id}", null, ct);
        secondReturnResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Verify the loan still reflects the returned state
        var loanResponse = await _client.GetAsync($"/api/loans/{borrowResponse.Id}", ct);
        loanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loanDto = await loanResponse.Content.ReadFromJsonAsync<LoanDto>(ct);

        loanDto.Should().NotBeNull();
        loanDto.ReturnedDate.Should().NotBeNull();
    }

    #region Helpers
    private async Task<(AuthorDto AuthorDto, BookDto BookDto, BookCopyDto CopyDto)> CreateAuthorWithBookWithCopy(CancellationToken ct)
    {
        var authorDto = await PostAsync<CreateAuthorRequestDto, AuthorDto>(
            "/api/authors", new CreateAuthorRequestDto { Name = "Name", Biography = "Biography" }, ct);

        var bookDto = await PostAsync<CreateBookRequestDto, BookDto>(
            "/api/books",
            new CreateBookRequestDto
            {
                AuthorId = authorDto.Id,
                Description = "Description",
                Title = "Title",
                Genre = BookGenre.Fiction,
                Price = 99.99m,
                Isbn = $"ISBN-{Guid.NewGuid():N}"[..13],
                PublicationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))
            }, ct, options: _jsonOptions);

        var copyDto = await PostAsync<CreateBookCopyRequestDto, BookCopyDto>(
            "/api/book-copies", new CreateBookCopyRequestDto { Barcode = $"BC-{Guid.NewGuid():N}"[..13], BookId = bookDto.Id }, ct);

        return (authorDto, bookDto, copyDto);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
    string url,
    TRequest request,
    CancellationToken ct,
    HttpStatusCode expectedCode = HttpStatusCode.Created,
    JsonSerializerOptions? options = null)
    {
        var response = await _client.PostAsJsonAsync(url, request, ct);
        if(response.StatusCode != expectedCode)
        {
            var details = await response.Content.ReadAsStringAsync(ct);
            response.StatusCode.Should().Be(expectedCode, because: $"API returned: {details}");
        }


        var dto = await response.Content.ReadFromJsonAsync<TResponse>(options, cancellationToken: ct);
        dto.Should().NotBeNull();

        return dto;
    }

    private async Task PostAsync<TRequest>(
    string url,
    TRequest request,
    CancellationToken ct,
    HttpStatusCode expectedCode = HttpStatusCode.Created)
    {
        var response = await _client.PostAsJsonAsync(url, request, ct);
        response.StatusCode.Should().Be(expectedCode);
    }
    #endregion
}
