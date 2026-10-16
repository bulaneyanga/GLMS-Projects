using System.ComponentModel.DataAnnotations;

namespace GLMS.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    [Display(Name = "On Hold")]
    OnHold
}
