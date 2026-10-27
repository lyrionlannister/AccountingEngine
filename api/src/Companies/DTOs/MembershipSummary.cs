using Companies.Domain.Enums;

namespace Companies.DTOs;

public sealed record MembershipSummary(Guid CompanyPublicId, string CompanyName, CompanyMembershipRole Role);
