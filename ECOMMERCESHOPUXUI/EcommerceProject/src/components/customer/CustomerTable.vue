<template>
  <main class="table" id="customers_table" @click.self="cancelDeleteMultiple">
    <section class="table__header">
      <h1>Danh Sách Khách Hàng</h1>
      <div class="input-group" style="height: 40px;">
        <input type="search" placeholder="Tìm kiếm..." v-model="searchQuery" @input="searchTable">
      </div>
      <div class="export__file">
        <label for="export-file" class="export__file-btn" title="Xuất File"></label>
        <input type="checkbox" id="export-file" v-model="showExportOptions">
        <div class="export__file-options" v-if="showExportOptions">
          <label @click="exportToServer('pdf')" id="toPDF">
            <i class="fas fa-file-pdf"></i> PDF
          </label>
          <label @click="exportToServer('excel')" id="toEXCEL">
            <i class="fas fa-file-excel"></i> EXCEL
          </label>
        </div>
      </div>
      <div class="action-buttons">
        <button class="btn delete-multiple-button" @click="toggleDeleteMultiple">
          <i class="fas fa-trash-alt"></i>
          {{isDeleteMultipleMode ? `Xác nhận xóa (${customers.filter(c => c.isSelected).length})` : 'Xóa nhiều'}}
        </button>
        <button v-if="isDeleteMultipleMode" class="btn cancel-button" @click="cancelDeleteMultiple">
          <i class="fas fa-times"></i> Hủy
        </button>
      </div>
    </section>
    <section class="table__body" style="margin-top: 50px;">
      <table>
        <thead>
          <tr>
            <th v-if="isDeleteMultipleMode" style="width: 5%;">
              <input type="checkbox" v-model="selectAll" @change="toggleSelectAll">
            </th>
            <th @click="sortTable('hoTen')">Họ Tên <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'hoTen', 'asc': !sortAsc && sortColumn === 'hoTen' }">↑</span></th>
            <th @click="sortTable('gioiTinh')">Giới Tính <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'gioiTinh', 'asc': !sortAsc && sortColumn === 'gioiTinh' }">↑</span>
            </th>
            <th @click="sortTable('ngaySinh')">Ngày Sinh <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'ngaySinh', 'asc': !sortAsc && sortColumn === 'ngaySinh' }">↑</span>
            </th>
            <th @click="sortTable('sdt')">SĐT <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'sdt', 'asc': !sortAsc && sortColumn === 'sdt' }">↑</span></th>
            <th @click="sortTable('email')">Email <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'email', 'asc': !sortAsc && sortColumn === 'email' }">↑</span></th>
            <th @click="sortTable('tinhTrang')">Tình Trạng <span class="icon-arrow"
                :class="{ 'active': sortColumn === 'tinhTrang', 'asc': !sortAsc && sortColumn === 'tinhTrang' }">↑</span>
            </th>
            <th>Thao Tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(customer, index) in displayedCustomers" :key="customer.maKH" :class="{ hide: customer.isHidden }"
            :style="{ '--delay': customer.delay, backgroundColor: customer.backgroundColor }">
            <td v-if="isDeleteMultipleMode" style="width: 5%;">
              <input type="checkbox" v-model="customer.isSelected" @change="updateSelectAll">
            </td>
            <td>
              <img :src="getImageUrl(customer.hinh)" alt="">
              <span class="customer-name" @click="showCustomerDetail(customer)">{{ customer.hoTen }}</span>
            </td>
            <td>{{ customer.gioiTinh }}</td>
            <td>{{ customer.ngaySinh ? formatDate(customer.ngaySinh) : '' }}</td>
            <td>{{ customer.sdt }}</td>
            <td>{{ customer.email }}</td>
            <td>
              <p :class="['status', getStatusClass(customer.tinhTrang)]">{{ customer.tinhTrang }}</p>
            </td>
            <td>
              <button @click="editCustomer(customer.maKH)" class="btn-edit">Sửa</button>
              <button @click="deleteCustomer(customer.maKH)" class="btn-delete">Xóa</button>
            </td>
          </tr>
          <tr v-if="displayedCustomers.length === 0">
            <td :colspan="isDeleteMultipleMode ? 8 : 7" class="no-data">Không có dữ liệu khách hàng</td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- Phân trang -->
    <div class="pagination-container">
      <div class="pagination-info">
        <!-- Thông tin phân trang -->
      </div>
      <div class="pagination">
        <button @click="changePage(1)" :disabled="currentPage === 1" class="pagination-button" title="Trang đầu">
          «
        </button>
        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="pagination-button"
          title="Trang trước">
          ‹
        </button>
        <button v-for="page in displayedPages" :key="page" @click="changePage(page)"
          :class="['pagination-button', page === currentPage ? 'active' : '']">
          {{ page }}
        </button>
        <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="pagination-button"
          title="Trang sau">
          ›
        </button>
        <button @click="changePage(totalPages)" :disabled="currentPage === totalPages" class="pagination-button"
          title="Trang cuối">
          »
        </button>
      </div>
    </div>

    <!-- Loading indicator -->
    <div class="loading-overlay" v-if="loading">
      <div class="loading-spinner"></div>
    </div>
    <div class="modal" v-if="showDetailModal" @click.self="closeDetailModal">
      <!-- Modal chi tiết -->
      <div class="modal-container" style="height: 900px; width: 900px;">
        <div class="modal-accent-border"></div>
        <header class="modal-header" id="modal-title" style="font-family: Arial, Helvetica, sans-serif; color: black;">
          Thông Tin Khách Hàng</header>
        <div class="form-columns">
          <!-- Left column -->
          <div class="left-column">
            <!-- Hình đại diện -->
            <div class="flex flex-col items-center sm:items-start">
              <label class="section-title w-full" for="avatar" style="margin-right: 10px;">Hình đại diện</label>
              <img :src="getImageUrl(selectedCustomer.hinh)" alt="Hình đại diện" class="image-preview mt-6"
                loading="lazy" width="150" height="190" aria-hidden="true" />
            </div>
            <!-- Giới tính -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="gender">Giới tính</label>
              <div class="info-value">{{ selectedCustomer.gioiTinh }}</div>
            </div>
            <!-- Số điện thoại -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="phone">Số điện thoại</label>
              <div class="info-value">{{ selectedCustomer.sdt }}</div>
            </div>
            <div class="flex flex-col justify-start">
              <label class="section-title" for="phone">Ngày Tạo</label>
              <div class="info-value">{{ formatDate(selectedCustomer.ngayTao) }}</div>
            </div>
          </div>
          <!-- Right column -->
          <div class="right-column">
            <!-- Họ tên -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="fullname">Họ tên</label>
              <div class="info-value">{{ selectedCustomer.hoTen }}</div>
            </div>
            <!-- Ngày sinh -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="dob">Ngày sinh</label>
              <div class="info-value">{{ formatDate(selectedCustomer.ngaySinh) }}</div>
            </div>
            <!-- CCCD -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="cccd">CCCD</label>
              <div class="info-value">{{ selectedCustomer.cccd }}</div>
            </div>
            <div class="flex flex-col justify-start">
              <label class="section-title" for="address">Địa chỉ</label>
              <div class="info-value">{{ selectedCustomer.diaChi }}</div>
            </div>
            <!-- Email -->
            <div class="flex flex-col justify-start">
              <label class="section-title" for="email">Email</label>
              <div class="info-value">{{ selectedCustomer.email }}</div>
            </div>
            <!-- Tình trạng -->
            <div class="flex flex-col justify-start mt-auto">
              <label class="section-title" for="status">Tình trạng</label>
              <div class="info-value">
                <span :class="['status-tag', getStatusClass(selectedCustomer.tinhTrang)]">
                  {{ selectedCustomer.tinhTrang }}
                </span>
              </div>
            </div>
          </div>
        </div>
        <!-- Buttons -->
        <div class="buttons">
          <button type="button" class="btn-cancel" @click="closeDetailModal" aria-label="Đóng" tabindex="0">
            Đóng
          </button>
          <button type="button" class="btn-submit" @click="editCustomer(selectedCustomer.maKH)" aria-label="Sửa"
            tabindex="0">
            Sửa
          </button>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue';
