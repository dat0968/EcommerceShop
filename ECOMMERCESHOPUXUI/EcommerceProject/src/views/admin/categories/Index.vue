<template>
  <div class="xp-contentbar position-relative mt-5">
    <Overlay
      :is-visible="isDisabled"
      overlayContent="Hiện không thể kết nối tới API để quản lý."
      isCoverPage="true"
    />
    <nav
      aria-label="breadcrumb"
      class="mb-3"
      style="display: flex; justify-content: center; padding: 10px 0; background-color: transparent"
    >
      <ol class="breadcrumb" style="display: flex; justify-content: center; margin: 0">
        <li class="breadcrumb-item active" style="display: flex; align-items: center">
          <h1 style="text-align: center; margin: 0; font-size: 3rem; font-weight: 700; color: #333">
            QUẢN LÝ DANH MỤC
          </h1>
        </li>
      </ol>
      <hr />
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
          :disabled="isDisabled"
        />
        <label class="btn btn-outline-primary" for="mode-view">Xem chi tiết sản phẩm</label>
        <input
          type="radio"
          class="btn-check"
          id="mode-parent"
          value="parent"
          v-model="focusMode"
          autocomplete="off"
          :disabled="isDisabled"
        />
        <label class="btn btn-outline-success" for="mode-parent">Quản lý danh mục cha</label>
        <input
          type="radio"
          class="btn-check"
          id="mode-child"
          value="child"
          v-model="focusMode"
          autocomplete="off"
          :disabled="isDisabled"
        />
        <label class="btn btn-outline-warning" for="mode-child">Quản lý danh mục con</label>
      </div>
    </div>

    <!-- Bộ lọc danh mục -->
    <div v-show="focusMode === 'view'" class="row">
      <!-- Cột bộ lọc -->
      <div class="col-md-3">
        <div class="card shadow-sm mb-3">
          <div class="card-header bg-light fw-bold"><i class="bi bi-funnel"></i> Bộ lọc</div>
          <div class="card-body">
            <details class="mb-3" open>
              <summary class="form-label bg-light rounded p-1">
                Danh mục cha (Đã chọn {{ selectedMaDanhMucCha.length }})
              </summary>
              <div class="border rounded p-1" style="max-height: 10em; overflow-y: auto">
                <div
                  v-for="item in optionsParentCategory"
                  :key="item.maDanhMucCha"
                  class="form-check"
                >
                  <input
                    type="checkbox"
                    class="form-check-input"
                    :id="'parent-' + item.maDanhMucCha"
                    :value="item.maDanhMucCha"
                    v-model="selectedMaDanhMucCha"
                    @change="onFilterChange"
                  />
                  <label
                    class="form-check-label d-flex justify-content-between"
                    :for="'parent-' + item.maDanhMucCha"
                  >
                    <span>{{ item.tenDanhMucCha }}</span>
                    <span>{{ item.isActive ? '🟢' : '🔴' }}</span>
                  </label>
                </div>
              </div>
            </details>
            <hr />
            <details class="mb-3" open>
              <summary class="form-label bg-light rounded p-1">
                Danh mục con (Đã chọn {{ selectedMaDanhMucCon.length }})
              </summary>
              <div class="border rounded p-1" style="max-height: 10em; overflow-y: auto">
                <div
                  v-for="item in optionsChildCategory"
                  :key="item.maDanhMucCon"
                  class="form-check"
                >
                  <input
                    type="checkbox"
                    class="form-check-input"
                    :id="'child-' + item.maDanhMucCon"
                    :value="item.maDanhMucCon"
                    v-model="selectedMaDanhMucCon"
                    @change="onFilterChange"
                  />

                  <label
                    class="form-check-label d-flex justify-content-between"
                    :for="'child-' + item.maDanhMucCon"
                  >
                    <span>{{ item.tenDanhMucCon }}</span>
                    <span>{{ item.isActive ? '🟢' : '🔴' }}</span>
                  </label>
                </div>
              </div>
            </details>
            <button class="btn btn-outline-secondary w-100" @click="resetFilters">
              <i class="bi bi-x-circle"></i> Xóa lọc
            </button>
          </div>
        </div>
      </div>
      <!-- Cột bảng dữ liệu -->
      <div class="col-md-9">
        <div class="card">
          <div class="card-header text-center">
            <h5>Danh sách danh mục</h5>
          </div>
          <div class="card-body">
            <table
              v-if="filteredCategories.length > 0"
              id="datatableCategories"
              class="table table-bordered table-striped"
              style="width: 100%"
            ></table>
            <NoDataMessage v-else contentText="Không có sản phẩm nào trong danh mục đã chọn." />
          </div>
        </div>
      </div>
    </div>

    <!-- Chế độ quản lý danh mục cha -->
    <div v-show="focusMode === 'parent'" class="row mt-4">
      <div class="col-lg-3 col-md-3 col-sm-12">
        <div class="card">
          <div class="card-header text-center">
            <h5>{{ isEditParent ? 'Cập nhật danh mục cha' : 'Thêm danh mục cha' }}</h5>
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
              <button type="submit" class="btn btn-primary w-100" :disabled="isSubmittingParent">
                <span
                  v-if="isSubmittingParent"
                  class="spinner-border spinner-border-sm"
                  role="status"
                  aria-hidden="true"
                ></span>
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
        <div class="card">
          <div class="card-header text-center">
            <h5>Danh sách danh mục cha</h5>
          </div>
          <div class="card-body">
            <table
              v-if="optionsParentCategory.length > 0"
              id="datatableParent"
              class="table table-bordered table-striped"
              style="width: 100%"
            ></table>
            <NoDataMessage v-else contentText="Không có danh mục cha nào." />
          </div>
        </div>
      </div>
    </div>

    <!-- Chế độ quản lý danh mục con -->
    <div v-show="focusMode === 'child'" class="row mt-4">
      <div class="col-lg-3 col-md-3 col-sm-12 position-relative">
        <div class="card">
          <div class="card-header text-center">
            <h5>{{ isEditChild ? 'Cập nhật danh mục con' : 'Thêm danh mục con' }}</h5>
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
              <button type="submit" class="btn btn-primary w-100" :disabled="isSubmittingChild">
                <span
                  v-if="isSubmittingChild"
                  class="spinner-border spinner-border-sm"
                  role="status"
                  aria-hidden="true"
                ></span>
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
        <div class="card">
          <div class="card-header text-center">
            <h5>Danh sách danh mục con</h5>
          </div>
          <div class="card-body">
            <table
              v-if="optionsChildCategory.length > 0"
              id="datatableChild"
              class="table table-bordered table-striped"
              style="width: 100%"
            ></table>
            <NoDataMessage v-else contentText="Không có danh mục con nào." />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import Swal from 'sweetalert2'

