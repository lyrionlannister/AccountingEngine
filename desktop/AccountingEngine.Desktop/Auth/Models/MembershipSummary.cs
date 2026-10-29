using System;

namespace AccountingEngine.Desktop.Auth.Models;

public enum CompanyMembershipRole
{
    Admin = 0,
    Accountant = 1,
    Viewer = 2,
}

public sealed record MembershipSummary(Guid CompanyPublicId, string CompanyName, CompanyMembershipRole Role);
