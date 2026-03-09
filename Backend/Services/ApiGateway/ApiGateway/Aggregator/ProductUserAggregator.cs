using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Aggregator
{
    public class ProductUserAggregator : IDefinedAggregator
    {
        private readonly ILogger<ProductUserAggregator> _logger;

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
                throw new Exception("User service response is null or failed.");
            }

            var usersJson = await userResponse.Content.ReadAsStringAsync();

            var productsRoot = JsonSerializer.Deserialize<JsonElement>(productsJson);

            var productItems = productsRoot.GetProperty("items").EnumerateArray().ToList();

            var userIds = productItems
                .Select(p => p.GetProperty("createdByUserId").GetInt32())
                .Distinct()
                .ToList();

            //_logger.LogInformation("User IDs: {UserIds}", string.Join(",", userIds));

            var usersRoot = JsonSerializer.Deserialize<JsonElement>(usersJson);
            var userItems = usersRoot.GetProperty("items").EnumerateArray().ToList();

            var mergedItems = (
                from product in productItems
                join user in userItems
                on product.GetProperty("createdByUserId").GetInt32()
                equals user.GetProperty("id").GetInt32()
                into userGroup
                from matchedUser in userGroup.DefaultIfEmpty()
                select new
                {
                    id = product.GetProperty("id").GetString(),
                    name = product.GetProperty("name").GetString(),
                    description = product.GetProperty("description").GetString(),
                    price = product.GetProperty("price").GetDecimal(),
                    dateOfManufacture = product.GetProperty("dateOfManufacture").GetString(),
                    createdByUserId = product.GetProperty("createdByUserId").GetInt32(),
                    createdByUserName = matchedUser.ValueKind != JsonValueKind.Undefined
                        ? matchedUser.GetProperty("userName").GetString()
                        : null,
                    imageUrl = product.GetProperty("imageUrl").GetString()
                }).ToList();


            //            var userDictionary = userItems.ToDictionary(
            //                u => u.GetProperty("id").GetInt32(),
            //                u => u.GetProperty("userName").GetString()
            //            );

            //            // Join products with users
            //            var mergedItems = productItems.Select(product =>
            //            {
            //                var createdByUserId = product.GetProperty("createdByUserId").GetInt32();

            //                userDictionary.TryGetValue(createdByUserId, out string? userName);

            //                var productDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            //                    product.GetRawText());

            //                productDict["createdByUserName"] = userName;

            //                return productDict;
            //            }).ToList();


            var finalResult = new
            {
                items = mergedItems,
                totalCount = productsRoot.GetProperty("totalCount").GetInt32(),
                pageNumber = productsRoot.GetProperty("pageNumber").GetInt32(),
                pageSize = productsRoot.GetProperty("pageSize").GetInt32(),
                totalPages = productsRoot.GetProperty("totalPages").GetInt32(),
                hasPreviousPage = productsRoot.GetProperty("hasPreviousPage").GetBoolean(),
                hasNextPage = productsRoot.GetProperty("hasNextPage").GetBoolean()
            };

            var content = JsonSerializer.Serialize(finalResult);
            //_logger.LogInformation("finalResult: {Content}", content);

            return new DownstreamResponse(
                new StringContent(content, Encoding.UTF8, "application/json"),
                HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                "OK"
            );
        }
    }
}







