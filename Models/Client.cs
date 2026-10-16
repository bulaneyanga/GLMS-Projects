using System.ComponentModel.DataAnnotations;

namespace GLMS.Models;

public class Client
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(240)]
    [Display(Name = "Contact details")]
    public string ContactDetails { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
