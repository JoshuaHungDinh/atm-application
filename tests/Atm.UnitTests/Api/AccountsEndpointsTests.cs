using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atm.Application.Accounts;
using Atm.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Atm.UnitTests.Api;

/// <summary>
/// Black-box tests over the running API: real HTTP in, JSON out, against a seeded
/// in-memory database. Covers the happy paths and every error-to-status mapping.
/// </summary>
public sealed class AccountsEndpointsTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly AtmApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountsEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<(Guid Checking, Guid Savings)> SeededIdsAsync()
    {
        List<AccountSnapshot> accounts = await GetAccountsAsync();
        return (
            accounts.Single(a => a.Name == "Checking").Id,
            accounts.Single(a => a.Name == "Savings").Id);
    }

    private async Task<List<AccountSnapshot>> GetAccountsAsync()
    {
        List<AccountSnapshot>? accounts =
            await _client.GetFromJsonAsync<List<AccountSnapshot>>("/accounts", Json);
        Assert.NotNull(accounts);
        return accounts;
    }

    [Fact]
    public async Task Get_accounts_returns_the_two_seeded_accounts()
    {
        List<AccountSnapshot> accounts = await GetAccountsAsync();

        Assert.Equal(2, accounts.Count);
        Assert.Equal(1000m, accounts.Single(a => a.Name == "Checking").Balance);
        Assert.Equal(500m, accounts.Single(a => a.Name == "Savings").Balance);
    }

    [Fact]
    public async Task Deposit_updates_the_balance_and_records_a_transaction()
    {
        (Guid checking, _) = await SeededIdsAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/accounts/{checking}/deposit", new { amount = 50m }, Json);

        response.EnsureSuccessStatusCode();
        AccountSnapshot? account = await response.Content.ReadFromJsonAsync<AccountSnapshot>(Json);
        Assert.NotNull(account);
        Assert.Equal(1050m, account.Balance);
        TransactionSnapshot entry = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Deposit, entry.Type);
        Assert.Equal(50m, entry.Amount);
    }

    [Fact]
    public async Task Withdrawing_more_than_the_balance_returns_409_problem_details()
    {
        (Guid checking, _) = await SeededIdsAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/accounts/{checking}/withdraw", new { amount = 999_999m }, Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.NotNull(problem);
        Assert.Equal("Insufficient funds", problem.Title);
    }

    [Fact]
    public async Task Depositing_a_negative_amount_returns_422()
    {
        (Guid checking, _) = await SeededIdsAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/accounts/{checking}/deposit", new { amount = -5m }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.NotNull(problem);
        Assert.Equal("Invalid amount", problem.Title);
    }

    [Fact]
    public async Task Transfer_moves_funds_between_the_two_accounts()
    {
        (Guid checking, Guid savings) = await SeededIdsAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/transfers",
            new { fromAccountId = checking, toAccountId = savings, amount = 100m },
            Json);

        response.EnsureSuccessStatusCode();
        TransferResult? result = await response.Content.ReadFromJsonAsync<TransferResult>(Json);
        Assert.NotNull(result);
        Assert.Equal(900m, result.From.Balance);
        Assert.Equal(600m, result.To.Balance);
        Assert.Equal("Transfer to Savings", Assert.Single(result.From.Transactions).Description);
    }

    [Fact]
    public async Task Operating_on_an_unknown_account_returns_404()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/accounts/{Guid.NewGuid()}/withdraw", new { amount = 1m }, Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.NotNull(problem);
        Assert.Equal("Account not found", problem.Title);
    }

    [Fact]
    public async Task Transferring_to_the_same_account_returns_422()
    {
        (Guid checking, _) = await SeededIdsAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/transfers",
            new { fromAccountId = checking, toAccountId = checking, amount = 1m },
            Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.NotNull(problem);
        Assert.Equal("Invalid transfer", problem.Title);
    }
}