import axios from 'axios';
import Swal from 'sweetalert2';

export default {
  name: 'CustomerTable',
  emits: ['edit-customer', 'refresh-data'],
  setup(props, { emit }) {
    const customers = ref([]);
    const searchQuery = ref('');
    const sortColumn = ref('maKH');
    const sortAsc = ref(true);
    const showExportOptions = ref(false);
    const apiUrl = 'https://localhost:7217';
    const loading = ref(false);
    const currentPage = ref(1);
    const pageSize = ref(6);
    const totalItems = ref(0);
    const totalPages = ref(1);
    const maxPagesToShow = ref(5);
    const filterHoTen = ref('');
    const filterGioiTinh = ref('');
    const filterGmail = ref('');
    const filterDienThoai = ref('');
    const filterDiaChi = ref('');
    const filterTinhTrang = ref('');
    const showDetailModal = ref(false);
    const selectedCustomer = ref(null);
    // Biến cho chế độ xóa nhiều
    const isDeleteMultipleMode = ref(false);
    const selectAll = ref(false);

    const showCustomerDetail = (customer) => {
      selectedCustomer.value = { ...customer };
      showDetailModal.value = true;
    };

    const closeDetailModal = () => {
      showDetailModal.value = false;
      selectedCustomer.value = null;
    };

    const fetchCustomers = async () => {
      try {
        loading.value = true;

        Swal.fire({
          title: 'Đang tải dữ liệu...',
          allowOutsideClick: false,
          didOpen: () => {
            Swal.showLoading();
          }
        });

        filterHoTen.value = searchQuery.value;
        filterDiaChi.value = searchQuery.value;
        filterDienThoai.value = searchQuery.value;
        filterGmail.value = searchQuery.value;

        const response = await axios.get(`${apiUrl}/api/Customer`, {
          params: {
            pageSize: pageSize.value,
            pageNumber: currentPage.value,
            hoTen: filterHoTen.value,
            gioiTinh: filterGioiTinh.value,
            tinhTrang: filterTinhTrang.value,
            isActive: true,
            diaChi: filterDiaChi.value,
            email: filterGmail.value,
            sdt: filterDienThoai.value
          }
        });

        customers.value = response.data.map(customer => ({
          ...customer,
          isHidden: false,
          delay: '0s',
          backgroundColor: 'transparent',
          isSelected: false
        }));

        const totalCount = response.headers['x-total-count'];
        if (totalCount) {
          totalItems.value = parseInt(totalCount);
        } else {
          try {
            const countResponse = await axios.get(`${apiUrl}/api/Customer/count`, {
              params: {
                hoTen: filterHoTen.value,
                gioiTinh: filterGioiTinh.value,
                tinhTrang: filterTinhTrang.value,
                isActive: true,
                diaChi: filterDiaChi.value,
                email: filterGmail.value,
                sdt: filterDienThoai.value
              }
            });
            totalItems.value = countResponse.data;
          } catch (error) {
            console.error('Lỗi khi lấy tổng số khách hàng:', error);
            const itemsReturned = customers.value.length;
            if (itemsReturned < pageSize.value) {
              totalItems.value = (currentPage.value - 1) * pageSize.value + itemsReturned;
            }
          }
        }

        totalPages.value = Math.ceil(totalItems.value / pageSize.value) || 1;

        searchTable();

        Swal.close();
      } catch (error) {
        console.error('Lỗi khi lấy dữ liệu khách hàng:', error);
        Swal.fire({
          title: 'Lỗi!',
          text: 'Không thể tải dữ liệu khách hàng. Vui lòng thử lại sau.',
          icon: 'error',
          confirmButtonColor: '#f44336'
        });
      } finally {
        loading.value = false;
      }
    };

    const getImageUrl = (relativePath) => {
      if (!relativePath) return 'Không ảnh';

      if (relativePath.includes('AnhKhachHang')) {
        const fileName = relativePath.split('/').pop();
        return `${apiUrl}/api/Customer/image/${fileName}`;
      }

      return `${apiUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
    };

    const displayedCustomers = computed(() => {
      return customers.value;
    });

    const displayedPages = computed(() => {
      const maxPages = maxPagesToShow.value;
      const pages = [];

      if (totalPages.value <= maxPages) {
        for (let i = 1; i <= totalPages.value; i++) {
          pages.push(i);
        }
      } else {
        const halfMax = Math.floor(maxPages / 2);
        let startPage = Math.max(currentPage.value - halfMax, 1);
        let endPage = Math.min(startPage + maxPages - 1, totalPages.value);

        if (endPage - startPage + 1 < maxPages) {
          startPage = Math.max(endPage - maxPages + 1, 1);
        }

        for (let i = startPage; i <= endPage; i++) {
          pages.push(i);
        }
      }

      return pages;
    });

    const changePage = (page) => {
      if (page >= 1 && page <= totalPages.value && page !== currentPage.value) {
        currentPage.value = page;
        fetchCustomers();
      }
    };

    const onPageSizeChange = () => {
      currentPage.value = 1;
      fetchCustomers();
    };

    const searchTable = () => {
      const query = searchQuery.value.toLowerCase();
      customers.value.forEach((customer, i) => {
        const customerData = [
          customer.hoTen?.toLowerCase() || '',
          customer.sdt?.toLowerCase() || '',
          customer.email?.toLowerCase() || '',
          customer.diaChi?.toLowerCase() || ''
        ].join(' ');

        customer.isHidden = !customerData.includes(query);
        customer.delay = i / 25 + 's';
      });

      const visibleRows = customers.value.filter(customer => !customer.isHidden);
      visibleRows.forEach((customer, i) => {
        customer.backgroundColor = i % 2 === 0 ? 'transparent' : '#0000000b';
      });
    };

    const sortTable = (column) => {
      if (sortColumn.value === column) {
        sortAsc.value = !sortAsc.value;
      } else {
        sortColumn.value = column;
        sortAsc.value = true;
      }

      sortCustomers();
    };

    const sortCustomers = () => {
      customers.value.sort((a, b) => {
        const aValue = a[sortColumn.value];
        const bValue = b[sortColumn.value];

        if (sortColumn.value === 'ngaySinh') {
          const dateA = new Date(aValue);
          const dateB = new Date(bValue);
          return sortAsc.value ? dateA - dateB : dateB - dateA;
        }

        if (aValue < bValue) return sortAsc.value ? -1 : 1;
        if (aValue > bValue) return sortAsc.value ? 1 : -1;
        return 0;
      });

      searchTable();
    };

    const formatDate = (dateString) => {
      if (!dateString) return '';
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '';
      return date.toLocaleDateString('vi-VN');
    };

    const getStatusClass = (status) => {
      if (!status) {
        console.warn('Trạng thái rỗng hoặc không xác định:', status);
        return 'unknown';
      }
      status = status.trim().toLowerCase();
      if (status.includes('hoạt động')) return 'delivered';
      if (status.includes('tạm khóa')) return 'cancelled';
      if (status.includes('chờ')) return 'pending';
      console.warn('Trạng thái không khớp:', status);
      return 'unknown';
    };

    const exportToServer = (fileType) => {
      let url = '';
      if (fileType === 'pdf') {
        url = `${apiUrl}/api/Customer/export/pdf`;
      } else if (fileType === 'excel') {
        url = `${apiUrl}/api/Customer/export/excel`;
      }

      Swal.fire({
        title: `Đang xuất file ${fileType.toUpperCase()}...`,
        allowOutsideClick: false,
        didOpen: () => {
          Swal.showLoading();
        }
      });

      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileType === 'pdf' ? 'DanhSachKhachHang.pdf' : 'DanhSachKhachHang.xlsx');
      document.body.appendChild(link);

      link.onload = () => {
        Swal.close();
      };

      link.onerror = () => {
        Swal.fire({
          title: 'Lỗi!',
          text: `Không thể tải xuống file ${fileType.toUpperCase()}.`,
          icon: 'error',
          confirmButtonColor: '#f44336'
        });
      };

      link.click();
      document.body.removeChild(link);
      showExportOptions.value = false;

      setTimeout(() => {
        Swal.close();
      }, 9000);
    };

    const editCustomer = (id) => {
      emit('edit-customer', id);
    };

    const deleteCustomer = async (id) => {
      Swal.close();

      const dialogResult = await Swal.fire({
        title: 'Xác nhận xóa',
        text: 'Bạn có chắc chắn muốn xóa khách hàng này không?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f44336',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Đồng ý, xóa!',
        cancelButtonText: 'Hủy',
        allowOutsideClick: false,
        allowEscapeKey: false,
        backdrop: `rgba(0,0,0,0.7)`,
        customClass: {
          container: 'swal-overlay-container',
          popup: 'swal-popup-priority',
          title: 'swal-title',
          content: 'swal-content',
          confirmButton: 'swal-confirm'
        },
        target: document.body,
        heightAuto: false,
        onOpen: () => {
          document.querySelector('.swal-overlay-container').style.zIndex = '999999';
        }
      });

      if (dialogResult.isConfirmed) {
        try {
          Swal.fire({
            title: 'Đang xóa...',
            allowOutsideClick: false,
            showConfirmButton: false,
            willOpen: () => {
              Swal.showLoading();
            },
            backdrop: `rgba(0,0,0,0.7)`,
            customClass: {
              container: 'swal-overlay-container'
            },
            target: document.body,
            heightAuto: false,
            onOpen: () => {
              document.querySelector('.swal-overlay-container').style.zIndex = '999999';
            }
          });

          await axios.delete(`${apiUrl}/api/Customer/${id}`);

          const index = customers.value.findIndex(customer => customer.maKH === id);
          if (index !== -1) {
            customers.value[index].isHidden = true;
            await new Promise(resolve => setTimeout(resolve, 500));
            customers.value.splice(index, 1);
          }

          totalItems.value -= 1;
          totalPages.value = Math.ceil(totalItems.value / pageSize.value) || 1;

          if (customers.value.length === 0 && currentPage.value > 1) {
            currentPage.value -= 1;
            await fetchCustomers();
          } else {
            searchTable();
          }

          Swal.close();
          setTimeout(() => {
            Swal.fire({
              title: 'Đã xóa!',
              text: 'Khách hàng đã được xóa thành công.',
              icon: 'success',
              confirmButtonColor: '#4CAF50',
              timer: 3000,
              backdrop: `rgba(0,0,0,0.7)`,
              customClass: {
                container: 'swal-overlay-container',
                popup: 'swal-popup-priority',
              },
              target: document.body,
              heightAuto: false,
              onOpen: () => {
                document.querySelector('.swal-overlay-container').style.zIndex = '999999';
              }
            });
          }, 300);

          emit('refresh-data');
        } catch (error) {
          console.error('Lỗi khi xóa khách hàng:', error);
          Swal.close();
          setTimeout(() => {
            Swal.fire({
              title: 'Lỗi!',
              text: 'Không thể xóa khách hàng. Vui lòng thử lại sau.',
              icon: 'error',
              confirmButtonColor: '#f44336',
              timer: 3000,
              backdrop: `rgba(0,0,0,0.7)`,
              customClass: {
                container: 'swal-overlay-container',
                popup: 'swal-popup-priority',
              },
              target: document.body,
              heightAuto: false,
              onOpen: () => {
                document.querySelector('.swal-overlay-container').style.zIndex = '999999';
              }
            });
          }, 300);
        }
      }
    };

    const toggleDeleteMultiple = async () => {
      if (!isDeleteMultipleMode.value) {
        isDeleteMultipleMode.value = true;
        customers.value.forEach(customer => {
          customer.isSelected = false;
        });
        selectAll.value = false;
        showExportOptions.value = false;
      } else {
        const selectedCustomers = customers.value.filter(customer => customer.isSelected);
        if (selectedCustomers.length === 0) {
          Swal.fire({
            title: 'Chưa chọn khách hàng',
            text: 'Vui lòng chọn ít nhất một khách hàng để xóa.',
            icon: 'warning',
            confirmButtonColor: '#f44336',
            backdrop: `rgba(0,0,0,0.7)`,
            customClass: {
              container: 'swal-overlay-container',
              popup: 'swal-popup-priority',
            },
            target: document.body,
            heightAuto: false,
            onOpen: () => {
              document.querySelector('.swal-overlay-container').style.zIndex = '999999';
            }
          });
          return;
        }

        const dialogResult = await Swal.fire({
          title: 'Xác nhận xóa nhiều',
          text: `Bạn có chắc chắn muốn xóa ${selectedCustomers.length} khách hàng đã chọn không?`,
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#f44336',
          cancelButtonColor: '#6c757d',
          confirmButtonText: 'Đồng ý, xóa!',
          cancelButtonText: 'Hủy',
          allowOutsideClick: false,
          allowEscapeKey: false,
          backdrop: `rgba(0,0,0,0.7)`,
          customClass: {
            container: 'swal-overlay-container',
            popup: 'swal-popup-priority',
            title: 'swal-title',
            content: 'swal-content',
            confirmButton: 'swal-confirm'
          },
          target: document.body,
          heightAuto: false,
          onOpen: () => {
            document.querySelector('.swal-overlay-container').style.zIndex = '999999';
          }
        });

        if (dialogResult.isConfirmed) {
          try {
            Swal.fire({
              title: 'Đang xóa...',
              allowOutsideClick: false,
              showConfirmButton: false,
              willOpen: () => {
                Swal.showLoading();
              },
              backdrop: `rgba(0,0,0,0.7)`,
              customClass: {
                container: 'swal-overlay-container'
              },
              target: document.body,
              heightAuto: false,
              onOpen: () => {
                document.querySelector('.swal-overlay-container').style.zIndex = '999999';
              }
            });

            const deletePromises = selectedCustomers.map(customer =>
              axios.delete(`${apiUrl}/api/Customer/${customer.maKH}`)
            );
            const results = await Promise.allSettled(deletePromises);
            const failed = results.filter(r => r.status === 'rejected').length;

            customers.value = customers.value.filter(customer => !customer.isSelected);
            totalItems.value -= selectedCustomers.length - failed;
            totalPages.value = Math.ceil(totalItems.value / pageSize.value) || 1;

            if (customers.value.length === 0 && currentPage.value > 1) {
              currentPage.value -= 1;
              await fetchCustomers();
            } else {
              searchTable();
            }

            isDeleteMultipleMode.value = false;
            selectAll.value = false;

            Swal.close();
            setTimeout(() => {
              if (failed > 0) {
                Swal.fire({
                  title: 'Cảnh báo',
                  text: `${failed} khách hàng không thể xóa. ${selectedCustomers.length - failed} khách hàng đã xóa thành công.`,
                  icon: 'warning',
                  confirmButtonColor: '#f44336',
                  backdrop: `rgba(0,0,0,0.7)`,
                  customClass: {
                    container: 'swal-overlay-container',
                    popup: 'swal-popup-priority',
                  },
                  target: document.body,
                  heightAuto: false,
                  onOpen: () => {
                    document.querySelector('.swal-overlay-container').style.zIndex = '999999';
                  }
                });
              } else {
                Swal.fire({
                  title: 'Đã xóa!',
                  text: `${selectedCustomers.length} khách hàng đã được xóa thành công.`,
                  icon: 'success',
                  confirmButtonColor: '#4CAF50',
                  timer: 3000,
                  backdrop: `rgba(0,0,0,0.7)`,
                  customClass: {
                    container: 'swal-overlay-container',
                    popup: 'swal-popup-priority',
                  },
                  target: document.body,
                  heightAuto: false,
                  onOpen: () => {
                    document.querySelector('.swal-overlay-container').style.zIndex = '999999';
                  }
                });
              }
            }, 300);

            emit('refresh-data');
          } catch (error) {
            console.error('Lỗi khi xóa nhiều khách hàng:', error);
            Swal.close();
            setTimeout(() => {
              Swal.fire({
                title: 'Lỗi!',
                text: 'Không thể xóa khách hàng. Vui lòng thử lại sau.',
                icon: 'error',
                confirmButtonColor: '#f44336',
                timer: 3000,
                backdrop: `rgba(0,0,0,0.7)`,
                customClass: {
                  container: 'swal-overlay-container',
                  popup: 'swal-popup-priority',
                },
                target: document.body,
                heightAuto: false,
                onOpen: () => {
                  document.querySelector('.swal-overlay-container').style.zIndex = '999999';
                }
              });
            }, 300);
          }
        }
      }
    };

    const toggleSelectAll = () => {
      customers.value.forEach(customer => {
        customer.isSelected = selectAll.value;
      });
    };

    const updateSelectAll = () => {
      selectAll.value = customers.value.every(customer => customer.isSelected);
    };

    const cancelDeleteMultiple = () => {
      isDeleteMultipleMode.value = false;
      customers.value.forEach(customer => {
        customer.isSelected = false;
      });
      selectAll.value = false;
    };

    let searchTimeout = null;
    watch(searchQuery, () => {
      if (searchTimeout) clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        currentPage.value = 1;
        fetchCustomers();
      }, 600);
    });

    onMounted(() => {
      fetchCustomers();
    });

    return {
      customers,
      displayedCustomers,
      searchQuery,
      sortColumn,
      sortAsc,
      showExportOptions,
      searchTable,
      sortTable,
      formatDate,
      getStatusClass,
      exportToServer,
      editCustomer,
      deleteCustomer,
      fetchCustomers,
      getImageUrl,
      loading,
      currentPage,
      pageSize,
      totalItems,
      totalPages,
      displayedPages,
      changePage,
      onPageSizeChange,
      showDetailModal,
      selectedCustomer,
      showCustomerDetail,
      closeDetailModal,
      isDeleteMultipleMode,
      selectAll,
      toggleSelectAll,
      updateSelectAll,
      toggleDeleteMultiple,
      cancelDeleteMultiple
    };
  }
};
</script>

<style scoped>
.table__body {
  width: 95%;
  max-height: calc(75% - 1.6rem);
  background-color: #fffb;
  margin: 0.8rem auto;
  border-radius: 0.6rem;
  overflow-x: auto;
  overflow-y: auto;
  flex: 1;
}

.table__body::-webkit-scrollbar {
  width: 0.5rem;
  height: 0.5rem;
}

.table__body::-webkit-scrollbar-thumb {
  border-radius: 0.5rem;
  background-color: #0004;
  visibility: hidden;
}

.table__body:hover::-webkit-scrollbar-thumb {
  visibility: visible;
}

table {
  width: 100%;
  min-width: 1200px;
  table-layout: fixed;
}

th,
td {
  width: 12.5%;
  padding: 1rem;
  text-align: left;
  border-collapse: collapse;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

thead th {
  position: sticky;
  top: 0;
  left: 0;
  background-color: #d5d1defe;
  cursor: pointer;
  text-transform: capitalize;
}

td img {
  width: 36px;
  height: 36px;
  margin-right: 0.5rem;
  border-radius: 50%;
  vertical-align: middle;
  object-fit: cover;
}

tbody tr {
  transition: 0.5s ease-in-out, background-color 0s;
}

tbody tr.hide {
  opacity: 0;
  transform: translateX(100%);
}

tbody tr.hide td,
tbody tr.hide td p {
  padding: 0;
  font: 0 / 0 sans-serif;
  transition: 0.2s ease-in-out 0.5s;
}

tbody tr.hide td img {
  width: 0;
  height: 0;
  transition: 0.2s ease-in-out 0.5s;
}

tbody tr:hover {
  background-color: #fff6 !important;
}

.status {
  padding: 0.4rem 0;
  border-radius: 2rem;
  text-align: center;
  display: inline-block;
  width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status.delivered {
  background-color: #86e49d;
  color: #006b21;
}

.status.cancelled {
  background-color: #d893a3;
  color: #b30021;
}

.status.pending {
  background-color: #ebc474;
  color: #6b5900;
}

.status.unknown {
  background-color: #d3d3d3;
  color: #333;
}

.no-data {
  text-align: center;
  font-style: italic;
  color: #888;
}

.modal {
  display: flex;
  justify-content: center;
  align-items: center;
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
}

.modal-container {
  background: #f9fafb;
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  max-width: 1000px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  position: relative;
  animation: waveExpand 0.5s ease-out;
}

.modal-container::-webkit-scrollbar {
  width: 8px;
}

.modal-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

.modal-container::-webkit-scrollbar-thumb {
  background: #3b82f6;
  border-radius: 4px;
}

.modal-container::-webkit-scrollbar-thumb:hover {
  background: #2563eb;
}

@keyframes waveExpand {
  0% {
    transform: scale(0.95);
    opacity: 0;
  }

  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.modal-accent-border {
  display: none;
}

.modal-header {
  font-family: 'Roboto', sans-serif;
  font-weight: 700;
  font-size: 1.5rem;
  color: white;
  padding: 1rem 1.5rem;
  background-color: rgb(194, 221, 245);
  border-radius: 1rem 1rem 0 0;
  text-align: center;
  position: sticky;
  top: 0;
  z-index: 10;
}

.form-columns {
  display: flex;
  gap: 1.5rem;
  padding: 1.5rem;
  border-top: none;

  border-bottom: none;
}

.left-column,
.right-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.left-column {
  border-right: none;
}

.section-title {
  font-weight: 600;
  font-size: 0.85rem;
  color: #374151;
  margin-bottom: 0.4rem;
  padding-left: 0;
}

.section-title::before {
  content: none;
}

.info-value {
  font-size: 0.9rem;
  color: #1e293b;
  padding: 0.6rem 0.8rem;
  background-color: #fff;
  border-radius: 0.375rem;
  border: 1px solid #d1d5db;
  box-shadow: none;
  font-weight: 500;
  font-family: 'Roboto', sans-serif;
  width: 100%;
  box-sizing: border-box;
}

.image-preview {
  margin-top: 0.8rem;
  width: 120px;
  height: 160px;
  border-radius: 0.5rem;
  object-fit: cover;
  border: 2px solid #3b82f6;
  box-shadow: 0 3px 5px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s ease;
  align-self: center;
}

.image-preview:hover {
  transform: scale(1.05);
}

.buttons {
  padding: 1.5rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.8rem;
  border-top: 1px solid #e5e7eb;
  border-radius: 0 0 1rem 1rem;
}

.btn-cancel,
.btn-submit {
  padding: 0.6rem 1.2rem;
  border-radius: 0.375rem;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: 'Roboto', sans-serif;
}

.btn-cancel {
  background: #f3f4f6;
  color: #4b5563;
  border: 1px solid #d1d5db;
}

.btn-cancel:hover {
  background: #e5e7eb;
  transform: translateY(-2px);
}

.btn-submit {
  background: #3b82f6;
  color: white;
  border: none;
}

.btn-submit:hover {
  background: #2563eb;
  transform: translateY(-2px);
}

.status-tag {
  padding: 0.4rem 0.8rem;
  border-radius: 0.375rem;
  font-weight: 500;
  font-size: 0.85rem;
}

.status-tag.delivered {
  background-color: #86e49d;
  color: #006b21;
}

.status-tag.cancelled {
  background-color: #d893a3;
  color: #b30021;
}

.status-tag.pending {
  background-color: #ebc474;
}

@media (max-width: 1024px) {
  .modal-container {
    max-width: 90%;
    margin: 1rem auto;
  }

  .form-columns {
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }

  .left-column,
  .right-column {
    padding: 0;
    gap: 0.8rem;
  }

  .modal-header {
    font-size: 1.25rem;
  }
}

@media (max-width: 768px) {
  .modal-container {
    max-height: 85vh;
  }

  .image-preview {
    width: 100px;
    height: 130px;
  }

  .info-value {
    font-size: 0.85rem;
    padding: 0.5rem 0.7rem;
  }

  .section-title {
    font-size: 0.8rem;
  }

  .status-tag {
    font-size: 0.8rem;
    padding: 0.35rem 0.7rem;
  }

  .buttons {
    flex-direction: column;
    gap: 0.5rem;
    padding: 1rem;
  }

  .btn-cancel,
  .btn-submit {
    width: 100%;
    font-size: 0.85rem;
    padding: 0.5rem;
  }
}

@media (max-width: 480px) {
  .modal-container {
    max-width: 95%;
    max-height: 80vh;
  }

  .modal-header {
    font-size: 1.1rem;
  }

  .form-columns {
    padding: 0.8rem;
  }
}

.swal-overlay-container {
  z-index: 999999 !important;
}

.swal-popup-priority {
  position: relative;
  z-index: 1000000 !important;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.5) !important;
}

main.table {
  width: 100%;
  height: 90vh;
  background: url('@/assets/images/html_table.jpg') center / cover;
  background-color: #fff5;
  backdrop-filter: blur(7px);
  box-shadow: 0 .4rem .8rem #0005;
  border-radius: .8rem;
  overflow: hidden;
  margin: 2rem 0;
  display: flex;
  flex-direction: column;
  position: relative;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.table__header {
  width: 100%;
  padding: .8rem 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #fff4;
}

.table__header .input-group {
  width: 35%;
  height: 40px;
  background-color: #fff5;
  padding: 0 .8rem;
  border-radius: 2rem;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: .2s;
}

.table__header .input-group:hover {
  width: 45%;
  background-color: #fff8;
  box-shadow: 0 .1rem .4rem #0002;
}

.table__header .input-group input {
  width: 100%;
  padding: 0 .5rem 0 .3rem;
  background-color: transparent;
  border: none;
  outline: none;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.delete-multiple-button {
  padding: 0.6rem 1.2rem;
  background-color: #f44336;
  color: white;
  border: none;
  border-radius: 0.3rem;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.delete-multiple-button:hover {
  background-color: #d32f2f;
  transform: translateY(-2px);
}

.cancel-button {
  padding: 0.6rem 1.2rem;
  background-color: #6c757d;
  color: white;
  border: none;
  border-radius: 0.3rem;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.cancel-button:hover {
  background-color: #5c636a;
  transform: translateY(-2px);
}

th input[type="checkbox"],
td input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
  vertical-align: middle;
}

.pagination-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  background-color: #fff4;
  padding: 1rem;
  margin-top: auto;
}

.pagination-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  color: #555;
  max-width: 800px;
}

.items-per-page {
  display: flex;
  align-items: center;
}

.items-per-page select {
  margin-left: 0.5rem;
  padding: 0.3rem;
  border: 1px solid #ddd;
  border-radius: 0.3rem;
  font-size: 0.9rem;
  cursor: pointer;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  flex-wrap: wrap;
}

.pagination-button {
  background-color: #fff;
  border: 1px solid #ddd;
  padding: 0.5rem 1rem;
  margin: 0 0.2rem;
  cursor: pointer;
  border-radius: 0.3rem;
  transition: all 0.3s ease;
  min-width: 2.5rem;
  text-align: center;
}

.pagination-button:hover {
  background-color: #f0f0f0;
}

.pagination-button.active {
  background-color: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.pagination-button:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.export__file {
  position: relative;
}

.export__file .export__file-btn {
  display: inline-block;
  width: 2rem;
  height: 2rem;
  background: #fff6 url('@/assets/images/export.png') center / 80% no-repeat;
  border-radius: 50%;
  transition: .2s ease-in-out;
}

.export__file .export__file-btn:hover {
  background-color: #fff;
  transform: scale(1.15);
  cursor: pointer;
}

.export__file input {
  display: none;
}

.export__file .export__file-options {
  position: absolute;
  right: 0;
  width: 12rem;
  border-radius: .5rem;
  overflow: hidden;
  text-align: center;
  z-index: 100;
  box-shadow: 0 .2rem .5rem #0004;
}

.export__file .export__file-options label {
  display: flex;
  width: 100%;
  padding: .6rem 0;
  background-color: #f2f2f2;
  justify-content: center;
  align-items: center;
  transition: .2s ease-in-out;
  cursor: pointer;
  font-size: 0.9rem;
  gap: 0.5rem;
  /* Khoảng cách giữa icon và văn bản */
}

.export__file .export__file-options label:first-of-type {
  padding: .6rem 0;
  /* Đồng bộ với label Excel */
  background-color: #f2f2f2;
  /* Đồng bộ màu nền */
}

.export__file .export__file-options label:hover {
  transform: scale(1.05);
  background-color: #fff;
}

.export__file .export__file-options label i.fa-file-pdf {
  color: #d32f2f;
  /* Màu đỏ cho PDF */
}

.export__file .export__file-options label i.fa-file-excel {
  color: #388e3c;
  /* Màu xanh lá cho Excel */
}

.btn-edit,
.btn-delete {
  padding: 0.3rem 0.6rem;
  border: none;
  border-radius: 0.3rem;
  cursor: pointer;
  font-size: 0.8rem;
  margin: 0 0.2rem;
  transition: transform 0.2s ease-in-out;
}

.btn-edit {
  background-color: #4caf50;
  color: white;
}

.btn-delete {
  background-color: #f44336;
  color: white;
}

.btn-edit:hover,
.btn-delete:hover {
  transform: scale(1.05);
}

thead th span.icon-arrow {
  display: inline-block;
  width: 1.3rem;
  height: 1.3rem;
  border-radius: 50%;
  border: 1.4px solid transparent;
  text-align: center;
  font-size: 1rem;
  margin-left: .5rem;
  transition: .2s ease-in-out;
}

thead th:hover span.icon-arrow {
  border: 1.4px solid #3b82f6;
}

thead th:hover {
  color: #3b82f6;
}

thead th.active span.icon-arrow {
  background-color: #3b82f6;
  color: #fff;
}

thead th.active span.icon-arrow.asc {
  transform: rotate(180deg);
}

thead th.active,
tbody td.active {
  color: #3b82f6;
}

@media print {

  .table,
  .table__body {
    overflow: visible;
    height: auto !important;
    width: auto !important;
  }
}

@media (max-width: 1000px) {
  td:not(:first-of-type) {
    min-width: 12.1rem;
  }

  .pagination-container {
    padding: 0.5rem;
  }

  .pagination-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .pagination {
    flex-wrap: wrap;
    margin-top: 0.5rem;
  }

  .pagination-button {
    margin-bottom: 0.5rem;
    padding: 0.4rem 0.8rem;
    font-size: 0.9rem;
  }
}

.customer-name {
  cursor: pointer;
  color: #3b82f6;
  font-weight: 500;
  transition: all 0.2s;
}

.customer-name:hover {
  text-decoration: underline;
}
</style>