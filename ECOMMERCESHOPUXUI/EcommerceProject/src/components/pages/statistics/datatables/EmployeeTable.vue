<template>
  <div v-if="employees.length" class="table-responsive">
    <table id="employeeDatatable" class="table table-hover"></table>
  </div>
  <p v-else>Không có nhân viên nào để hiển thị.</p>
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'
import pathReplaceImg from '@/utils/processPathImg'

export default {
  name: 'EmployeeTable',
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
    initDataTable() {
      const dataSet = this.employees.map((employee) => ({
        employeeId: employee.employeeId,
        employeeName: employee.employeeName,
        performanceScore: employee.performanceScore,
        positionName: employee.positionName,
        salesAmount: formatCurrency(employee.salesAmount), // Định dạng doanh số
      }))

      // Khởi tạo DataTable
      $('#employeeDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'employeeId', title: 'Mã nhân viên', className: 'text-center' },
          { data: 'employeeName', title: 'Tên nhân viên' },
          { data: 'performanceScore', title: 'Điểm hiệu suất', className: 'text-center' },
          { data: 'positionName', title: 'Chức vụ' },
          { data: 'salesAmount', title: 'Doanh số', className: 'text-right' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#employeeDatatable`, this.formatDetails.bind(this))
        },
      })
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const employee = this.employees.find((x) => x.employeeId == rowData.employeeId)

      const detailsHtml = `
    <div class="container">
      <div class="row mb-3 gap-1 justify-content-between detail-list">
        ${
          employee.orderRecents && employee.orderRecents.length > 0
            ? employee.orderRecents
                .map(
                  (order) => `
                    <div class="col-sm-12 col-md-6 p-3 detail-item">
                      <div class="row border p-1 rounded bg-light">
                        <div class="col-4 d-flex align-items-center">
                          <img src="${pathReplaceImg(undefined, 'HinhAnh/Avatar/', order.avatar)}" class="img-fluid rounded" alt="Khách hàng">
                        </div>
                        <div class="col-8">
                          <div class="text-primary flex flex-flow-column justify-content-between">
                            <span class="col-auto">Mã hóa đơn: ${order.maHd}</span>
                          </div>
                          <p><strong>Tên khách hàng:</strong> ${order.hoTen}</p>
                          <p><strong>Ngày tạo:</strong> ${new Date(order.ngayTao).toLocaleDateString()}</p>
                          <p><strong>Trạng thái:</strong> <span class="${order.isActive ? 'text-success' : 'text-danger'}">${order.tinhTrang}</span></p>
                          <p><strong>Địa chỉ nhận:</strong> ${order.diaChiNhanHang}</p>
                        </div>
                      </div>
                    </div>
                  `,
                )
                .join('')
            : '<p>Không có đơn hàng nào để hiển thị.</p>'
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