import * as axiosConfig from '@/utils/axiosClient'
import ConfigsRequest from '@/models/ConfigsRequest'
import * as configsDt from '@/utils/configsDatatable.js'
import ResponseAPI from '@/models/ResponseAPI'
import { formatCurrency } from '@/constants/formatCurrency'
import Overlay from '@/components/common/Overlay.vue'
import pathReplaceImg from '@/utils/processPathImg'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

export default {
  name: 'CategoryIndex',
  components: {
    Overlay,
    NoDataMessage,
  },
  data() {
    return {
      listCategories: [],
      optionsParentCategory: [],
      optionsChildCategory: [],
      isLoading: false,
      selectedMaDanhMucCha: [],
      selectedMaDanhMucCon: [],
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
      isEndpointActive: axiosConfig.isEndpointAvailable(), // Biến để kiểm tra kết nối API
      isSubmittingParent: false,
      isSubmittingChild: false,
      blockedCategories: new Map(), // Stores blocked categories and their unblock time
      pendingStatusUpdates: new Map(), // Stores pending debounce timeouts
    }
  },
  computed: {
    filteredCategories() {
      return this.listCategories.filter((x) => {
        let matchCha =
          !this.selectedMaDanhMucCha.length ||
          this.selectedMaDanhMucCha.includes('') ||
          this.selectedMaDanhMucCha.includes(x.maDanhMucCha)
        let matchCon =
          !this.selectedMaDanhMucCon.length ||
          this.selectedMaDanhMucCon.includes('') ||
          this.selectedMaDanhMucCon.includes(x.maDanhMucCon)
        return matchCha && matchCon
      })
    },
    isDisabled() {
      return !this.isEndpointActive
    },
  },
  async mounted() {
    this.isLoading = true
    // Kiểm tra endpoint trước khi load dữ liệu
    this.isEndpointActive = await axiosConfig.isEndpointAvailable?.()
    if (!this.isEndpointActive) {
      Swal.fire({
        icon: 'error',
        title: 'Không có kết nối API',
        text: 'Không thể kết nối tới máy chủ API. Vui lòng kiểm tra lại kết nối hoặc cấu hình endpoint.',
        confirmButtonText: 'Đóng',
      })
      this.isLoading = false
      return
    }
    await this.getCategories()
    await this.loadOption()
    this.initDataTable()
    this.initDataTableParent()
    this.initDataTableChild()
    this.isLoading = false
  },
  methods: {
    async loadOption() {
      const resOptionParen = await axiosConfig.getFromApi(
        '/categories/parents',
        ConfigsRequest.takeAuth(),
      )
      if (ResponseAPI.handleNotificationAndIsFailResponse(resOptionParen)) {
        return
      }
      this.optionsParentCategory = Array.isArray(resOptionParen.data) ? resOptionParen.data : []

      const resOptionChild = await axiosConfig.getFromApi(
        '/categories/childs',
        ConfigsRequest.takeAuth(),
      )
      this.optionsChildCategory = Array.isArray(resOptionChild.data) ? resOptionChild.data : []
      this.reloadDataTableParent()
      this.reloadDataTableChild()
    },
    async getCategories() {
      const res = await axiosConfig.getFromApi(
        '/categories/GetAllCategories',
        ConfigsRequest.takeAuth(),
      )
      this.listCategories = Array.isArray(res.data) ? res.data : []
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
      const result = await Swal.fire({
        title: 'Bạn có chắc chắn muốn xóa?',
        text: `Xóa danh mục cha: "${item.tenDanhMucCha}"`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Vâng, xóa nó!',
        cancelButtonText: 'Hủy',
      })

      if (result.isConfirmed) {
        const response = await axiosConfig.deleteFromApi(
          `/categories/parent/${item.maDanhMucCha}`,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(response, true)) {
          return
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
      this.isSubmittingParent = true
      try {
        if (this.isEditParent) {
          const res = await axiosConfig.postToApi(
            `/categories/parent/${this.formParent.maDanhMucCha}`,
            {
              tenDanhMucCha: this.formParent.tenDanhMucCha,
              isActive: this.formParent.isActive,
            },
            ConfigsRequest.takeAuth(),
          )
          if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
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
            ConfigsRequest.takeAuth(),
          )
          if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
            return
          }
          this.optionsParentCategory = [...this.optionsParentCategory, res.data]
          this.breadcrumbText = 'Thêm mới danh mục cha thành công'
        }
        // await this.loadOption()
        this.resetFormParent()
      } finally {
        this.isSubmittingParent = false
      }
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
      const result = await Swal.fire({
        title: 'Bạn có chắc chắn muốn xóa?',
        text: `Xóa danh mục con: "${item.tenDanhMucCon}"`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Vâng, xóa nó!',
        cancelButtonText: 'Hủy',
      })

      if (result.isConfirmed) {
        const response = await axiosConfig.deleteFromApi(
          `/categories/child/${item.maDanhMucCon}`,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(response, true)) {
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
      this.isSubmittingChild = true
      try {
        if (this.isEditChild) {
          // Cập nhật danh mục con
          const res = await axiosConfig.postToApi(
            `/categories/child/${this.formChild.maDanhMucCon}`,
            {
              tenDanhMucCon: this.formChild.tenDanhMucCon,
              isActive: this.formChild.isActive,
            },
            ConfigsRequest.takeAuth(),
          )
          if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
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
            ConfigsRequest.takeAuth(),
          )
          if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
            return
          }
          this.optionsChildCategory = [...this.optionsChildCategory, res.data]
          this.reloadDataTableChild()
          this.breadcrumbText = 'Thêm mới danh mục con thành công'
        }
        // await this.loadOption()
        this.resetFormChild()
      } finally {
        this.isSubmittingChild = false
      }
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
            configsDt.attachSearchDebounce('#datatableCategories', this.datatable)
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
                                            <img src="${pathReplaceImg(undefined, 'HinhAnh/Products', detail.imageUrl)}" class="img-fluid rounded" alt="Hình ảnh sản phẩm">
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
              className: 'text-center',
              title: 'Trạng thái',
              render: function (data, type, row) {
                const statusText = data ? 'Hoạt động' : 'Không hoạt động';
                const badgeClass = data ? 'bg-success' : 'bg-secondary';
                return `<span class="badge ${badgeClass} status-toggle" data-id="${row.maDanhMucCha}" data-type="parent" data-status="${data}">${statusText}</span>`;
              },
            },
            {
              data: null,
              title: 'Hành động',
              className: "text-center",
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
          initComplete: () => {
            configsDt.attachSearchDebounce('#datatableParent', this.datatableParent);
          },
        })

        $('#datatableParent tbody')
          .off('click', 'button') // Giữ lại off('click', 'button') để tránh xung đột
          .on('click', 'button', function () {
            const rowData = vm.datatableParent.row($(this).parents('tr')).data()
            if ($(this).hasClass('btn-edit-parent')) {
              vm.onEditParent(rowData)
            } else if ($(this).hasClass('btn-delete-parent')) {
              vm.onDeleteParent(rowData)
            }
          })
          .off('click', '.status-toggle') // Tắt sự kiện cũ trên .status-toggle
          .on('click', '.status-toggle', function () {
            const id = $(this).data('id');
            const type = $(this).data('type');
            const status = $(this).data('status');
            vm.toggleStatus(id, type, status);
          });
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
              className: 'text-center',
              title: 'Trạng thái',
              render: function (data, type, row) {
                const statusText = data ? 'Hoạt động' : 'Không hoạt động';
                const badgeClass = data ? 'bg-success' : 'bg-secondary';
                return `<span class="badge ${badgeClass} status-toggle" data-id="${row.maDanhMucCon}" data-type="child" data-status="${data}">${statusText}</span>`;
              },
            },
            {
              data: null,
              title: 'Hành động',
              orderable: false,
              className: "text-center",
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
          initComplete: () => {
            configsDt.attachSearchDebounce('#datatableChild', this.datatableChild);
          },
        })

        $('#datatableChild tbody')
          .off('click', 'button') // Giữ lại off('click', 'button') để tránh xung đột
          .on('click', 'button', function () {
            const rowData = vm.datatableChild.row($(this).parents('tr')).data()
            if ($(this).hasClass('btn-edit-child')) {
              vm.onEditChild(rowData)
            } else if ($(this).hasClass('btn-delete-child')) {
              vm.onDeleteChild(rowData)
            }
          })
          .off('click', '.status-toggle') // Tắt sự kiện cũ trên .status-toggle
          .on('click', '.status-toggle', function () {
            const id = $(this).data('id');
            const type = $(this).data('type');
            const status = $(this).data('status');
            vm.toggleStatus(id, type, status);
          });
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
    resetFilters() {
      this.selectedMaDanhMucCha = []
      this.selectedMaDanhMucCon = []
    },
    async toggleStatus(id, type, currentStatus) {
      const key = `${id}_${type}`;
      const unblockTime = this.blockedCategories.get(key);
      const now = Date.now();

      if (unblockTime && now < unblockTime) {
        const timeLeft = Math.ceil((unblockTime - now) / 1000);
        Swal.fire({
          icon: 'warning',
          title: 'Thao tác quá nhanh!',
          text: `Vui lòng chờ ${timeLeft} giây trước khi thử lại.`, 
          confirmButtonText: 'Đóng',
        });
        return;
      }

      // Store original status for potential rollback
      let originalStatus = currentStatus;

      // Update UI immediately
      let targetArray = type === 'parent' ? this.optionsParentCategory : this.optionsChildCategory;
      let targetItem = targetArray.find(item => (type === 'parent' ? item.maDanhMucCha : item.maDanhMucCon) === id);
      if (targetItem) {
        targetItem.isActive = !currentStatus; // Toggle status immediately
        if (type === 'parent') {
          this.reloadDataTableParent();
        } else {
          this.reloadDataTableChild();
        }
      }

      // Clear any pending update for this item
      if (this.pendingStatusUpdates.has(key)) {
        clearTimeout(this.pendingStatusUpdates.get(key));
        this.pendingStatusUpdates.delete(key);
      }

      // Set a new debounced API call
      const timeoutId = setTimeout(async () => {
        try {
          const endpoint = type === 'parent' ? `/categories/parent/${id}/change-status` : `/categories/child/${id}/change-status`;
          const res = await axiosConfig.patchToApi(endpoint, {}, ConfigsRequest.takeAuth());

          if (ResponseAPI.handleNotificationAndIsFailResponse(res, false)) {
            // API call failed, revert UI and block
            if (targetItem) {
              targetItem.isActive = originalStatus; // Revert to original status
              if (type === 'parent') {
                this.reloadDataTableParent();
              } else {
                this.reloadDataTableChild();
              }
            }
            this.blockedCategories.set(key, now + 120 * 1000); // Block for 2 minutes
            Swal.fire({
              icon: 'error',
              title: 'Lỗi cập nhật!',
              text: res?.message || 'Không thể cập nhật trạng thái. Vui lòng thử lại sau.',
              confirmButtonText: 'Đóng',
            });
          } else {
            // API call successful, no need to do anything as UI is already updated
            /* Swal.fire({
              icon: 'success',
              title: 'Cập nhật thành công!',
              text: `Trạng thái danh mục ${targetItem.tenDanhMucCha || targetItem.tenDanhMucCon} đã được cập nhật.`, 
              showConfirmButton: false,
              timer: 1500
            }); */
            console.log(`Trạng thái danh mục ${targetItem.tenDanhMucCha || targetItem.tenDanhMucCon} đã được cập nhật.`);
          }
        } catch (error) {
          console.error('Error updating status:', error);
          if (targetItem) {
            targetItem.isActive = originalStatus; // Revert to original status on network error
            if (type === 'parent') {
              this.reloadDataTableParent();
            } else {
              this.reloadDataTableChild();
            }
          }
          this.blockedCategories.set(key, now + 120 * 1000); // Block for 2 minutes
          Swal.fire({
            icon: 'error',
            title: 'Lỗi kết nối!',
            text: 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra lại kết nối mạng.',
            confirmButtonText: 'Đóng',
          });
        } finally {
          this.pendingStatusUpdates.delete(key);
        }
      }, 10000); // 10 seconds debounce

      this.pendingStatusUpdates.set(key, timeoutId);
    },
  },
  watch: {
    selectedMaDanhMucCha() {
      // this.selectedMaDanhMucCon = ''
      this.reloadDataTable()
    },
    selectedMaDanhMucCon() {
      // this.selectedMaDanhMucCon = ''
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

<style scoped>
/* Disable pointer events và opacity khi mất kết nối */
.xp-contentbar[disabled] {
  pointer-events: none;
  opacity: 0.6;
}
select > option {
  max-height: 10em;
  overflow-y: auto;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(255, 255, 255, 0.7);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
