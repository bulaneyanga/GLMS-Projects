using System.ComponentModel.DataAnnotations;

namespace GLMS.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Amount (USD)")]
    public decimal AmountUsd { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Cost (ZAR)")]
    public decimal CostZar { get; set; }

    [Range(typeof(decimal), "0.000001", "999999")]
    [Display(Name = "USD/ZAR rate")]
    public decimal ExchangeRate { get; set; }

    [Required, StringLength(40)]
    public string Status { get; set; } = "Submitted";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
