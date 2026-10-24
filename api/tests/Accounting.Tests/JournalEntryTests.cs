using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class JournalEntryTests
{
    private static JournalEntry CreateBalancedDraft()
    {
        var entry = JournalEntry.Create(1, 1, 1, new DateOnly(2026, 10, 23), "Venta de mercancía");
        entry.AddLine(accountId: 1, costCenterId: null, partyId: null, currencyId: 1, debitAmount: 100m, creditAmount: 0m);
        entry.AddLine(accountId: 2, costCenterId: null, partyId: null, currencyId: 1, debitAmount: 0m, creditAmount: 100m);
        return entry;
    }

    [Fact]
    public void Post_WhenBalanced_Succeeds()
    {
        var entry = CreateBalancedDraft();

        entry.Post(DateTimeOffset.UtcNow, "user-1");

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        Assert.NotNull(entry.PostedAt);
        Assert.Equal("user-1", entry.PostedBy);
    }

    [Fact]
    public void Post_WhenUnbalanced_Throws()
    {
        var entry = JournalEntry.Create(1, 1, 1, new DateOnly(2026, 10, 23), "Asiento desbalanceado");
        entry.AddLine(1, null, null, 1, debitAmount: 100m, creditAmount: 0m);
        entry.AddLine(2, null, null, 1, debitAmount: 0m, creditAmount: 50m);

        Assert.Throws<InvalidOperationException>(() => entry.Post(DateTimeOffset.UtcNow, "user-1"));
    }

    [Fact]
    public void Post_WithFewerThanTwoLines_Throws()
    {
        var entry = JournalEntry.Create(1, 1, 1, new DateOnly(2026, 10, 23), "Asiento incompleto");
        entry.AddLine(1, null, null, 1, debitAmount: 100m, creditAmount: 0m);

        Assert.Throws<InvalidOperationException>(() => entry.Post(DateTimeOffset.UtcNow, "user-1"));
    }

    [Fact]
    public void Post_Twice_Throws()
    {
        var entry = CreateBalancedDraft();
        entry.Post(DateTimeOffset.UtcNow, "user-1");

        Assert.Throws<InvalidOperationException>(() => entry.Post(DateTimeOffset.UtcNow, "user-1"));
    }

    [Fact]
    public void AddLine_AfterPosting_Throws()
    {
        var entry = CreateBalancedDraft();
        entry.Post(DateTimeOffset.UtcNow, "user-1");

        Assert.Throws<InvalidOperationException>(() => entry.AddLine(1, null, null, 1, 10m, 0m));
    }

    [Fact]
    public void Void_WhenDraft_Succeeds()
    {
        var entry = CreateBalancedDraft();

        entry.Void();

        Assert.Equal(JournalEntryStatus.Voided, entry.Status);
    }

    [Fact]
    public void Void_WhenPosted_Throws()
    {
        var entry = CreateBalancedDraft();
        entry.Post(DateTimeOffset.UtcNow, "user-1");

        Assert.Throws<InvalidOperationException>(() => entry.Void());
    }

    [Fact]
    public void Reverse_WhenPosted_CreatesNewEntryWithSwappedLinesAndDoesNotMutateOriginal()
    {
        var entry = CreateBalancedDraft();
        entry.Post(DateTimeOffset.UtcNow, "user-1");

        var reversal = entry.Reverse(new DateOnly(2026, 10, 24), "Reversión de venta");

        Assert.Equal(JournalEntryStatus.Draft, reversal.Status);
        Assert.Equal(entry.Id, reversal.ReversalOfEntryId);
        Assert.Equal(entry.Lines[0].CreditAmount, reversal.Lines[0].DebitAmount);
        Assert.Equal(entry.Lines[1].DebitAmount, reversal.Lines[1].CreditAmount);

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        Assert.Equal(100m, entry.Lines[0].DebitAmount);
        Assert.Equal(100m, entry.Lines[1].CreditAmount);
    }

    [Fact]
    public void Reverse_WhenDraft_Throws()
    {
        var entry = CreateBalancedDraft();

        Assert.Throws<InvalidOperationException>(() => entry.Reverse(new DateOnly(2026, 10, 24), "Reversión"));
    }
}
