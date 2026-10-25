using Companies.Domain.Enums;

namespace Companies.DTOs;

public sealed record MembershipSummary(long CompanyId, CompanyMembershipRole Role);
