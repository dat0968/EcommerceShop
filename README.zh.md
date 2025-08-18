[Tiếng Việt](./README.vi.md) | [English](./README.md) | [中文](./README.zh.md)

# EcommerceShop - 现代在线服装店铺

一个全面的电子商务平台，专为在线销售服装而设计，采用现代化、解耦的架构。该项目具有可扩展的后端API和动态前端，带来极佳的购物体验。

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

## 目录

- [功能特点](#🚀-功能特点)
- [安装指南](#🛠️-安装指南)
- [使用说明](#🎯-使用说明)
- [配置方式](#⚙️-配置方式)
- [架构设计](#🏗️-架构设计)
- [项目结构](#📂-项目结构)
- [作者信息](#👥-作者信息)
- [许可证](#📜-许可证)

---

## 🚀 功能特点

- 🛒 **商品目录：** 按类别浏览商品，查看详细描述和图片
- 🔐 **客户验证：** 通过JWT和Google验证实现安全登录和注册
- 🛍️ **购物车和订单管理：** 添加/修改/删除商品，提交订单，查看订单历史和状态
- 💳 **安全支付集成：** 通过VNPAY进行支付
- 📊 **后台管理面板：** 管理商品、类别、客户和订单
- 💡 **推荐系统：** 利用ML.NET提供个性化商品推荐
- 📧 **邮件服务：** 自动发送订单确认、通知和优惠码
- 🌟 **高级客户功能：** 
  - 商品对比（结合AI API实现虚拟试穿）
  - 转盘抽奖优惠券
  - 聊天机器人支持
  - 查看近期浏览历史

---

## 🛠️ 安装指南

**系统需求：**
- Windows 10/11
- PowerShell 5.1或更高版本
- .NET 8 SDK
- Node.js（建议使用LTS版本）
- Vue CLI（`npm install -g @vue/cli`）
- SQL Server或SQL Server Express/LocalDB

**操作步骤：**

```powershell
# 1. 克隆仓库
git clone <仓库地址>  # 替换为你的仓库链接
cd EcommerceShop

# 后端API
cd APIClothesEcommerceShop/APIClothesEcommerceShop
# 配置数据库连接：
# 编辑appsettings.json文件，填写你的SQL Server连接字符串
# 例：
# "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# 保存文件

# 恢复依赖并更新数据库
dotnet restore
dotnet ef database update

# 运行后端API
dotnet run
# API将访问地址为：https://localhost:7217/swagger 或 http://localhost:7218/swagger

# 前端界面
cd ../../ECOMMERCESHOPUXUI/EcommerceProject
npm install
npm run dev
# 访问地址：http://localhost:5173
```

---

## 🎯 使用说明

前后端服务启动后：

- 打开浏览器，访问 [http://localhost:5173](http://localhost:5173)
- 使用界面浏览商品、注册/登录、添加到购物车、使用VNPAY结账

**主要功能：**

- 商品浏览与搜索
- 购物车管理和结算
- 用户注册、登录与个人资料管理
- AI虚拟试穿商品对比
- 转盘抽奖优惠券
- 实时聊天机器人支持
- 查看订单历史与近期活动
- 后台商品、订单、用户和数据分析管理

---

## ⚙️ 配置方式

- 在环境变量（`.env`文件）中设置Cloudinary、Gemini AI、Firebase、VNPAY等服务的API密钥
- 在`appsettings.json`中更新数据库连接字符串
- 配置第三方API接口，用于AI、支付和邮件通知

---

## 🏗️ 系统架构

系统采用客户端-服务器解耦架构：

- **后端API：** 使用.NET 8 Web API开发，处理业务逻辑、数据存储、第三方服务集成及提供RESTful接口
- **前端应用：** 基于Vue.js 3，使用Vite作为构建工具，Pinia管理状态，Vue Router进行路由配置，Bootstrap 5提供UI样式，确保用户操作流畅快速。

---

## 📂 项目结构

```
/
├── APIClothesEcommerceShop/                # 后端API
│   ├── Controllers/                        # API控制器
│   ├── DTO/                                # 数据传输对象
│   ├── Data/                               # 数据库上下文、迁移
│   ├── Models/                             # 数据模型
│   ├── Repositories/                       #数据访问层
│   └── Services/                           # 业务逻辑
└── ECOMMERCESHOPUXUI/                        # 前端Vue.js应用
    └── EcommerceProject/
        ├── src/
        │   ├── assets/                     # 图片资源
        │   ├── components/                 # 可复用组件
        │   ├── views/                      # 页面视图
        │   ├── router/                     # 路由配置
        │   ├── stores/                     # 状态管理
        │   └── services/                   # API调用
        └── package.json
```

---

## 作者

- **主要贡献者**：Silent Stack团队

- **贡献者**：
<p align="center">
    <a href="https://github.com/dat0968/EcommerceShop/graphs/contributors">
      <img src="https://contrib.rocks/image?repo=dat0968/EcommerceShop" style="max-width: 400px;" />
    </a>
</p>


---

## 📜 许可证

本项目基于MIT许可证授权，详见 [LICENSE.md](LICENSE.md)。