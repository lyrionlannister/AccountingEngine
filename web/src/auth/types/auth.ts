export const CompanyMembershipRole = {
  Admin: 0,
  Accountant: 1,
  Viewer: 2,
} as const

export type MembershipSummary = {
  companyPublicId: string
  companyName: string
  role: (typeof CompanyMembershipRole)[keyof typeof CompanyMembershipRole]
}

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = {
  accessToken: string
  memberships: MembershipSummary[]
}

export type Session = {
  accessToken: string
  memberships: MembershipSummary[]
}
