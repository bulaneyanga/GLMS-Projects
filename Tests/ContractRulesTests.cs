using GLMS.Models;
using GLMS.Services;
using Xunit;

namespace GLMS.Tests;

public class ContractRulesTests
{
    [Theory]
    [InlineData(ContractStatus.Draft, true)]
    [InlineData(ContractStatus.Active, true)]
    [InlineData(ContractStatus.Expired, false)]
    [InlineData(ContractStatus.OnHold, false)]
    public void CanCreateServiceRequest_AllowsOnlyEligibleContractStates(ContractStatus status, bool expected)
    {
        Assert.Equal(expected, ContractRules.CanCreateServiceRequest(status));
    }

    [Fact]
    public void CanCreateServiceRequest_RejectsUndefinedStatus()
    {
        Assert.False(ContractRules.CanCreateServiceRequest((ContractStatus)999));
    }

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void EnsureCanCreateServiceRequest_ThrowsForBlockedStates(ContractStatus status)
    {
        Assert.Throws<InvalidOperationException>(() => ContractRules.EnsureCanCreateServiceRequest(status));
    }
}
