public interface IProductRepository
{
    Task<Product>? GetProductAsync(int id);

    IEnumerable<Product>? GetProducts();

    Task<Product>? UpdateProductAsync(Product product);

    Task<bool> DeleteProductAsync(int id);
}