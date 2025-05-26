<template>
  <div>
    <div class="row mb-3">
      <div class="col-md-3">
        <label class="form-label">Lọc theo mã danh mục cha</label>
        <select class="form-select" v-model="selectedMaDanhMucCha" @change="onFilterChange">
          <option value="">Tất cả</option>
          <option v-for="item in uniqueMaDanhMucCha" :key="item" :value="item">{{ item }}</option>
        </select>
      </div>
      <div class="col-md-3">
        <label class="form-label">Lọc theo mã danh mục con</label>
        <select class="form-select" v-model="selectedMaDanhMucCon" @change="onFilterChange">
          <option value="">Tất cả</option>
          <option v-for="item in uniqueMaDanhMucCon" :key="item" :value="item">{{ item }}</option>
        </select>
      </div>
    </div>
    <table id="datatableCategories"></table>
  </div>
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'

import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'

export default {
  name: 'CategoryIndex',
  data() {
    return {
      listCategories: [],
      isLoading: true,
      selectedMaDanhMucCha: '',
      selectedMaDanhMucCon: '',
    }
  },
  computed: {
    uniqueMaDanhMucCha() {
      return [...new Set(this.listCategories.map((x) => x.maDanhMucCha))]
    },
    uniqueMaDanhMucCon() {
      // Lọc theo mã cha nếu đã chọn
      let filtered = this.selectedMaDanhMucCha
        ? this.listCategories.filter((x) => x.maDanhMucCha == this.selectedMaDanhMucCha)
        : this.listCategories
      return [...new Set(filtered.map((x) => x.maDanhMucCon))]
    },
    filteredCategories() {
      return this.listCategories.filter((x) => {
        let matchCha = this.selectedMaDanhMucCha
          ? x.maDanhMucCha == this.selectedMaDanhMucCha
          : true
        let matchCon = this.selectedMaDanhMucCon
          ? x.maDanhMucCon == this.selectedMaDanhMucCon
          : true
        return matchCha && matchCon
      })
    },
  },
  async mounted() {
    await this.getCategories()
    this.initDataTable()
  },
  methods: {
    async getCategories() {
      this.listCategories = (
        await axiosConfig.getFromApi(
          '/categories/GetAllCategories',
          ConfigsRequest.getSkipAuthConfig(),
        )
      ).data
      console.log(this.listCategories)
    },
    initDataTable() {
      if ($.fn.DataTable.isDataTable('#datatableCategories')) {
        $('#datatableCategories').DataTable().destroy()
      }
      const dataSet = this.filteredCategories.map((cate) => ({
        maDanhMucCha: cate.maDanhMucCha,
        tenDanhMucCha: cate.tenDanhMucCha,
        maDanhMucCon: cate.maDanhMucCon,
        tenDanhMucCon: cate.tenDanhMucCon,
        maSp: cate.maSp,
        tenSanPham: cate.tenSanPham,
        isActiveDanhMucCha: cate.isActiveDanhMucCha,
        isActiveDanhMucCon: cate.isActiveDanhMucCon,
      }))

      $('#datatableCategories').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          { data: 'maDanhMucCha', title: 'Mã mục cha', className: 'text-center' },
          { data: 'tenDanhMucCha', title: 'Tên mục cha', className: 'text-center' },
          { data: 'maDanhMucCon', title: 'Mã mục con', className: 'text-center' },
          { data: 'tenDanhMucCon', title: 'Tên mục con', className: 'text-center' },
          { data: 'maSp', title: 'Mã sản phẩm' },
          { data: 'tenSanPham', title: 'Tên sản phẩm' },
          { data: 'isActiveDanhMucCha', title: 'Tình trạng mục cha', className: 'text-right' },
          { data: 'isActiveDanhMucCon', title: 'Tình trạng mục con', className: 'text-center' },
        ],
        language: configsDt.defaultLanguageDatatable,
      })
    },
    onFilterChange() {
      this.selectedMaDanhMucCon = '' // Reset mã con nếu mã cha thay đổi
      this.initDataTable()
    },
  },
  watch: {
    selectedMaDanhMucCha() {
      this.selectedMaDanhMucCon = ''
      this.initDataTable()
    },
    selectedMaDanhMucCon() {
      this.initDataTable()
    },
  },
}
</script>
