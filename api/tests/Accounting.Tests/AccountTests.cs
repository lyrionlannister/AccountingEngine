using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var account = Account.Create(1, "1105", "Caja", AccountNature.Debit, AccountType.Asset, null, true);

        Assert.Equal(1, account.CompanyId);
        Assert.Equal("1105", account.Code);
        Assert.Equal("Caja", account.Name);
        Assert.Equal(AccountNature.Debit, account.Nature);
        Assert.Equal(AccountType.Asset, account.Type);
        Assert.Null(account.ParentAccountId);
        Assert.True(account.AllowsPosting);
        Assert.True(account.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Account.Create(1, code, "Caja", AccountNature.Debit, AccountType.Asset, null, true));
    }

    [Fact]
    public void SetPostingRequirements_UpdatesFlags()
    {
        var account = Account.Create(1, "1105", "Caja", AccountNature.Debit, AccountType.Asset, null, true);

        account.SetPostingRequirements(requiresCostCenter: true, requiresParty: true);

        Assert.True(account.RequiresCostCenter);
        Assert.True(account.RequiresParty);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var account = Account.Create(1, "1105", "Caja", AccountNature.Debit, AccountType.Asset, null, true);

        account.Deactivate();

        Assert.False(account.IsActive);
    }
}
