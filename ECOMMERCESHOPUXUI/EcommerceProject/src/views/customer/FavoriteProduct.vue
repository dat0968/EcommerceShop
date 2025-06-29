<script>
import { ref, onMounted } from 'vue';
import Cookies from 'js-cookie';
import { jwtDecode } from 'jwt-decode';
import Swal from 'sweetalert2';
function ReadToken(token) {
    if (token) {
        const decoded = jwtDecode(token);
        return {
            IdUser: decoded.sub,
            Phone: decoded.PhoneNumber,
            Name: decoded.FullName,
            Role: decoded.role,
            Exp: decoded.exp // Đơn vị giây
        };
    }
    return null;
}

export default {
    name: 'FavoriteList',
    setup() {
        const token = Cookies.get('accessToken');
        const decodedToken = ReadToken(token);
        const idKhachHang = decodedToken ? decodedToken.IdUser : null;

        const idKhachHangRef = ref(idKhachHang || 1);
        const favorites = ref({});
        const message = ref('');
        const success = ref(false);
        const getApiUrl = ref('https://localhost:7217'); // URL cơ sở cho API, điều chỉnh nếu cần

        const fetchFavorites = async () => {
            
            try {
                if (!idKhachHangRef.value) {
                    throw new Error('ID khách hàng không hợp lệ.');
                }
                const response = await fetch(`https://localhost:7217/api/Favorite/GetFavoriteProducts?idKhachHang=${idKhachHangRef.value}`);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data = await response.json();
                console.log('API Response:', data);
                favorites.value = {
                    success: data.success || false,
                    message: data.message || 'Không có thông tin.',
                    data: Array.isArray(data.data) ? data.data : []
                };
                message.value = '';
            } catch (error) {
                console.error('Error:', error);
                favorites.value = { success: false, message: 'Lỗi khi lấy danh sách sản phẩm yêu thích.' };
                message.value = error.message || 'Lỗi không xác định.';
                success.value = false;
            }
        };

        const deleteFavorite = async (idSp) => {
            const result = await Swal.fire({
                title: 'Bạn có chắc muốn xóa?',
                text: 'Sản phẩm sẽ bị xóa khỏi danh sách yêu thích.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy'
            });

            if (!result.isConfirmed) return;
            try {
                if (!idKhachHangRef.value) {
                    throw new Error('ID khách hàng không hợp lệ.');
                }
                const response = await fetch('https://localhost:7217/api/Favorite/DeleteFavoriteProducts', {
                    method: 'DELETE',
                    headers: {
                    'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                    maKh: idKhachHangRef.value,
                    maSp: idSp,
                    
                    })

                })
                
                const data = await response.json();
                message.value = data.message;
                success.value = true;
                await fetchFavorites(); Swal.fire({
                    icon: 'success',
                    title: 'Đã xóa!',
                    text: data.message || 'Sản phẩm đã được xóa khỏi danh sách yêu thích.',
                    timer: 2000,
                    showConfirmButton: false,
                }); // Cập nhật lại danh sách sau khi xóa
            } catch (error) {
                message.value = error.message || 'Lỗi khi xóa sản phẩm yêu thích.';
                success.value = false;
            }
        };

        // Tự động load khi component được mount
        onMounted(() => {
            fetchFavorites();
        });

        return {
            idKhachHangRef,
            favorites,
            message,
            success,
            deleteFavorite,
            getApiUrl,
        };
    },
};
</script>

<template>
    <div class="favorite-container">
        <h2>Danh sách sản phẩm yêu thích</h2>


        <!-- Hiển thị bảng -->
        <table v-if="favorites.data && favorites.data.length > 0" class="favorite-table">
            <thead>
                <tr>
                    <th>Hình Ảnh</th>
                    <th>Tên Sản Phẩm</th>
                    <th>Giá (VNĐ)</th>
                    <th>Chi tiết</th>
                    <th>Hành Động</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in favorites.data" :key="item.maSp" class="favorite-item">
                    <td>
                        <img :src="getApiUrl + '/HinhAnh/Products/' + item.hinhAnh" alt="Product Image" width="50"
                            height="50" style="object-fit: cover; border-radius: 5px" />
                    </td>
                    <td>{{ item.tenSanPham }}</td>
                    <td>{{ item.khoangGia }}</td>
                    <td>
                        <router-link :to="`/product/${item.maSp}`" class="detail-btn">
                            Xem chi tiết
                        </router-link>
                    </td>
                    <td><button @click="deleteFavorite(item.maSp)" class="delete-btn">Xóa</button></td>
                </tr>
            </tbody>
        </table>
        <p v-else-if="favorites.message && !favorites.success" class="error-message">
            {{ favorites.message }}
        </p>
        <p v-else class="no-data">Không có sản phẩm yêu thích.</p>

        <!-- Thông báo -->
        <p v-if="message" :class="{ 'success': success, 'error': !success }">{{ message }}</p>
    </div>
</template>

<style scoped>
.favorite-container {
    max-width: 800px;
    margin: 20px auto;
    padding: 20px;
    font-family: Arial, sans-serif;
}

.input-group {
    margin-bottom: 20px;
}

p {
    margin: 0 10px 10px 0;
}

.detail-btn {
    padding: 5px 10px;
    background-color: #2196F3;
    color: white;
    border: none;
    border-radius: 3px;
    text-decoration: none;
    display: inline-block;
    text-align: center;
}

.detail-btn:hover {
    background-color: #1976D2;
}

.favorite-table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 20px;
}

.delete-btn {
    padding: 5px 10px;
    background-color: #f44336;
    color: white;
    border: none;
    cursor: pointer;
    border-radius: 3px;
}

.favorite-table th,
.favorite-table td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
}

.favorite-table th {
    background-color: #f2f2f2;
}

.delete-btn {
    padding: 5px 10px;
    background-color: #f44336;
    color: white;
    border: none;
    cursor: pointer;
}

.delete-btn:hover {
    opacity: 0.8;
}

.error-message,
.no-data {
    color: #f44336;
}

.success {
    color: #4CAF50;
}

.error {
    color: #f44336;
}
</style>