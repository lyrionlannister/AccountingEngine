export type RegisterRequest = {
  email: string
  password: string
  tenantName: string
  companyName: string
  countryCode: string
}

export type RegisterResponse = {
  ownerPublicId: string
  tenantPublicId: string
  companyPublicId: string
  membershipPublicId: string
}
