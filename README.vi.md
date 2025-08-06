[Tiếng Việt](./README.vi.md) | [English](./README.md) | [中文](./README.zh.md)

# EcommerceShop - Cửa Hàng Quần Áo Trực Tuyến Hiện Đại

Nền tảng thương mại điện tử toàn diện dành cho việc bán quần áo trực tuyến, xây dựng theo kiến trúc hiện đại, tách rời. Dự án có API phần backend mở rộng và giao diện người dùng linh hoạt, mang đến trải nghiệm mua sắm hấp dẫn.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

## Mục Lục

- [Tính Năng](#🚀-tính-năng)
- [Cài Đặt](#🛠️-cài-đặt)
- [Sử Dụng](#🎯-sử-dụng)
- [Cấu Hình](#⚙️-cấu-hình)
- [Kiến Trúc](#🏗️-kiến-trúc)
- [Cấu Trúc Dự Án](#📂-cấu-trúc-dự-án)
- [Tác Giả](#👥-tác-giả)
- [Giấy Phép](#📜-giấy-phép)

---

## 🚀 Tính Năng

- 🛒 **Thư Viện Sản Phẩm:** Duyệt sản phẩm theo danh mục, xem chi tiết mô tả, hình ảnh rõ nét.
- 🔐 **Xác Thực Khách Hàng:** Đăng nhập, đăng ký an toàn qua JWT và xác thực Google.
- 🛍️ **Giỏ Hàng & Quản Lý Đơn Hàng:** Thêm, sửa, xóa sản phẩm, đặt hàng, xem lịch sử mua, theo dõi trạng thái.
- 💳 **Thanh Toán An Toàn:** Tích hợp thanh toán qua VNPAY.
- 📊 **Bảng Điều Khiển Quản Trị:** Quản lý sản phẩm, danh mục, khách hàng, đơn hàng.
- 💡 **Hệ Thống Gợi Ý:** Đề xuất sản phẩm cá nhân hoá dựa trên ML.NET.
- 📧 **Dịch Vụ Gửi Email:** Tự động gửi xác nhận đơn hàng, thông báo, mã giảm giá.
- 🌟 **Các Tính Năng Nâng Cao Khác:**
  - So sánh sản phẩm (có API AI thử thử ảo)
  - Vòng quay may mắn giảm giá
  - Chatbot hỗ trợ và tư vấn khách hàng
  - Xem lịch sử duyệt gần nhất

---

## 🛠️ Cài Đặt

**Yêu cầu hệ thống**:
- Windows 10/11
- PowerShell 5.1 trở lên
- .NET 8 SDK
- Node.js (phiên bản LTS khuyên dùng)
- Vue CLI (`npm install -g @vue/cli`)
- SQL Server hoặc SQL Server Express/LocalDB

**Các bước thực hiện**:

```powershell
# 1. Clone kho chứa mã nguồn
git clone <url_kh_oọdọ>  # Thay thế bằng URL repo của bạn
cd EcommerceShop

# Backend - API
cd APIClothesEcommerceShop/APIClothesEcommerceShop
# Cấu hình kết nối Database:
# Mở file appsettings.json chỉnh sửa chuỗi kết nối SQL Server của bạn
# Ví dụ:
# "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# Lưu file

# Khôi phục thư viện và cập nhật database
dotnet restore
dotnet ef database update

# Chạy API backend
dotnet run
# API sẽ có thể truy cập tại https://localhost:7217/swagger hoặc http://localhost:7218/swagger

# Frontend - Giao diện người dùng
cd ../../ECOMMERCESHOPUXUI/EcommerceProject
npm install
npm run dev
# Truy cập tại http://localhost:5173
```

---

## 🎯 Sử Dụng

Sau khi cả hai dịch vụ backend và frontend đều hoạt động:

- Mở trình duyệt truy cập [http://localhost:5173](http://localhost:5173)
- Sử dụng giao diện thân thiện để duyệt sản phẩm, đăng ký/đăng nhập, thêm sản phẩm vào giỏ, thanh toán qua VNPAY.

**Các chức năng chính:**

- Duyệt và tìm kiếm sản phẩm
- Quản lý giỏ hàng và thanh toán
- Đăng ký, đăng nhập, quản lý hồ sơ cá nhân
- So sánh sản phẩm với thử ảo AI
- Vòng quay may mắn mã giảm giá
- Chatbot hỗ trợ trực tuyến
- Xem lịch sử đơn hàng và hoạt động gần nhất
- Quản trị sản phẩm, đơn hàng, người dùng, phân tích dữ liệu

---

## ⚙️ Cấu Hình

- Cấu hình API keys cho dịch vụ như Cloudinary, Gemini AI, Firebase, VNPAY trong các biến môi trường (`.env`)
- Cập nhật chuỗi kết nối database trong `appsettings.json`
- Cấu hình API của bên thứ ba cho AI, thanh toán, email, gửi thông báo

---

## 🏗️ Kiến Trúc Hệ Thống

Hệ thống theo kiến trúc tách biệt, client-server:

- **API Backend:** Viết bằng .NET 8 Web API, quản lý logic nghiệp vụ, dữ liệu, tích hợp dịch vụ thứ ba và cung cấp API RESTful.
- **Giao diện Frontend:** Phát triển bằng Vue.js 3, dùng Vite làm công cụ build, Pinia quản lý trạng thái, Vue Router điều hướng, Bootstrap 5 UI. Tạo trải nghiệm tương tác nhanh, mượt mà.

---

## 📂 Cấu Trúc Dự Án

```
/
├── APIClothesEcommerceShop/                # API Backend
│   ├── Controllers/                        # Các điểm cuối API
│   ├── DTO/                                # Data Transfer Objects
│   ├── Data/                               # DbContext, Migration
│   ├── Models/                             # Mô hình dữ liệu
│   ├── Repositories/                       # Lớp tiếp cận dữ liệu
│   └── Services/                           # Logic nghiệp vụ
└── ECOMMERCESHOPUXUI/                        # Frontend Vue.js
    └── EcommerceProject/
        ├── src/
        │   ├── assets/                     # Hình ảnh, tài nguyên
        │   ├── components/                 # Components dùng chung
        │   ├── views/                      # Các trang view
        │   ├── router/                     # Cấu hình định tuyến
        │   ├── stores/                     # Quản lý trạng thái
        │   └── services/                   # Call API
        └── package.json
```

---


## Tác giả

- **Nhóm đóng góp chính**: Silent Stack Team

- **Người đóng góp**:
<p align="center">
    <a href="https://github.com/dat0968/EcommerceShop/graphs/contributors">
      <img src="https://contrib.rocks/image?repo=dat0968/EcommerceShop" style="max-width: 400px;" />
    </a>
</p>


---

## 📜 Giấy Phép

Dự án được cấp phép theo Giấy phép MIT. Xem chi tiết tại [LICENSE.md](LICENSE.md).
