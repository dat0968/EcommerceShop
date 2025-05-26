<template>
  <div class="container pt-5">
    <!-- Breadcrumb trạng thái -->
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active">
          {{ breadcrumbText }}
        </li>
      </ol>
    </nav>
    <div class="row">
      <!-- Form thêm/sửa (3 col) -->
      <div class="col-md-3">
        <div class="card">
          <div class="card-header">
            {{ isEdit ? 'Cập nhật danh mục' : 'Thêm danh mục' }}
          </div>
          <div class="card-body">
            <form @submit.prevent="onSubmit">
              <div class="mb-3">
                <label class="form-label">Tên danh mục cha</label>
                <input v-model="form.tenDanhMucCha" type="text" class="form-control" required />
              </div>
              <div class="mb-3">
                <label class="form-label">Trạng thái</label>
                <select v-model="form.isActive" class="form-select">
                  <option :value="true">Hoạt động</option>
                  <option :value="false">Không hoạt động</option>
                </select>
              </div>
              <button type="submit" class="btn btn-primary w-100">
                {{ isEdit ? 'Cập nhật' : 'Thêm mới' }}
              </button>
              <button
                v-if="isEdit"
                type="button"
                class="btn btn-secondary w-100 mt-2"
                @click="resetForm"
              >
                Hủy
              </button>
            </form>
          </div>
        </div>
      </div>
      <!-- Bảng dữ liệu (7 col) -->
      <div class="col-md-9">
        <div class="row mb-3">
          <div class="col-md-3">
            <label class="form-label">Lọc theo mã danh mục cha</label>
            <select class="form-select" v-model="selectedMaDanhMucCha" @change="onFilterChange">
              <option value="">Tất cả</option>
              <option
                v-for="item in optionsParentCategory"
                :key="item.maDanhMucCha"
                :value="item.maDanhMucCha"
              >
                {{ item.tenDanhMucCha }}
                {{ item.isActive ? '✔️' : '❌' }}
              </option>
            </select>
          </div>
          <div class="col-md-3">
            <label class="form-label">Lọc theo mã danh mục con</label>
            <select class="form-select" v-model="selectedMaDanhMucCon" @change="onFilterChange">
              <option value="">Tất cả</option>
              <option
                v-for="item in optionsChildCategory"
                :key="item.maDanhMucCon"
                :value="item.maDanhMucCon"
              >
                {{ item.tenDanhMucCon }}
                {{ item.isActive ? '✔️' : '❌' }}
              </option>
            </select>
          </div>
        </div>
        <table
          id="datatableCategories"
          class="table table-bordered table-striped"
          style="width: 100%"
        ></table>
      </div>
    </div>
  </div>
</template>

<script>
import * as axiosConfig from '@/utils/axiosClient'
import ConfigsRequest from '@/models/ConfigsRequest'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { defaultLanguageDatatable } from '@/utils/configsDatatable'

export default {
  name: 'CategoryIndex',
  data() {
    return {
      listCategories: [],
      optionsParentCategory: [],
      optionsChildCategory: [],
      isLoading: true,
      selectedMaDanhMucCha: '',
      selectedMaDanhMucCon: '',
      isEdit: false,
      form: {
        maDanhMucCha: null,
        tenDanhMucCha: '',
        isActive: true,
      },
      breadcrumbText: 'Thêm mới danh mục',
      datatable: null,
    }
  },
  computed: {
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
    await this.loadOption()
  },
  methods: {
    async loadOption() {
      const resOptionParen = await axiosConfig.getFromApi(
        '/categories/GetAllParentCategories',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.optionsParentCategory = resOptionParen.data

      const resOptionChild = await axiosConfig.getFromApi(
        '/categories/GetAllSubCategories',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.optionsChildCategory = resOptionChild.data
    },
    async getCategories() {
      const res = await axiosConfig.getFromApi(
        '/categories/GetAllCategories',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.listCategories = res.data
      this.reloadDataTable()
    },
    onFilterChange() {
      // this.selectedMaDanhMucCon = ''
      this.reloadDataTable()
    },
    onEdit(cate) {
      this.isEdit = true
      this.form.maDanhMucCha = cate.maDanhMucCha
      this.form.tenDanhMucCha = cate.tenDanhMucCha
      this.form.isActive = cate.isActiveDanhMucCha
      this.breadcrumbText = 'Cập nhật danh mục cha'
    },
    async onDelete(cate) {
      if (confirm('Bạn có chắc chắn muốn xóa danh mục này?')) {
        await axiosConfig.deleteFromApi(
          `/categories/DeleteCategory/${cate.maDanhMucCha}`,
          ConfigsRequest.getSkipAuthConfig(),
        )
        this.breadcrumbText = 'Đã xóa danh mục cha'
        await this.getCategories()
        this.resetForm()
      }
    },
    async onSubmit() {
      if (this.isEdit) {
        await axiosConfig.postToApi(
          `/categories/UpsertCategory`,
          {
            maDanhMucCha: this.form.maDanhMucCha,
            tenDanhMucCha: this.form.tenDanhMucCha,
            isActive: this.form.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        this.breadcrumbText = 'Cập nhật thành công'
      } else {
        await axiosConfig.postToApi(
          `/categories/UpsertCategory`,
          {
            maDanhMucCha: 0,
            tenDanhMucCha: this.form.tenDanhMucCha,
            isActive: this.form.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        this.breadcrumbText = 'Thêm mới thành công'
      }
      await this.getCategories()
      this.resetForm()
    },
    resetForm() {
      this.isEdit = false
      this.form = {
        maDanhMucCha: null,
        tenDanhMucCha: '',
        isActive: true,
      }
      this.breadcrumbText = 'Thêm mới danh mục'
    },
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableCategories')) {
          $('#datatableCategories').DataTable().destroy()
        }
        this.datatable = $('#datatableCategories').DataTable({
          data: vm.filteredCategories,
          columns: [
            { data: 'maSp', title: 'Mã sản phẩm', className: 'text-center' },
            { data: 'tenSanPham', title: 'Tên sản phẩm', className: 'text-center' },
            {
              data: null,
              title: 'Hành động',
              className: 'text-center',
              orderable: false,
              render: function (data, type, row) {
                return `
                  <button class="btn btn-sm btn-warning me-1 btn-edit">Sửa</button>
                  <button class="btn btn-sm btn-danger btn-delete">Xóa</button>
                `
              },
            },
          ],
          destroy: true,
          language: defaultLanguageDatatable,
        })

        // Sự kiện cho nút Sửa/Xóa
        $('#datatableCategories tbody')
          .off('click')
          .on('click', 'button', function () {
            const rowData = vm.datatable.row($(this).parents('tr')).data()
            if ($(this).hasClass('btn-edit')) {
              vm.onEdit(rowData)
            } else if ($(this).hasClass('btn-delete')) {
              vm.onDelete(rowData)
            }
          })
      })
    },
    reloadDataTable() {
      if (this.datatable) {
        this.datatable.clear()
        this.datatable.rows.add(this.filteredCategories)
        this.datatable.draw()
      } else {
        this.initDataTable()
      }
    },
  },
  watch: {
    selectedMaDanhMucCha() {
      this.selectedMaDanhMucCon = ''
      this.reloadDataTable()
    },
    selectedMaDanhMucCon() {
      this.reloadDataTable()
    },
  },
}
</script>
