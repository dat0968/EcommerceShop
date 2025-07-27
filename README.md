[Tiếng Việt](./README.vi.md) | [English](./README.md) | [中文](./README.zh.md)

# E-commerce Shop Project
![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

> A comprehensive e-commerce platform with a .NET 8.0 backend API and a Vue.js frontend application.

## Table of Contents
- [Features](#🚀-features)
- [Installation](#🛠️-installation)
- [Usage](#🎯-usage)
- [Configuration](#⚙️-configuration)
- [Architecture](#🏗️-architecture)
- [Authors](#👥-authors)
- [License](#📜-license)

## 🚀 Features
- 🛒 **Product Catalog**: Browse and search for products with detailed descriptions and images.
- 🔐 **User Authentication**: Secure user registration and login using JWT and Google Authentication.
- 🛍️ **Shopping Cart & Order Management**: Add, update, remove items from the cart, place orders, view history, and track status.
- 💳 **Payment Integration**: Secure payment processing via VNPAY.
- 📊 **Admin Panel**: Manage products, categories, users, and orders (details to be added if confirmed).
- 💡 **Recommendation System**: Potentially provides product recommendations.
- 📧 **Email Service**: Integration for sending emails (e.g., order confirmations).

## 🛠️ Installation
This section provides instructions to get a copy of the project up and running on your local machine for development and testing purposes.

**System Requirements**:
- Windows 10/11
- PowerShell 5.1 or higher
- .NET 8.0 SDK
- Node.js (LTS version recommended)
- Vue CLI (Install globally: `npm install -g @vue/cli`)
- SQL Server (or SQL Server Express/LocalDB)
- SQL Server Management Studio (SSMS) (Optional, for database management)

**Steps**:
```powershell
# Step 1: Clone the repository
git clone <repository_url> # Replace <repository_url> with the actual URL
cd EcommerceShop

# Step 2: Backend Setup (APIClothesEcommerceShop)
# Navigate to the backend project directory
cd APIClothesEcommerceShop/APIClothesEcommerceShop

# Configure Database Connection:
# Open appsettings.json (and appsettings.Development.json) and update the ConnectionStrings section.
# Example: "EcommerceShopConnect_Dot": "Server=.;Database=EcommerceShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# Replace '.' with your SQL Server instance name if different.

# Apply Migrations and Seed Data
dotnet ef database update

# Run the Backend API
dotnet run

# Step 3: Frontend Setup (ECOMMERCESHOPUXUI)
# Navigate to the frontend project directory
cd ../../ECOMMERCESHOPUXUI/EcommerceProject

# Install Dependencies
npm install

# Run the Vue.js Application
npm run serve
```

## 🎯 Usage

After successfully setting up the backend and frontend, you can access the application through your web browser. The frontend application provides the user interface for browsing products, adding items to the cart, managing orders, and making payments.

### Accessing the Application
*   **Frontend**: Open your web browser and navigate to `http://localhost:8080/` (or the address shown in your frontend console after running `npm run serve`).
*   **Backend API Documentation**: For detailed API endpoints and testing, visit `https://localhost:7217/swagger/index.html` or `http://localhost:7218/swagger/index.html` in your browser.

### Key Features Usage
*   **Product Browsing**: Navigate through categories or use the search bar to find products.
*   **Shopping & Checkout**: Add items to your cart, proceed to checkout, and complete payment via VNPAY.
*   **User Account**: Register, log in, manage your profile, and view order history.
*   **Product Comparison (AI-powered)**: Utilize the `CompareProduct` feature to compare items, including virtual try-on (ensure AI API keys are configured).
*   **Lucky Wheel Coupon**: Participate in the `WheelCoupon` feature to win discount codes.
*   **Chat/Chatbot**: Interact with the system via chat or a chatbot for support or inquiries.
*   **View History**: Keep track of recently viewed products.
*   **Admin Features**: (Accessible to staff/admins) Manage products, categories, users, orders, staff, view statistics, and upload images.

Detailed usage instructions for specific features will be added here.

## ⚙️ Configuration
(Information about configuring the application, e.g., API keys, environment variables, will go here.)

## 🏗️ Architecture
(Overview of the project's architecture, e.g., diagrams, component breakdown, will go here.)

## 👥 Authors

*   [Your Name/Team Name] - Initial work

Feel free to add your name or team name here!

## 📜 License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.