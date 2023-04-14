using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Webhooks.API.Model;

namespace Webhooks.API.Infrastructure;

public class WebhooksContext : DbContext
{

    public WebhooksContext(DbContextOptions<WebhooksContext> options) : base(options)
    {
    }
    public DbSet<WebhookSubscription> Subscriptions { get; set; }
}
