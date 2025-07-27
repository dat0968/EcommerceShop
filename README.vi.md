[Tiếng Việt](./README.vi.md) | [English](./README.md) | [中文](./README.zh.md)

# Dự án Cửa hàng Thương mại Điện tử
![Trạng thái Build](https://img.shields.io/badge/build-passing-brightgreen)
![Giấy phép](https://img.shields.io/badge/license-MIT-blue)

> Một nền tảng thương mại điện tử toàn diện với API backend .NET 8.0 và ứng dụng frontend Vue.js.

## Bảng mục lục
- [Tính năng](#🚀-tính-năng)
- [Cài đặt](#🛠️-cài-đặt)
- [Sử dụng](#🎯-sử-dụng)
- [Cấu hình](#⚙️-cấu-hình)
- [Kiến trúc](#🏗️-kiến-trúc)
- [Tác giả](#👥-tác-giả)
- [Giấy phép](#📜-giấy-phép)

## 🚀 Tính năng
- 🛒 **Danh mục sản phẩm**: Duyệt và tìm kiếm sản phẩm với mô tả chi tiết và hình ảnh.
- 🔐 **Xác thực người dùng**: Đăng ký và đăng nhập người dùng an toàn bằng JWT và Xác thực Google.
- 🛍️ **Giỏ hàng & Quản lý đơn hàng**: Thêm, cập nhật, xóa các mặt hàng khỏi giỏ hàng, đặt hàng, xem lịch sử và theo dõi trạng thái.
- 💳 **Tích hợp thanh toán**: Xử lý thanh toán an toàn qua VNPAY.
- 📊 **Bảng điều khiển quản trị**: Quản lý sản phẩm, danh mục, người dùng và đơn hàng (chi tiết sẽ được thêm nếu xác nhận).
- 💡 **Hệ thống đề xuất**: Có khả năng cung cấp các đề xuất sản phẩm.
- 📧 **Dịch vụ Email**: Tích hợp để gửi email (ví dụ: xác nhận đơn hàng).

## 🛠️ Cài đặt
Phần này cung cấp hướng dẫn để bạn có thể thiết lập và chạy dự án trên máy cục bộ cho mục đích phát triển và thử nghiệm.

**Yêu cầu hệ thống**:
- Windows 10/11
- PowerShell 5.1 trở lên
- .NET 8.0 SDK
- Node.js (khuyến nghị phiên bản LTS)
- Vue CLI (Cài đặt toàn cục: `npm install -g @vue/cli`)
- SQL Server (hoặc SQL Server Express/LocalDB)
- SQL Server Management Studio (SSMS) (Tùy chọn, để quản lý cơ sở dữ liệu)

**Các bước thực hiện**:
```powershell
# Bước 1: Clone kho lưu trữ
git clone <repository_url> # Thay thế <repository_url> bằng URL thực tế
cd EcommerceShop

# Bước 2: Thiết lập Backend (APIClothesEcommerceShop)
# Điều hướng đến thư mục dự án backend
cd APIClothesEcommerceShop/APIClothesEcommerceShop

# Cấu hình kết nối cơ sở dữ liệu:
# Mở appsettings.json (và appsettings.Development.json) và cập nhật phần ConnectionStrings.
# Ví dụ: "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# Thay thế '.' bằng tên phiên bản SQL Server của bạn nếu khác.

# Áp dụng Migrations và Dữ liệu mẫu
dotnet ef database update

# Chạy API Backend
dotnet run

# Bước 3: Thiết lập Frontend (ECOMMERCESHOPUXUI)
# Điều hướng đến thư mục dự án frontend
cd ../../ECOMMERCESHOPUXUI/EcommerceProject

# Cài đặt các phụ thuộc
npm install

# Chạy ứng dụng Vue.js
npm run serve
```

## 🎯 Sử dụng
## 🎯 Sử dụng

Sau khi thiết lập thành công backend và frontend, bạn có thể truy cập ứng dụng thông qua trình duyệt web của mình. Ứng dụng frontend cung cấp giao diện người dùng để duyệt sản phẩm, thêm mặt hàng vào giỏ hàng, quản lý đơn hàng và thực hiện thanh toán.

### Truy cập ứng dụng
*   **Frontend**: Mở trình duyệt web của bạn và điều hướng đến `http://localhost:8080/` (hoặc địa chỉ hiển thị trong console frontend sau khi chạy `npm run serve`).
*   **Tài liệu API Backend**: Để biết chi tiết các điểm cuối API và thử nghiệm, hãy truy cập `https://localhost:7217/swagger/index.html` hoặc `http://localhost:7218/swagger/index.html` trong trình duyệt của bạn.

### Sử dụng các tính năng chính
*   **Duyệt sản phẩm**: Điều hướng qua các danh mục hoặc sử dụng thanh tìm kiếm để tìm sản phẩm.
*   **Mua sắm & Thanh toán**: Thêm mặt hàng vào giỏ hàng, tiến hành thanh toán và hoàn tất thanh toán qua VNPAY.
*   **Tài khoản người dùng**: Đăng ký, đăng nhập, quản lý hồ sơ và xem lịch sử đơn hàng.
*   **So sánh sản phẩm (có hỗ trợ AI)**: Sử dụng tính năng `CompareProduct` để so sánh các mặt hàng, bao gồm thử đồ ảo (đảm bảo các khóa API AI đã được cấu hình).
*   **Vòng quay may mắn**: Tham gia tính năng `WheelCoupon` để giành mã giảm giá.
*   **Trò chuyện/Chatbot**: Tương tác với hệ thống qua trò chuyện hoặc chatbot để được hỗ trợ hoặc giải đáp thắc mắc.
*   **Lịch sử xem**: Theo dõi các sản phẩm đã xem gần đây.
*   **Tính năng quản trị**: (Dành cho nhân viên/quản trị viên) Quản lý sản phẩm, danh mục, người dùng, đơn hàng, nhân viên, xem số liệu thống kê và tải lên hình ảnh.

Hướng dẫn sử dụng chi tiết cho các tính năng cụ thể sẽ được thêm vào đây.

## ⚙️ Cấu hình
(Thông tin về cấu hình ứng dụng, ví dụ: khóa API, biến môi trường, sẽ được đặt ở đây.)

## 🏗️ Kiến trúc
(Tổng quan về kiến trúc của dự án, ví dụ: sơ đồ, phân tích thành phần, sẽ được đặt ở đây.)

## 👥 Tác giả

*   [Tên của bạn/Tên nhóm] - Công việc ban đầu

Hãy thêm tên hoặc tên nhóm của bạn vào đây!

## 📜 Giấy phép
Dự án này được cấp phép theo Giấy phép MIT - xem tệp [LICENSE.md](LICENSE.md) để biết chi tiết.