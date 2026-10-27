import { ArrowDown, ArrowUp, ChevronsUpDown } from 'lucide-react'
import type { TableProps } from '../props/table-props'

export const Table = <T,>({ columns, rows, rowKey, sortKey, sortDir = 'asc', onSort, empty }: TableProps<T>) => {
  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse font-body text-[12.5px]">
        <thead>
          <tr>
            {columns.map((col) => {
              const sorted = col.key === sortKey
              return (
                <th
                  key={col.key}
                  className={`border-t border-b border-line bg-bg px-3 py-2.5 font-mono text-[10.5px] font-medium tracking-wider text-muted uppercase whitespace-nowrap first:pl-4.5 last:pr-4.5 ${
                    col.align === 'right' ? 'text-right' : 'text-left'
                  }`}
                >
                  {col.sortable ? (
                    <button
                      type="button"
                      onClick={() => onSort?.(col.key)}
                      className={`inline-flex items-center gap-1 ${sorted ? 'text-text' : 'hover:text-text'} ${col.align === 'right' ? 'flex-row' : ''}`}
                    >
                      {col.header}
                      <span className={sorted ? 'text-primary opacity-100' : 'opacity-45'}>
                        {sorted ? (
                          sortDir === 'asc' ? (
                            <ArrowUp className="size-3" aria-hidden />
                          ) : (
                            <ArrowDown className="size-3" aria-hidden />
                          )
                        ) : (
                          <ChevronsUpDown className="size-3" aria-hidden />
                        )}
                      </span>
                    </button>
                  ) : (
                    col.header
                  )}
                </th>
              )
            })}
          </tr>
        </thead>
        <tbody>
          {rows.length ? (
            rows.map((row) => (
              <tr key={rowKey(row)} className="group">
                {columns.map((col) => (
                  <td
                    key={col.key}
                    className={`border-b border-line px-3 py-2.5 align-middle whitespace-nowrap group-hover:bg-row-hover first:pl-4.5 last:pr-4.5 ${
                      col.align === 'right' ? 'text-right' : 'text-left'
                    }`}
                  >
                    {col.render(row)}
                  </td>
                ))}
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={columns.length} className="py-8 text-center whitespace-normal text-muted">
                {empty}
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
