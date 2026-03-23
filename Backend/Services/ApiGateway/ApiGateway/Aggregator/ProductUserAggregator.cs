using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ApiGateway.Exceptions;

namespace ApiGateway.Aggregator
{
    public class ProductUserAggregator : IDefinedAggregator
    {
        private const string ProductServiceName = "ProductService";
        private const string UserServiceName = "UserService";

        private readonly ILogger<ProductUserAggregator> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProductUserAggregator(ILogger<ProductUserAggregator> logger)
        {
            _logger = logger;
        }

        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            var productResponse = responses.First(r => r.Items.DownstreamRoute().Key == "ProductRoute").Items.DownstreamResponse();
            var userResponse = responses.First(r => r.Items.DownstreamRoute().Key == "UserRoute").Items.DownstreamResponse();

            var productsJson = await productResponse.Content.ReadAsStringAsync();

            if (userResponse == null)
            {
               _logger.LogError("User service response is null or failed. Status Code: {StatusCode}", userResponse?.StatusCode);
                throw new ServiceResponseException(UserServiceName,
    "User service response was null or the call failed.",
    (int?)userResponse?.StatusCode);
            }

            var usersJson = await userResponse.Content.ReadAsStringAsync();

            var productsWrapper = JsonSerializer.Deserialize<ServiceResponse<PagedData<ProductItem>>>(productsJson, JsonOptions)
                ?? throw new ServiceResponseException(ProductServiceName,
    "Failed to deserialize product service response.", null);

            if (!productsWrapper.Success)
            {
                _logger.LogError("Product service returned failure. Message: {Message}, Errors: {Errors}",
                    productsWrapper.Message, productsWrapper.Errors is { Count: > 0 } prodErrors ? string.Join(", ", prodErrors) : null);
                throw new ServiceResponseException(ProductServiceName, productsWrapper.Message ?? "Product service returned failure.", null);
            }

            var productsData = productsWrapper.Data
                ?? throw new ServiceResponseException(ProductServiceName, "Product service response data was null.", null);

            var usersWrapper = JsonSerializer.Deserialize<ServiceResponse<PagedData<UserItem>>>(usersJson, JsonOptions)
                ?? throw new ServiceResponseException(UserServiceName,
    "Failed to deserialize user service response.", null);

            if (!usersWrapper.Success)
            {
                _logger.LogError("User service returned failure. Message: {Message}, Errors: {Errors}",
                    usersWrapper.Message, usersWrapper.Errors is { Count: > 0 } userErrors ? string.Join(", ", userErrors) : null);
                throw new ServiceResponseException(UserServiceName, usersWrapper.Message ?? "User service returned failure.", null);
            }

            var usersData = usersWrapper.Data
                ?? throw new ServiceResponseException(UserServiceName, "User service response data was null.", null);

            var userLookup = usersData.Items.ToDictionary(u => u.Id, u => u.UserName);

            var mergedItems = productsData.Items.Select(product => new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                dateOfManufacture = product.DateOfManufacture,
                createdByUserId = product.CreatedByUserId,
                createdByUserName = userLookup.GetValueOrDefault(product.CreatedByUserId),
                imageUrl = product.ImageUrl
            }).ToList();

            var finalResult = new
            {
                success = true,
                message = "Products with users fetched successfully.",
                data = new
                {
                    items = mergedItems,
                    totalCount = productsData.TotalCount,
                    pageNumber = productsData.PageNumber,
                    pageSize = productsData.PageSize,
                    totalPages = productsData.TotalPages,
                    hasPreviousPage = productsData.HasPreviousPage,
                    hasNextPage = productsData.HasNextPage
                },
                errors = (List<string>?)null
            };

            var content = JsonSerializer.Serialize(finalResult);

            return new DownstreamResponse(
                new StringContent(content, Encoding.UTF8, "application/json"),
                HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                "OK"
            );
        }

        private sealed record ServiceResponse<T>(
            bool Success,
            string? Message,
            T? Data,
            List<string>? Errors
        );

        private sealed record PagedData<T>(
            List<T> Items,
            int TotalCount,
            int PageNumber,
            int PageSize,
            int TotalPages,
            bool HasPreviousPage,
            bool HasNextPage
        );

        private sealed record ProductItem(
            string? Id,
            string? Name,
            string? Description,
            decimal Price,
            string? DateOfManufacture,
            int CreatedByUserId,
            string? ImageUrl
        );

        private sealed record UserItem(
            int Id,
            string? UserName
        );
    }
}







