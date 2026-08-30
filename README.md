[English](./README.md)

# EcommerceShop - Modern Online Clothing Store

A comprehensive e-commerce platform for selling clothes online, built with a modern, decoupled architecture. This project features a scalable backend API and a dynamic frontend, offering an engaging shopping experience.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

## Table of Contents

- [Features](#🚀-features)
- [Installation](#🛠️-installation)
- [Usage](#🎯-usage)
- [Configuration](#⚙️-configuration)
- [Architecture](#🏗️-architecture)
- [Project Structure](#📂-project-structure)
- [Authors](#👥-authors)
- [License](#📜-license)

---

## 🚀 Features

- 🛒 **Product Catalog:** Browse products by categories, view detailed descriptions, and images.
- 🔐 **Customer Authentication:** Secure login and registration using JWT and Google Authentication.
- 🛍️ **Shopping Cart & Order Management:** Add, modify, or remove products from cart, place orders, view order history, and track statuses.
- 💳 **Secure Payment Integration:** Payment processing via VNPAY.
- 📊 **Admin Dashboard:** Manage products, categories, customers, and orders.
- 💡 **Recommendation System:** Personalized product suggestions powered by ML.NET.
- 📧 **Email Service:** Automate sending order confirmations, notifications, and promo codes.
- 🌟 **Advanced Customer Features:**
  - Product comparison (with AI API for virtual try-on)
  - Spin-the-wheel coupons for discounts
  - Chatbot for support and inquiries
  - View recent browsing history

---

## 🛠️ Installation

**System Requirements**:
- Windows 10/11
- PowerShell 5.1 or higher
- .NET 8 SDK
- Node.js (LTS version recommended)
- Vue CLI (`npm install -g @vue/cli`)
- SQL Server or SQL Server Express/LocalDB

**Steps**:

```powershell
# 1. Clone the repository
git clone <repository_url> # Replace with your repository URL
cd EcommerceShop

# Backend - API Service
cd APIClothesEcommerceShop/APIClothesEcommerceShop
# Configure Database Connection:
# Edit appsettings.json to set your SQL Server connection string
# Example:
# "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# Save the file

# Restore dependencies and update database
dotnet restore
dotnet ef database update

# Run the backend API
dotnet run
# API will be accessible at https://localhost:7217/swagger or http://localhost:7218/swagger

# Frontend - User Interface
cd ../../ECOMMERCESHOPUXUI/EcommerceProject
npm install
npm run dev
# Access at http://localhost:5173
```

---

## 🎯 Usage

Once both backend and frontend services are running:

- Open your browser and go to [http://localhost:5173](http://localhost:5173)
- Use the intuitive UI to browse products, register/login, add items to the cart, and check out with VNPAY.

**Main features include:**

- Product browsing and searching
- Shopping cart management and checkout
- User registration, login, profile management
- Product comparison with AI-powered virtual try-on
- Lucky wheel coupons for discounts
- Real-time chatbot support
- Viewing past orders and recent activity
- Admin management of products, orders, users, and analytics

---

## ⚙️ Configuration & Environment Setup

To secure API credentials and database credentials, the `appsettings.json` file has been added to `.gitignore` and will not be pushed to the repository. 

After cloning the project, you must **manually create a new file named `appsettings.json`** in the backend root directory (`APIClothesEcommerceShop/APIClothesEcommerceShop/`) and paste the following template structure into it:

### 1. Required `appsettings.json` Template:

```json
{
  "ConnectionStrings": {
    "QuanLyCuaHangContext": "Data Source=YOUR_SQL_SERVER_NAME;Initial Catalog=QuanLyCuaHang;Integrated Security=True;Trust Server Certificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "JWT": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_HERE_MIN_32_CHARS"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "Vnpay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "BaseUrl": "https://vnpayment.vn",
    "ReturnUrl": "https://localhost:7139/api/VNPAY",
    "Command": "pay",
    "CurrCode": "VND",
    "Version": "2.1.0",
    "Locale": "vn",
    "TimeZoneId": "SE Asia Standard Time"
  },
  "GeminiSettings": {
    "Google": {
      "GoogleAPIUrl": "https://googleapis.com",
      "GoogleAPIKey": "YOUR_GEMINI_API_KEY"
    }
  },
  "AllowedHosts": "*"
}
```

### 2. Configuration Details to Update:

- **`ConnectionStrings`**: Replace `YOUR_SQL_SERVER_NAME` with your local SQL Server instance name (e.g., `.`, `localhost`, or `DESKTOP-XXXX\SQLEXPRESS`).
- **`JWT.SecretKey`**: Enter a random string of at least 32 characters to secure and sign your JWT tokens.
- **`Authentication.Google`**: Provide your own `ClientId` and `ClientSecret` from the Google Cloud Console if you need to test the Google Sign-In flow.
- **`Vnpay`**: Replace `YOUR_VNPAY_TMN_CODE` and `YOUR_VNPAY_HASH_SECRET` with the test keys provided by your VNPAY Sandbox account to debug payments.
- **`GeminiSettings`**: Paste your personal Gemini API Key generated from Google AI Studio to enable the chatbot, customer support assistant, and AI-driven features.

*Note: Never remove `appsettings.json` from `.gitignore` to prevent accidentally leaking private credentials to shared branches.*

## 🏗️ Architecture

The system follows a decoupled, client-server architecture:

- **Backend API:** Built with .NET 8 Web API, managing business logic, data storage, third-party integrations, and serving RESTful endpoints.
- **Frontend Application:** Developed with Vue.js 3, utilizing Vite as the build tool, Pinia for state management, Vue Router for navigation, and Bootstrap 5 for UI styling. It provides a fast, interactive user experience.

---

## 📂 Project Structure

```
/
├── APIClothesEcommerceShop/                # Backend API
│   ├── Controllers/                        # API endpoints
│   ├── DTO/                                # Data Transfer Objects
│   ├── Data/                               # DbContext, Migrations
│   ├── Models/                             # Database models
│   ├── Repositories/                       # Data access layer
│   └── Services/                           # Business logic
└── ECOMMERCESHOPUXUI/                        # Frontend Vue.js App
    └── EcommerceProject/
        ├── src/
        │   ├── assets/                     # Images, assets
        │   ├── components/                 # Reusable components
        │   ├── views/                      # Page views
        │   ├── router/                     # Routing configurations
        │   ├── stores/                     # State management
        │   └── services/                   # API service calls
        └── package.json
```

---


## Authors

- **Main Contributor**: Silent Stack Team

- **Contributors**:
<p align="center">
    <a href="https://github.com/dat0968/EcommerceShop/graphs/contributors">
      <img src="https://contrib.rocks/image?repo=dat0968/EcommerceShop" style="max-width: 400px;" />
    </a>
</p>


---

## 📜 License

This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
