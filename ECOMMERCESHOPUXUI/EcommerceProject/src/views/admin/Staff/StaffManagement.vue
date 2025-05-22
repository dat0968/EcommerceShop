<!-- StaffManagement với imports tương đối -->
<template>
    <div class="staff-management" style="margin-top: 70px;">

        <div class="action-buttons">
            <button class="btn add-button" @click="showAddModal = true">
                <i class="fas fa-plus"></i> Thêm Nhân Viên
            </button>
            <div class="export-buttons">
                <button class="btn export-button" @click="exportToServer('pdf')">
                    <i class="fas fa-file-pdf"></i> Xuất PDF
                </button>
                <button class="btn export-button" @click="exportToServer('excel')">
                    <i class="fas fa-file-excel"></i> Xuất Excel
                </button>
            </div>
        </div>

        <StaffTable ref="staffTable" @edit-staff="openEditModal" @refresh-data="refreshData" />

        <!-- Modal thêm nhân viên -->
        <div class="modal" v-if="showAddModal">
            <div class="modal-header">
                <span class="close" @click="showAddModal = false">&times;</span>
            </div>
            <div class="modal-body">
                <StaffForm :isEdit="false" @submit-success="handleAddSuccess" @cancel="showAddModal = false" />
            </div>
        </div>

        <!-- Modal sửa nhân viên -->
        <div class="modal" v-if="showEditModal">
            <div class="modal-header">
                <span class="close" @click="showEditModal = false">&times;</span>
            </div>
            <div class="modal-body">
                <StaffForm :isEdit="true" :staffId="selectedStaffId" @submit-success="handleEditSuccess"
                    @cancel="showEditModal = false" />
            </div>
        </div>
    </div>
</template>

<script>
import { ref } from 'vue';
import StaffTable from '@/components/staff/StaffTable.vue';
import StaffForm from '@/components/staff/StaffForm.vue';
export default {
    name: 'StaffManagement',
    components: {
        StaffTable,
        StaffForm
    },
    setup() {
        const staffTable = ref(null);
        const showAddModal = ref(false);
        const showEditModal = ref(false);
        const selectedStaffId = ref(null);
        const apiUrl = 'https://localhost:7217/api';

        const openEditModal = (staffId) => {
            selectedStaffId.value = staffId;
            showEditModal.value = true;
        };

        const handleAddSuccess = () => {
            showAddModal.value = false;
            refreshData();
        };

        const handleEditSuccess = () => {
            showEditModal.value = false;
            refreshData();
        };

        const refreshData = () => {
            if (staffTable.value) {
                staffTable.value.fetchStaff();
            }
        };

        const exportToServer = (fileType) => {
            let url = '';

            if (fileType === 'pdf') {
                url = `${apiUrl}/Staff/export/pdf`;
            } else if (fileType === 'excel') {
                url = `${apiUrl}/Staff/export/excel`;
            }

            // Tạo một thẻ a để tải file
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', fileType === 'pdf' ? 'DanhSachNhanVien.pdf' : 'DanhSachNhanVien.xlsx');
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        };

        return {
            staffTable,
            showAddModal,
            showEditModal,
            selectedStaffId,
            openEditModal,
            handleAddSuccess,
            handleEditSuccess,
            refreshData,
            exportToServer
        };
    }
};
</script>

<style scoped>
.staff-management {
    background: url('@/assets/images/html_table.jpg') center / cover;
    padding: 1rem 2rem;
    max-width: 2000px;
    margin: 0 auto;
}

h1 {
    color: #333;
    margin-bottom: 1.5rem;
}

.action-buttons {
    display: flex;
    justify-content: space-between;
    margin-bottom: 1rem;
}

.btn {
    padding: 0.6rem 1.2rem;
    border: none;
    border-radius: 0.3rem;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.btn i {
    font-size: 1rem;
}

.add-button {
    background-color: #4CAF50;
    color: white;
}

.export-buttons {
    display: flex;
    gap: 0.5rem;
}

.export-button {
    background-color: #2196F3;
    color: white;
}

/* Modal styles */
.modal {
    position: fixed;
    z-index: 1000;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    overflow: auto;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-content {
    background-color: #fff;
    border-radius: 0.5rem;
    width: 80%;
    max-width: 800px;
    animation: modalopen 0.3s;
}

.modal-header {
    padding: 1rem;
    background-color: #f5f5f5;
    border-top-left-radius: 0.5rem;
    border-top-right-radius: 0.5rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-body {
    padding: 1rem;
}

.close {
    color: #888;
    font-size: 1.5rem;
    font-weight: bold;
    cursor: pointer;
}

.close:hover {
    color: #000;
}

@keyframes modalopen {
    from {
        opacity: 0;
        transform: translateY(-50px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@media (max-width: 768px) {
    .action-buttons {
        flex-direction: column;
        gap: 0.5rem;
    }

    .export-buttons {
        margin-top: 0.5rem;
    }

    .modal-content {
        width: 95%;
    }
}
</style>