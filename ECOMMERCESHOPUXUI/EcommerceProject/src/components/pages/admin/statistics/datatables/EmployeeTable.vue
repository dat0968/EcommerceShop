<template>
  <div v-if="employees.length" class="table-responsive">
    <table id="employeeDatatable" class="table table-hover"></table>
  </div>
  <NoDataMessage v-else />
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'
import pathReplaceImg from '@/utils/processPathImg'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

export default {
  name: 'EmployeeTable',
  components: {
    NoDataMessage,
  },
  props: {
    employees: {
      type: Array,
      required: true,
    },
  },
  mounted() {
    this.initDataTable()
  },
  methods: {
    async initDataTable() {
      await this.$nextTick()
      const dataSet = this.employees.map((employee) => ({
        employeeId: employee.employeeId,
        employeeName: employee.employeeName,
        performanceScore: employee.performanceScore,
        positionName: employee.positionName,
        salesAmount: formatCurrency(employee.salesAmount), // Định dạng doanh số
      }))

      // Khởi tạo DataTable
      const table = $('#employeeDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'employeeId', title: 'Mã nhân viên', className: 'text-center' },
          { data: 'employeeName', title: 'Tên nhân viên' },
          // { data: 'performanceScore', title: 'Điểm hiệu suất', className: 'text-center' },
          { data: 'positionName', title: 'Chức vụ' },
          // ! { data: 'salesAmount', title: 'Doanh số', className: 'text-right' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#employeeDatatable`, this.formatDetails.bind(this))
        },
      })
      configsDt.attachSearchDebounce('#employeeDatatable', table)
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const employee = this.employees.find((x) => x.employeeId == rowData.employeeId)

      const detailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết đơn hàng gần đây của ${employee.employeeName}</h6>
          <div class="row g-3">
            ${
              employee.orderRecents && employee.orderRecents.length > 0
                ? employee.orderRecents
                    .map(
                      (order) => `
                        <div class="col-sm-12 col-md-6 col-lg-4">
                          <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                              <div class="d-flex align-items-center mb-3">
                                <img src="${pathReplaceImg(undefined, 'HinhAnh/Avatar/', order.avatar)}" class="rounded-circle me-3" style="width: 60px; height: 60px; object-fit: cover;" alt="Nhân viên">
                                <div>
                                  <h5 class="card-title mb-0">Mã hóa đơn: ${order.maHd}</h5>
                                  <p class="card-subtitle text-muted">${order.hoTen}</p>
                                </div>
                              </div>
                              <p class="mb-1"><strong>Ngày tạo:</strong> ${order.ngayTao ? new Date(order.ngayTao).toLocaleDateString() : '-'}</p>
                              <p class="mb-1"><strong>Trạng thái:</strong> <span class="badge ${order.isActive ? 'bg-success' : 'bg-danger'}">${order.tinhTrang}</span></p>
                              <p class="mb-0"><strong>Địa chỉ nhận:</strong> <span title="${order.diaChiNhanHang}">${order.diaChiNhanHang}</span></p>
                            </div>
                          </div>
                        </div>
                      `,
                    )
                    .join('')
                : '<div class="col-12"><p class="text-center text-muted">Không có đơn hàng nào để hiển thị.</p></div>'
            }
          </div>
        </div>`
      div.html(detailsHtml)
      return div
    },
  },
}
</script>

<style scoped>
.table th,
.table td {
  vertical-align: middle;
}
</style>
