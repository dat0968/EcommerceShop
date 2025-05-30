<template>
  <div style="margin-top: 90px" class="xp-contentbar">
    <!-- Breadcrumb trạng thái -->
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5">Quản lý danh mục</li>
      </ol>
    </nav>
    <!-- Chọn chế độ -->
    <div class="mb-3 d-flex align-items-center gap-3">
      <label class="me-2 fw-bold">Chế độ:</label>
      <div class="btn-group" role="group">
        <input
          type="radio"
          class="btn-check"
          id="mode-view"
          value="view"
          v-model="focusMode"
          autocomplete="off"
        />
        <label class="btn btn-outline-primary" for="mode-view">Xem chi tiết sản phẩm</label>
        <input
          type="radio"
          class="btn-check"
          id="mode-parent"
          value="parent"
          v-model="focusMode"
          autocomplete="off"
        />
        <label class="btn btn-outline-success" for="mode-parent">Quản lý danh mục cha</label>
        <input
          type="radio"
          class="btn-check"
          id="mode-child"
          value="child"
          v-model="focusMode"
          autocomplete="off"
        />
        <label class="btn btn-outline-warning" for="mode-child">Quản lý danh mục con</label>
      </div>
    </div>

    <!-- Chế độ xem chi tiết sản phẩm -->
    <div v-show="focusMode === 'view'" class="col-md-12">
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
              {{ item.tenDanhMucCha }} {{ item.isActive ? '✔️' : '❌' }}
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
              {{ item.tenDanhMucCon }} {{ item.isActive ? '✔️' : '❌' }}
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

    <!-- Chế độ quản lý danh mục cha -->
    <div v-show="focusMode === 'parent'" class="row">
      <div class="col-lg-3 col-md-3 col-sm-12 position-relative">
        <div class="card position-sticky start-0 top-5" style="margin-top: 5rem">
          <div class="card-header">
            {{ isEditParent ? 'Cập nhật danh mục cha' : 'Thêm danh mục cha' }}
          </div>
          <div class="card-body">
            <form @submit.prevent="onSubmitParent">
              <div class="mb-3">
                <label class="form-label">Tên danh mục cha</label>
                <input
                  v-model="formParent.tenDanhMucCha"
                  type="text"
                  class="form-control"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label">Trạng thái</label>
                <select v-model="formParent.isActive" class="form-select">
                  <option :value="true">Hoạt động</option>
                  <option :value="false">Không hoạt động</option>
                </select>
              </div>
              <button type="submit" class="btn btn-primary w-100">
                {{ isEditParent ? 'Cập nhật' : 'Thêm mới' }}
              </button>
              <button
                v-if="isEditParent"
                type="button"
                class="btn btn-secondary w-100 mt-2"
                @click="resetFormParent"
              >
                Hủy
              </button>
            </form>
          </div>
        </div>
      </div>
      <div class="col-md-9">
        <div class="mt-4">
          <h5>Danh sách danh mục cha</h5>
          <table
            id="datatableParent"
            class="table table-bordered table-striped"
            style="width: 100%"
          ></table>
        </div>
      </div>
    </div>

    <!-- Chế độ quản lý danh mục con -->
    <div v-show="focusMode === 'child'" class="row">
      <div class="col-lg-3 col-md-3 col-sm-12 position-relative">
        <div class="card position-sticky start-0 top-5" style="margin-top: 5rem">
          <div class="card-header">
            {{ isEditChild ? 'Cập nhật danh mục con' : 'Thêm danh mục con' }}
          </div>
          <div class="card-body">
            <form @submit.prevent="onSubmitChild">
              <div class="mb-3">
                <label class="form-label">Tên danh mục con</label>
                <input
                  v-model="formChild.tenDanhMucCon"
                  type="text"
                  class="form-control"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label">Trạng thái</label>
                <select v-model="formChild.isActive" class="form-select">
                  <option :value="true">Hoạt động</option>
                  <option :value="false">Không hoạt động</option>
                </select>
              </div>
              <button type="submit" class="btn btn-primary w-100">
                {{ isEditChild ? 'Cập nhật' : 'Thêm mới' }}
              </button>
              <button
                v-if="isEditChild"
                type="button"
                class="btn btn-secondary w-100 mt-2"
                @click="resetFormChild"
              >
                Hủy
              </button>
            </form>
          </div>
        </div>
      </div>
      <div class="col-md-9">
        <div class="mt-4">
          <h5>Danh sách danh mục con</h5>
          <table
            id="datatableChild"
            class="table table-bordered table-striped"
            style="width: 100%"
          ></table>
        </div>
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
import * as configsDt from '@/utils/configsDatatable.js'
import ResponseAPI from '@/models/ResponseAPI'
import { formatCurrency } from '@/constants/formatCurrency'

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
      isEditParent: false,
      isEditChild: false,
      formParent: {
        maDanhMucCha: null,
        tenDanhMucCha: '',
        isActive: true,
      },
      formChild: {
        maDanhMucCon: null,
        tenDanhMucCon: '',
        isActive: true,
      },
      breadcrumbText: 'Quản lý danh mục',
      datatable: null,
      datatableParent: null,
      datatableChild: null,
      focusMode: 'view',
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
    await this.loadOption()
    this.initDataTable()
    this.initDataTableParent()
    this.initDataTableChild()
  },
  methods: {
    async loadOption() {
      const resOptionParen = await axiosConfig.getFromApi(
        '/categories/parents',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.optionsParentCategory = resOptionParen.data

      const resOptionChild = await axiosConfig.getFromApi(
        '/categories/childs',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.optionsChildCategory = resOptionChild.data
      this.reloadDataTableParent()
      this.reloadDataTableChild()
    },
    async getCategories() {
      const res = await axiosConfig.getFromApi('/categories', ConfigsRequest.getSkipAuthConfig())
      this.listCategories = res.data
      this.reloadDataTable()
    },
    onFilterChange() {
      this.reloadDataTable()
    },
    // --- Danh mục cha ---
    onEditParent(item) {
      this.isEditParent = true
      this.formParent.maDanhMucCha = item.maDanhMucCha
      this.formParent.tenDanhMucCha = item.tenDanhMucCha
      this.formParent.isActive = item.isActive
      this.breadcrumbText = 'Cập nhật danh mục cha'
    },
    async onDeleteParent(item) {
      if (confirm('Bạn có chắc chắn muốn xóa danh mục cha này?')) {
        const response = await axiosConfig.deleteFromApi(
          `/categories/parent/${item.maDanhMucCha}`,
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(response)) {
          alert('Đã có dữ liệu liên kết với danh mục, xóa thất bại')
        } else {
          this.optionsParentCategory = this.optionsParentCategory.filter(
            (x) => x.maDanhMucCha !== item.maDanhMucCha,
          )
        }
        this.breadcrumbText = 'Đã xóa danh mục cha'
        // await this.loadOption()
        this.resetFormParent()
      }
    },
    async onSubmitParent() {
      if (this.isEditParent) {
        const res = await axiosConfig.postToApi(
          `/categories/parent/${this.formParent.maDanhMucCha}`,
          {
            tenDanhMucCha: this.formParent.tenDanhMucCha,
            isActive: this.formParent.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(res)) {
          alert('Đã có dữ liệu liên kết với danh mục, cập nhật thất bại')
          return
        }
        this.optionsParentCategory = this.optionsParentCategory.map((x) =>
          x.maDanhMucCha === this.formParent.maDanhMucCha ? res.data : x,
        )
        this.breadcrumbText = 'Cập nhật danh mục cha thành công'
      } else {
        const res = await axiosConfig.postToApi(
          `/categories/parent/0`,
          {
            maDanhMucCha: 0,
            tenDanhMucCha: this.formParent.tenDanhMucCha,
            isActive: this.formParent.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(res)) {
          alert('Đã có dữ liệu liên kết với danh mục, thêm mới thất bại')
          return
        }
        this.optionsParentCategory.push(res.data)
        this.breadcrumbText = 'Thêm mới danh mục cha thành công'
      }
      // await this.loadOption()
      this.resetFormParent()
    },
    resetFormParent() {
      this.isEditParent = false
      this.formParent = {
        maDanhMucCha: null,
        tenDanhMucCha: '',
        isActive: true,
      }
      this.breadcrumbText = 'Thêm mới danh mục cha'
    },
    // --- Danh mục con ---
    onEditChild(item) {
      this.isEditChild = true
      this.formChild.maDanhMucCon = item.maDanhMucCon
      this.formChild.tenDanhMucCon = item.tenDanhMucCon
      this.formChild.isActive = item.isActive
      this.breadcrumbText = 'Cập nhật danh mục con'
    },
    async onDeleteChild(item) {
      if (confirm('Bạn có chắc chắn muốn xóa danh mục con này?')) {
        const response = await axiosConfig.deleteFromApi(
          `/categories/child/${item.maDanhMucCon}`,
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(response)) {
          alert('Đã có dữ liệu liên kết với danh mục, xóa thất bại')
          return
        }
        this.optionsChildCategory = this.optionsChildCategory.filter(
          (x) => x.maDanhMucCon !== item.maDanhMucCon,
        )
        this.reloadDataTableChild()
        this.breadcrumbText = 'Đã xóa danh mục con'
        // await this.loadOption()
        this.resetFormChild()
      }
    },
    async onSubmitChild() {
      if (this.isEditChild) {
        // Cập nhật danh mục con
        const res = await axiosConfig.postToApi(
          `/categories/child/${this.formChild.maDanhMucCon}`,
          {
            tenDanhMucCon: this.formChild.tenDanhMucCon,
            isActive: this.formChild.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(res)) {
          alert('Đã có dữ liệu liên kết với danh mục, cập nhật thất bại')
          return
        }
        this.optionsChildCategory = this.optionsChildCategory.map((x) =>
          x.maDanhMucCon === this.formChild.maDanhMucCon ? res.data : x,
        )
        this.reloadDataTableChild()
        this.breadcrumbText = 'Cập nhật danh mục con thành công'
      } else {
        // Thêm mới danh mục con
        const res = await axiosConfig.postToApi(
          `/categories/child/0`,
          {
            maDanhMucCon: 0,
            tenDanhMucCon: this.formChild.tenDanhMucCon,
            isActive: this.formChild.isActive,
          },
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotification(res)) {
          alert('Đã có dữ liệu liên kết với danh mục, thêm mới thất bại')
          return
        }
        this.optionsChildCategory.push(res.data)
        this.reloadDataTableChild()
        this.breadcrumbText = 'Thêm mới danh mục con thành công'
      }
      // await this.loadOption()
      this.resetFormChild()
    },
    resetFormChild() {
      this.isEditChild = false
      this.formChild = {
        maDanhMucCon: null,
        tenDanhMucCon: '',
        isActive: true,
      }
      this.breadcrumbText = 'Thêm mới danh mục con'
    },
    // --- DataTable sản phẩm ---
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableCategories')) {
          $('#datatableCategories').DataTable().destroy()
        }
        this.datatable = $('#datatableCategories').DataTable({
          data: vm.filteredCategories,
          columns: [
            configsDt.defaultTdToShowDetail,
            { data: 'maSp', title: 'Mã sản phẩm', className: 'text-center' },
            { data: 'tenSanPham', title: 'Tên sản phẩm', className: 'text-center' },
            {
              data: null,
              title: 'Danh mục',
              render: function (data, style, row) {
                return `<span class="badge ${row.isActiveDanhMucCha ? 'bg-success' : 'bg-secondary'}">${row.tenDanhMucCha}</span> > <span class="badge  ${row.isActiveDanhMucCon ? 'bg-success' : 'bg-secondary'}">${row.tenDanhMucCon}</span>`
              },
              className: 'text-center',
            },
          ],
          destroy: true,
          language: configsDt.defaultLanguageDatatable,
          initComplete: () => {
            configsDt.attachDetailsControl(`#datatableCategories`, this.formatDetails.bind(this))
          },
        })
      })
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const detailProduct = this.listCategories.find((x) => x.maSp == rowData.maSp)

      const detailsHtml = `
        <div class="container">
            <div class="row p-1">Mô tả: ${detailProduct.moTa}
              <br/>
              <hr/>  
            </div>
            <div class="row mb-3 justify-content-between detail-list">
                ${
                  detailProduct.detailProducts && detailProduct.detailProducts.length > 0
                    ? detailProduct.detailProducts
                        .map(
                          (detail) => `
                                <div class="col-sm-12 col-md-6 col-lg-4 detail-item">
                                    <div class="row border m-1 p-4 shadow rounded bg-white">
                                        <div class="col-4 d-flex align-items-center">
                                            <img src="${detail.imageUrl || '/images/default.png'}" class="img-fluid rounded" alt="Hình ảnh sản phẩm">
                                        </div>
                                        <div class="col-8">
                                            <div class="text-primary flex flex-flow-column justify-content-between"><span class="col-auto">Màu: ${detail.mauSac || '-'}</span> | <span class="col-auto">Size: ${detail.kichThuoc || '-'}</span></div>
                                            <p><strong>Giá:</strong> <span class="text-danger">${formatCurrency(detail.donGia || 0)}</span></p>
                                            <p><strong>Số lượng tồn:</strong> <span class="text-warning">${detail.soLuongTon}</span></p>
                                            <p><strong>Trạng thái:</strong> <span class="${detail.isActive ? 'text-success' : 'text-danger'}">${detail.isActive ? 'Đang bán' : 'Ngừng bán'}</span></p>
                                        </div>
                                    </div>
                                </div>
                            `,
                        )
                        .join('')
                    : '<p>Không có biến thể nào để hiển thị.</p>'
                }
            </div>
        </div>`
      div.html(detailsHtml)
      return div
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
    // --- DataTable danh mục cha ---
    initDataTableParent() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableParent')) {
          $('#datatableParent').DataTable().destroy()
        }
        this.datatableParent = $('#datatableParent').DataTable({
          data: vm.optionsParentCategory,
          columns: [
            { data: 'maDanhMucCha', title: 'Mã danh mục cha', className: 'text-center' },
            { data: 'tenDanhMucCha', title: 'Tên danh mục cha', className: 'text-center' },
            {
              data: 'isActive',
              title: 'Trạng thái',
              render: function (data) {
                return data
                  ? '<span class="badge bg-success">Hoạt động</span>'
                  : '<span class="badge bg-secondary">Không hoạt động</span>'
              },
            },
            {
              data: null,
              title: 'Hành động',
              orderable: false,
              render: function () {
                return `
                  <button class="btn btn-sm btn-warning me-1 btn-edit-parent">Sửa</button>
                  <button class="btn btn-sm btn-danger btn-delete-parent">Xóa</button>
                `
              },
            },
          ],
          destroy: true,
          language: configsDt.defaultLanguageDatatable,
        })

        $('#datatableParent tbody')
          .off('click')
          .on('click', 'button', function () {
            const rowData = vm.datatableParent.row($(this).parents('tr')).data()
            if ($(this).hasClass('btn-edit-parent')) {
              vm.onEditParent(rowData)
            } else if ($(this).hasClass('btn-delete-parent')) {
              vm.onDeleteParent(rowData)
            }
          })
      })
    },
    reloadDataTableParent() {
      if (this.datatableParent) {
        this.datatableParent.clear()
        this.datatableParent.rows.add(this.optionsParentCategory)
        this.datatableParent.draw()
      } else {
        this.initDataTableParent()
      }
    },
    // --- DataTable danh mục con ---
    initDataTableChild() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableChild')) {
          $('#datatableChild').DataTable().destroy()
        }
        this.datatableChild = $('#datatableChild').DataTable({
          data: vm.optionsChildCategory,
          columns: [
            { data: 'maDanhMucCon', title: 'Mã danh mục con', className: 'text-center' },
            { data: 'tenDanhMucCon', title: 'Tên danh mục con', className: 'text-center' },
            {
              data: 'isActive',
              title: 'Trạng thái',
              render: function (data) {
                return data
                  ? '<span class="badge bg-success">Hoạt động</span>'
                  : '<span class="badge bg-secondary">Không hoạt động</span>'
              },
            },
            {
              data: null,
              title: 'Hành động',
              orderable: false,
              render: function () {
                return `
                  <button class="btn btn-sm btn-warning me-1 btn-edit-child">Sửa</button>
                  <button class="btn btn-sm btn-danger btn-delete-child">Xóa</button>
                `
              },
            },
          ],
          destroy: true,
          language: configsDt.defaultLanguageDatatable,
        })

        $('#datatableChild tbody')
          .off('click')
          .on('click', 'button', function () {
            const rowData = vm.datatableChild.row($(this).parents('tr')).data()
            if ($(this).hasClass('btn-edit-child')) {
              vm.onEditChild(rowData)
            } else if ($(this).hasClass('btn-delete-child')) {
              vm.onDeleteChild(rowData)
            }
          })
      })
    },
    reloadDataTableChild() {
      if (this.datatableChild) {
        this.datatableChild.clear()
        this.datatableChild.rows.add(this.optionsChildCategory)
        this.datatableChild.draw()
      } else {
        this.initDataTableChild()
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
    optionsParentCategory() {
      this.reloadDataTableParent()
    },
    optionsChildCategory() {
      this.reloadDataTableChild()
    },
    focusMode(newVal) {
      if (newVal === 'parent') {
        this.resetFormParent()
      }
      if (newVal === 'child') {
        this.resetFormChild()
      }
    },
  },
}
</script>
