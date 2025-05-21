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
          { data: 'employeeId', title: 'Mã nhân viên', className: 'text-center' },
          { data: 'employeeName', title: 'Tên nhân viên' },
          { data: 'performanceScore', title: 'Điểm hiệu suất', className: 'text-center' },
          { data: 'positionName', title: 'Chức vụ' },
          { data: 'salesAmount', title: 'Doanh số', className: 'text-right' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
      })
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
