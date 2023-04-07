using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SampleApi2.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductRepository repository;

    public ProductController(IProductRepository repository)
    {
        this.repository = repository;
    }

    //Get api/v1/[controller]/:id
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Product>> ProductByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = await repository.GetProductAsync(id);

        if (item != null)
        {
            return item;
        }

        return NotFound();
    }

    //PUT api/v1/[controller]/update
    [Route("update")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateProductAsync([FromBody] Product ProductToUpdate)
    {
        var product = await repository.UpdateProductAsync(ProductToUpdate);

        if (product == null)
        {
            return NotFound(new { Message = $"Item with id {ProductToUpdate.Id} not found." });
        }

        return CreatedAtAction(nameof(ProductByIdAsync), new { id = ProductToUpdate.Id }, ProductToUpdate);
    }

    //DELETE api/v1/[controller]/id
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteProductAsync(int id)
    {
        var product = await repository.DeleteProductAsync(id);

        if (!product)
        {
            return NotFound();
        }

        return NoContent();
    }

    //POST api/v1/[controller]/create
    [Route("create")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreateProductAsync([FromBody] Product product)
    {
        var item = new Product
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
        };

        var created = await repository.UpdateProductAsync(product);

        return CreatedAtAction(nameof(ProductByIdAsync), new { id = item.Id }, null);
    }
}