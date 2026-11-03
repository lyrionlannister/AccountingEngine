using System.Collections.Generic;

namespace AccountingEngine.Desktop.Auth.Models;

public sealed record Session(string AccessToken, IReadOnlyList<MembershipSummary> Memberships);
