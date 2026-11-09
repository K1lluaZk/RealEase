using System.Net.Http.Json;
using RealEase.Application.Dtos.Contract;
using RealEase.Application.Dtos.Payment;
using RealEase.Application.Dtos.Property;
using RealEase.Application.Dtos.Visit;

namespace RealEase.Web.Services;

public sealed class RealEaseApiClient(HttpClient httpClient)
{
    public Task<PropertyDto?> GetPropertyAsync(long id, CancellationToken cancellationToken = default) => GetAsync<PropertyDto>($"api/Properties/{id}", cancellationToken);
    public Task<VisitDto?> GetVisitAsync(long id, CancellationToken cancellationToken = default) => GetAsync<VisitDto>($"api/Visits/{id}", cancellationToken);
    public Task<ContractDto?> GetContractAsync(long id, CancellationToken cancellationToken = default) => GetAsync<ContractDto>($"api/Contracts/{id}", cancellationToken);
    public Task<PaymentDto?> GetPaymentAsync(long id, CancellationToken cancellationToken = default) => GetAsync<PaymentDto>($"api/Payments/{id}", cancellationToken);

    public async Task<List<PropertyDto>> GetPropertiesAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<PropertyDto>>("api/Properties", cancellationToken) ?? [];

    public async Task<List<VisitDto>> GetVisitsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<VisitDto>>("api/Visits", cancellationToken) ?? [];

    public async Task<List<ContractDto>> GetContractsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<ContractDto>>("api/Contracts", cancellationToken) ?? [];

    public async Task<List<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<PaymentDto>>("api/Payments", cancellationToken) ?? [];

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(endpoint, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}
