using System.ComponentModel.DataAnnotations;

namespace GLMS.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Amount (USD)")]
    public decimal AmountUsd { get; set; }

    public decimal? ExchangeRate { get; set; }
}
