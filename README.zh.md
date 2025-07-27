[Tiếng Việt](./README.vi.md) | [English](./README.md) | [中文](./README.zh.md)

# 电子商务商店项目
![构建状态](https://img.shields.io/badge/build-passing-brightgreen)
![许可证](https://img.shields.io/badge/license-MIT-blue)

> 一个全面的电子商务平台，具有.NET 8.0后端API和Vue.js前端应用程序。

## 目录
- [功能](#🚀-功能)
- [安装](#🛠️-安装)
- [使用](#🎯-使用)
- [配置](#⚙️-配置)
- [架构](#🏗️-架构)
- [作者](#👥-作者)
- [许可证](#📜-许可证)

## 🚀 功能
- 🛒 **产品目录**：浏览和搜索带有详细描述和图片的产品。
- 🔐 **用户认证**：使用JWT和Google认证进行安全的用户注册和登录。
- 🛍️ **购物车和订单管理**：添加、更新、从购物车中删除商品，下订单，查看历史记录和跟踪状态。
- 💳 **支付集成**：通过VNPAY进行安全的支付处理。
- 📊 **管理面板**：管理产品、类别、用户和订单（如果确认将添加详细信息）。
- 💡 **推荐系统**：可能提供产品推荐。
- 📧 **电子邮件服务**：集成用于发送电子邮件（例如，订单确认）。

## 🛠️ 安装
本节提供在本地计算机上设置和运行项目以进行开发和测试的说明。

**系统要求**：
- Windows 10/11
- PowerShell 5.1或更高版本
- .NET 8.0 SDK
- Node.js (推荐LTS版本)
- Vue CLI (全局安装：`npm install -g @vue/cli`)
- SQL Server (或SQL Server Express/LocalDB)
- SQL Server Management Studio (SSMS) (可选，用于数据库管理)

**步骤**：
```powershell
# 步骤 1: 克隆仓库
git clone <repository_url> # 将 <repository_url> 替换为实际的URL
cd EcommerceShop

# 步骤 2: 后端设置 (APIClothesEcommerceShop)
# 导航到后端项目目录
cd APIClothesEcommerceShop/APIClothesEcommerceShop

# 配置数据库连接:
# 打开 appsettings.json (和 appsettings.Development.json) 并更新 ConnectionStrings 部分。
# 示例: "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# 如果不同，请将 '.' 替换为您的SQL Server实例名称。

# 应用迁移和种子数据
dotnet ef database update

# 运行后端API
dotnet run

# 步骤 3: 前端设置 (ECOMMERCESHOPUXUI)
# 导航到前端项目目录
cd ../../ECOMMERCESHOPUXUI/EcommerceProject

# 安装依赖项
npm install

# 运行Vue.js应用程序
npm run serve
```

## 🎯 使用

成功设置后端和前端后，您可以通过Web浏览器访问应用程序。前端应用程序提供用户界面，用于浏览产品、将商品添加到购物车、管理订单和进行支付。

### 访问应用程序
*   **前端**：打开您的Web浏览器并导航到 `http://localhost:8080/`（或运行 `npm run serve` 后前端控制台中显示的地址）。
*   **后端API文档**：有关详细的API端点和测试，请在浏览器中访问 `https://localhost:7217/swagger/index.html` 或 `http://localhost:7218/swagger/index.html`。

### 主要功能使用
*   **产品浏览**：通过类别导航或使用搜索栏查找产品。
*   **购物和结账**：将商品添加到购物车，进行结账，并通过VNPAY完成支付。
*   **用户账户**：注册、登录、管理您的个人资料和查看订单历史记录。
*   **产品比较（AI驱动）**：利用 `CompareProduct` 功能比较商品，包括虚拟试穿（确保AI API密钥已配置）。
*   **幸运轮优惠券**：参与 `WheelCoupon` 功能以赢取折扣代码。
*   **聊天/聊天机器人**：通过聊天或聊天机器人与系统互动以获取支持或查询。
*   **浏览历史**：跟踪最近浏览的产品。
*   **管理功能**：（供员工/管理员访问）管理产品、类别、用户、订单、员工、查看统计数据和上传图像。

特定功能（例如，管理功能）的详细使用说明将在此处添加。

## ⚙️ 配置
（有关配置应用程序的信息，例如API密钥、环境变量，将在此处。）

## 🏗️ 架构
（项目架构概述，例如图表、组件分解，将在此处。）

## 👥 作者

*   Silent Stack - 初始工作和开发


## 📜 许可证
本项目根据MIT许可证授权 - 有关详细信息，请参阅[LICENSE.md](LICENSE.md)文件。