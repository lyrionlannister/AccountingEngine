export type PaginationProps = {
  page: number
  pageCount: number
  totalLabel?: string
  onChange: (page: number) => void
}
