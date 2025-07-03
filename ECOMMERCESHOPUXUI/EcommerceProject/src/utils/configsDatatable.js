import $ from 'jquery'

export function attachDetailsControl(tableSelector, format) {
  try {
    $(tableSelector + ' tbody')
      .off('click', 'td.details-control')
      .on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr')
        var tdi = tr.find('i.fa')
        var row = $(tableSelector).DataTable().row(tr)

        if (row.child.isShown()) {
          row.child.hide()
          tr.removeClass('shown')
          tdi.first().removeClass('fa-minus-square')
          tdi.first().addClass('fa-plus-square')
        } else {
          row.child(format(row.data())).show()
          tr.addClass('shown')
          tdi.first().removeClass('fa-plus-square')
          tdi.first().addClass('fa-minus-square')
        }
      })
  } catch (e) {
    console.warn(e)
  }
}

//#region Default value for Datatable
export const defaultLanguageDatatable = {
  sSearch: 'Tìm kiếm:',
  lengthMenu: 'Hiển thị _MENU_ mục',
  info: 'Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ mục',
  paginate: {
    first: '<<',
    last: '>>',
    next: '>',
    previous: '<',
  },
  zeroRecords: `<div style="text-align: center;">Không tìm thấy kết quả nào.</div>`,
  infoEmpty: 'Không có mục nào để hiển thị',
  infoFiltered: '(lọc từ _MAX_ mục)',
}

export const defaultTdToShowDetail = {
  className: 'details-control',
  orderable: false,
  data: null,
  defaultContent: '',
  render: function () {
    return '<i class="fa fa-plus-square" aria-hidden="true"></i>'
  },
  width: '15px',
}
//#endregion

//#region Optimize Search

/**
 * Attaches a debounce mechanism to the default search input of a DataTable.
 * @param {string} tableSelector - The selector for the DataTable.
 * @param {object} table - The DataTable instance.
 */
export function attachSearchDebounce(tableSelector, table) {
  let searchTimeout
  $(`${tableSelector}_filter input`)
    .off('keyup')
    .on('keyup', function () {
      clearTimeout(searchTimeout)
      const searchTerm = this.value
      searchTimeout = setTimeout(() => {
        table.search(searchTerm).draw()
      }, 300) // Đợi 300ms trước khi thực hiện tìm kiếm
    })
}

//#endregion
