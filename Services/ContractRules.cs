using GLMS.Models;

namespace GLMS.Services;

public static class ContractRules
{
    public static bool CanCreateServiceRequest(ContractStatus status) =>
        status is ContractStatus.Draft or ContractStatus.Active;

    public static void EnsureCanCreateServiceRequest(ContractStatus status)
    {
        if (!CanCreateServiceRequest(status))
            throw new InvalidOperationException($"Service requests cannot be created for a {status} contract.");
    }
}
