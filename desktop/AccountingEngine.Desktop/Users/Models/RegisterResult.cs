using System;

namespace AccountingEngine.Desktop.Users.Models;

public sealed record RegisterResult(Guid OwnerPublicId, Guid TenantPublicId, Guid CompanyPublicId, Guid MembershipPublicId);
