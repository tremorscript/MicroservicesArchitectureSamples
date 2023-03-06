using StackExchange.Redis;
using System.Text.Json;

public class ProductRepository : IProductRepository
{
    private readonly ILogger<ProductRepository> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public ProductRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<ProductRepository>();
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _database.KeyDeleteAsync(Convert.ToString(id));
    }

    public async Task<Product>? GetProductAsync(int id)
    {
        var data = await _database.StringGetAsync(Convert.ToString(id));

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Product>(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public IEnumerable<Product>? GetProducts()
    {
        var server = GetServer();
        var data = server.Keys();
        var keys = data?.Select(key => (string)key).ToArray();
        List<Product> products = new List<Product>();
        if ((keys == null) || (keys.Length == 0))
        {
            return null;
        }

        foreach (string key in keys)
        {
            var value = _database.StringGet(key);

            var product = JsonSerializer.Deserialize<Product>(value, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            products.Add(product!);
        }

        return products;
    }

    public async Task<Product>? UpdateProductAsync(Product product)
    {
        var created = await _database.StringSetAsync(Convert.ToString(product.Id), JsonSerializer.Serialize(product));

        if (!created)
        {
            _logger.LogInformation("Problem occur persisting the item.");
            return null;
        }

        _logger.LogInformation("Basket item persisted succesfully.");

        return await GetProductAsync(product.Id);
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }
}