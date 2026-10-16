using System.ComponentModel.DataAnnotations;

namespace GLMS.Models;

public class Contract
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Client")]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, StringLength(300)]
    [Display(Name = "Service level")]
    public string ServiceLevel { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AgreementFileName { get; set; }
    [StringLength(1000)]
    public string? AgreementStorageKey { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
