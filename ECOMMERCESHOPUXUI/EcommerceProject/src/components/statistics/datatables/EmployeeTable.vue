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

      const orderDetailsHtml =
        employee.orderRecents && employee.orderRecents.length > 0
          ? employee.orderRecents
              .map(
                (order) => `
                  <div class="row">
                    <div class="col-6 col-md-12 order-item border rounded p-2 mb-2 bg-light position-relative">
                      <p><strong>Mã hóa đơn:</strong> ${order.maHd}</p>
                      <p><strong>Tên khách hàng:</strong> ${order.hoTen}</p>
                      <p><strong>Ngày tạo:</strong> ${new Date(order.ngayTao).toLocaleDateString()}</p>
                      <p><strong>Trạng thái:</strong> <span class="${order.isActive ? 'text-success' : 'text-danger'}">${order.tinhTrang}</span></p>
                    
                      <span class="tooltip-icon position-absolute top-0 end-0 m-1" data-toggle="tooltip" title="${order.diaChiNhanHang}">
                        <i class="fas fa-info-circle"></i>
                      </span>
                    </div>
                  </div>
                `,
              )
              .join('')
          : '<p>Không có đơn hàng nào để hiển thị.</p>'

      div.html(orderDetailsHtml)

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
