using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MiniBox.SimulationEngine.Models;

namespace MiniBox.SimulationEngine.Services;

public sealed class SimulatedApiClient(HttpClient client, IOptions<SimulationSettings> settings)
{
    private readonly string _baseAddress = settings.Value.TargetApiUrl;
    private readonly SemaphoreSlim _authenticationLock = new(1, 1);
    private string? _accessToken;

    public async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        await _authenticationLock.WaitAsync(cancellationToken);
        try
        {
            using var response = await client.PostAsJsonAsync($"{_baseAddress}/api/auth/login", new
            {
                email = "virtual00000@minibox.local",
                password = settings.Value.VirtualUserPassword
            }, cancellationToken);
            response.EnsureSuccessStatusCode();
            var session = await response.Content.ReadFromJsonAsync<SimulationAuthResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Simulation user login returned an empty response.");
            _accessToken = session.AccessToken;
        }
        finally { _authenticationLock.Release(); }
    }
    public async Task<IReadOnlyList<SimulatedProduct>> GetProductsAsync(CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"{_baseAddress}/api/products?pageSize=100", cancellationToken);
        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<ProductPageResponse>(cancellationToken: cancellationToken);
        return page?.Items ?? [];
    }
    public async Task GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"{_baseAddress}/api/products/{productId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> CreateOrderAsync(SimulatedProduct product, CancellationToken cancellationToken)
    {
        var token = Volatile.Read(ref _accessToken);
        if (token is null)
        {
            await AuthenticateAsync(cancellationToken);
            token = Volatile.Read(ref _accessToken);
        }
        using var response = await SendCheckoutAsync(product, token!, cancellationToken);
        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized) return response.IsSuccessStatusCode;
        await _authenticationLock.WaitAsync(cancellationToken);
        try
        {
            if (token == _accessToken) await AuthenticateAsyncWithoutLockAsync(cancellationToken);
            token = _accessToken;
        }
        finally { _authenticationLock.Release(); }
        using var retry = await SendCheckoutAsync(product, token!, cancellationToken);
        return retry.IsSuccessStatusCode;
    }

    private async Task AuthenticateAsyncWithoutLockAsync(CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync($"{_baseAddress}/api/auth/login", new
        {
            email = "virtual00000@minibox.local",
            password = settings.Value.VirtualUserPassword
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<SimulationAuthResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Simulation user login returned an empty response.");
        _accessToken = session.AccessToken;
    }

    private Task<HttpResponseMessage> SendCheckoutAsync(SimulatedProduct product, string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseAddress}/api/orders")
        {
            Content = JsonContent.Create(new
            {
                customerName = "MiniBox Simulator", email = "virtual00000@minibox.local",
                deliveryStreet = "Demo street 1", deliveryCity = "Київ", deliveryPostalCode = "01001", deliveryCountry = "Україна",
                items = new[] { new { productId = product.Id, quantity = 1 } }
            })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request, cancellationToken);
    }
    private sealed record ProductPageResponse(IReadOnlyList<SimulatedProduct> Items);
    private sealed record SimulationAuthResponse(string AccessToken);
}
