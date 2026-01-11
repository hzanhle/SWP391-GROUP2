# 🛵 EV Station-Based Rental System

A comprehensive full-stack web application for managing electric vehicle (EV) rentals through station-based operations. This system enables users to book, rent, and return electric vehicles with integrated payment processing, contract management, and administrative oversight.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-19.1-blue.svg)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red.svg)

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Screenshots](#-screenshots)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### 👥 User Features
- **User Authentication & Authorization**
  - Email-based registration with OTP verification
  - JWT-based authentication
  - Password reset functionality
  - Profile management with document upload (Citizen ID, Driver License)

- **Vehicle Booking System**
  - Browse available vehicles by station
  - Real-time vehicle availability checking
  - Multi-step booking process (Station → Model → Schedule)
  - Booking preview with cost breakdown
  - Order management and tracking

- **Payment Integration**
  - Multiple payment gateways (VNPay, PayOS)
  - Secure deposit payment processing
  - Payment status tracking
  - Automatic order expiration handling

- **Rental Management**
  - Check-in/Check-out process with photo verification
  - Real-time order status updates (SignalR)
  - Rental history and booking details
  - Feedback and rating system

- **Interactive Maps**
  - Station location mapping (Leaflet/OpenStreetMap)
  - Station details and availability

### 👨‍💼 Staff Features
- Staff shift management
- Vehicle verification and inspection
- Order processing at stations
- Vehicle status updates

### 🔐 Admin Features
- **Dashboard & Analytics**
  - Real-time system statistics
  - Revenue analytics and reports
  - User growth statistics
  - Vehicle usage analytics
  - Peak hours analysis

- **Content Management**
  - Vehicle model management
  - Station management (CRUD operations)
  - Vehicle inventory management
  - Staff management and shift scheduling

- **Order & User Management**
  - Order monitoring and management
  - User account management
  - Risk customer tracking
  - Settlement and refund processing

- **Transfer Management**
  - Vehicle transfer between stations
  - Transfer history tracking
  - Ongoing transfer monitoring

### 🔧 System Features
- **Microservices Architecture**
  - API Gateway with reverse proxy (YARP)
  - Service isolation and scalability
  - Independent database per service

- **Background Jobs**
  - Automated order expiration checking (Hangfire)
  - Payment status synchronization
  - Email notifications

- **Contract Management**
  - Automated PDF contract generation (Puppeteer)
  - AWS S3 storage for contracts
  - Digital contract signing workflow

- **Trust Score System**
  - User trust score calculation
  - Risk assessment for customers
  - Trust score history tracking

## 🛠️ Tech Stack

### Frontend
- **Framework**: React 19.1 with Vite 7.1
- **UI Library**: Material-UI (MUI) 7.3
- **Styling**: Tailwind CSS 4.1
- **State Management**: React Hooks & Context API
- **Maps**: Leaflet & React-Leaflet
- **HTTP Client**: Native Fetch API
- **Real-time Communication**: SignalR Client
- **Date Handling**: Day.js
- **Charts**: MUI X Charts

### Backend
- **Framework**: ASP.NET Core 8.0
- **Architecture**: Microservices with API Gateway
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer Tokens
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **Background Jobs**: Hangfire
- **Documentation**: Swagger/OpenAPI

### Services
- **Payment Gateways**: VNPay, PayOS
- **Cloud Storage**: AWS S3 (for contracts and documents)
- **Email Service**: SMTP (Gmail)
- **PDF Generation**: PuppeteerSharp

### DevOps
- **Version Control**: Git
- **CI/CD**: GitHub Actions
- **Deployment**: GitHub Pages (Frontend)

## 🏗️ Architecture

This project follows a **Microservices Architecture** pattern:

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend (React)                        │
│                    Port: 5173 (Dev)                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        │ HTTP/REST
                        │
┌───────────────────────▼─────────────────────────────────────┐
│                    API Gateway (YARP)                        │
│                      Port: 5000                              │
│              Reverse Proxy + CORS + Routing                  │
└─────┬───────┬───────┬───────┬───────┬───────────────────────┘
      │       │       │       │       │
      │       │       │       │       │
┌─────▼──┐ ┌─▼────┐ ┌▼─────┐ ┌▼──────▼┐ ┌───────────────┐
│Booking │ │User  │ │Station│ │Vehicle │ │Admin Dashboard│
│Service │ │Service│ │Service│ │Service │ │   Service     │
│ 5049   │ │ 5109 │ │ 5185  │ │  5002  │ │    5167       │
└────┬───┘ └───┬──┘ └───┬───┘ └───┬────┘ └───────┬───────┘
     │         │        │         │              │
     │         │        │         │              │
┌────▼─────────▼────────▼─────────▼──────────────▼──────┐
│              SQL Server Database                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│  │ Booking  │ │   User   │ │ Station  │ │ Vehicle  │ │
│  │   DB     │ │    DB    │ │    DB    │ │    DB    │ │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ │
│  ┌──────────────────────────────────────────────┐     │
│  │         Admin Dashboard DB                   │     │
│  └──────────────────────────────────────────────┘     │
└────────────────────────────────────────────────────────┘
```

### Service Breakdown

1. **ApiGateway** (Port 5000)
   - Single entry point for all client requests
   - Route forwarding to appropriate services
   - CORS configuration
   - Load balancing (ready for scaling)

2. **BookingService** (Port 5049)
   - Order management
   - Payment processing (VNPay, PayOS)
   - Contract generation (PDF)
   - Settlement and refunds
   - Feedback management
   - Background job scheduling (Hangfire)

3. **UserService** (Port 5109)
   - User authentication and authorization
   - Profile management
   - Document verification (Citizen ID, Driver License)
   - OTP email service
   - Notification management

4. **StationService** (Port 5185)
   - Station CRUD operations
   - Staff shift management
   - Feedback collection
   - Station analytics

5. **TwoWheelVehicleService** (Port 5002)
   - Vehicle model management
   - Vehicle inventory management
   - Vehicle transfer between stations
   - Vehicle availability tracking

6. **AdminDashboardService** (Port 5167)
   - Aggregated analytics from all services
   - Dashboard statistics
   - Reporting and visualization

## 📸 Screenshots

<!-- Add your screenshots here -->
<!-- Example format:
![Home Page](docs/screenshots/home.png)
![Booking Flow](docs/screenshots/booking.png)
![Admin Dashboard](docs/screenshots/admin-dashboard.png)
![Payment](docs/screenshots/payment.png)
-->

> **Note**: Screenshots will be added here. Please add your project screenshots to showcase the application.

## 🚀 Getting Started

### Prerequisites

- **.NET SDK 8.0** or later
- **Node.js 18+** and npm
- **SQL Server** (LocalDB, Express, or Full)
- **Git**

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/SWP391-GROUP2.git
cd SWP391-GROUP2
```

#### 2. Backend Setup

1. **Configure SQL Server**
   - Ensure SQL Server is running on `localhost:1433`
   - Default credentials: `sa` / `12345`
   - Databases will be created automatically on first run

2. **Restore NuGet Packages**
   ```bash
   cd Backend/EV_Rental_System
   dotnet restore
   ```

3. **Update Connection Strings** (if needed)
   - Edit `appsettings.json` in each service
   - Update connection strings if your SQL Server setup differs

4. **Run Database Migrations**
   ```bash
   # Migrations will run automatically on first start
   # Or manually run:
   cd BookingService
   dotnet ef database update
   # Repeat for other services
   ```

5. **Start Backend Services**
   
   **Option A: Using Visual Studio**
   - Open `Backend/EV_Rental_System/EV_Rental_System.sln`
   - Set multiple startup projects (all services)
   - Press F5 to run

   **Option B: Using Command Line**
   ```bash
   # Terminal 1 - ApiGateway
   cd Backend/EV_Rental_System/ApiGateway
   dotnet run
   
   # Terminal 2 - BookingService
   cd Backend/EV_Rental_System/BookingService
   dotnet run
   
   # Terminal 3 - UserService
   cd Backend/EV_Rental_System/UserService
   dotnet run
   
   # Terminal 4 - StationService
   cd Backend/EV_Rental_System/StationService
   dotnet run
   
   # Terminal 5 - TwoWheelVehicleService
   cd Backend/EV_Rental_System/TwoWheelVehicleService
   dotnet run
   
   # Terminal 6 - AdminDashboardService
   cd Backend/EV_Rental_System/AdminDashboardService
   dotnet run
   ```

#### 3. Frontend Setup

1. **Install Dependencies**
   ```bash
   cd "Frontend/EV Station-based Rental System"
   npm install
   ```

2. **Configure Environment Variables**
   
   Create a `.env` file:
   ```env
   VITE_API_URL=http://localhost:5000
   ```

3. **Start Development Server**
   ```bash
   npm run dev
   ```

4. **Access the Application**
   - Frontend: http://localhost:5173
   - API Gateway: http://localhost:5000

### Configuration

#### Payment Gateways

Update payment gateway credentials in `BookingService/appsettings.json`:
- **VNPay**: Update `VNPaySettings` section
- **PayOS**: Update `PayOSSettings` section

#### AWS S3 (Optional)

If using AWS S3 for contract storage, update `AwsS3Settings` in `BookingService/appsettings.json`.

#### Email Service

Configure SMTP settings in `UserService/appsettings.json` and `BookingService/appsettings.json` for OTP and notifications.

## 📁 Project Structure

```
SWP391-GROUP2/
│
├── Backend/
│   └── EV_Rental_System/
│       ├── ApiGateway/              # API Gateway service
│       ├── BookingService/          # Booking & payment service
│       ├── UserService/             # User authentication service
│       ├── StationService/          # Station management service
│       ├── TwoWheelVehicleService/  # Vehicle management service
│       ├── AdminDashboardService/   # Admin analytics service
│       └── EV_Rental_System.sln     # Solution file
│
├── Frontend/
│   └── EV Station-based Rental System/
│       ├── src/
│       │   ├── api/                 # API client functions
│       │   ├── components/          # Reusable components
│       │   ├── pages/               # Page components
│       │   ├── styles/              # CSS files
│       │   └── utils/               # Utility functions
│       ├── public/                  # Static assets
│       └── package.json
│
├── .github/
│   └── workflows/
│       └── deploy-frontend.yml      # GitHub Pages deployment
│
└── README.md
```

## 📚 API Documentation

API documentation is available via Swagger when running services in Development mode:

- **BookingService**: http://localhost:5049
- **UserService**: http://localhost:5109
- **StationService**: http://localhost:5185
- **TwoWheelVehicleService**: http://localhost:5002
- **AdminDashboardService**: http://localhost:5167

All APIs are routed through the **API Gateway** at `http://localhost:5000`:
- `/booking/*` → BookingService
- `/user/*` → UserService
- `/station/*` → StationService
- `/vehicle/*` → TwoWheelVehicleService
- `/admin-dashboard/*` → AdminDashboardService

## 🚢 Deployment

### Frontend Deployment (GitHub Pages)

The frontend is automatically deployed to GitHub Pages via GitHub Actions when changes are pushed to the `main` branch.

**Manual Deployment:**
```bash
cd "Frontend/EV Station-based Rental System"
npm run build
# Deploy the 'dist' folder to GitHub Pages
```

**GitHub Pages URL**: `https://yourusername.github.io/SWP391-GROUP2/`

### Backend Deployment

For production deployment, consider:
- **Cloud Platforms**: Azure App Service, AWS Elastic Beanstalk, Google Cloud Run
- **Containerization**: Docker + Kubernetes
- **Database**: Managed SQL Server (Azure SQL, AWS RDS)
- **API Gateway**: Keep YARP or use cloud-native solutions (Azure API Management, AWS API Gateway)

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Team

SWP391 Group 2

## 📧 Contact

For questions or support, please open an issue in the GitHub repository.

---

**Built with ❤️ using React, .NET, and SQL Server**
