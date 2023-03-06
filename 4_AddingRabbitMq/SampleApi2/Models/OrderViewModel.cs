using System.ComponentModel.DataAnnotations;

namespace SampleApi2.Models;

public class OrderViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string CustomerNumber { get; set; }

    public string Notes { get; set; }
}