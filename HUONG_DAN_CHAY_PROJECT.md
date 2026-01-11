# 📚 HƯỚNG DẪN CHẠY PROJECT SWP391-GROUP2

## 🏗️ Tổng quan về Project

Đây là hệ thống cho thuê xe điện với kiến trúc **Microservices**:
- **Backend**: .NET/C# với 6 services (ApiGateway + 5 microservices)
- **Frontend**: React + Vite
- **Database**: SQL Server
- **Ngrok**: Để expose API ra ngoài (dùng cho payment callbacks)

---

## 📋 Yêu cầu hệ thống

### Cần cài đặt:
1. **.NET SDK 8.0** (hoặc mới hơn)
   - Download: https://dotnet.microsoft.com/download
   - Kiểm tra: `dotnet --version`

2. **SQL Server** (LocalDB hoặc SQL Server Express/Full)
   - Đảm bảo SQL Server đang chạy
   - Port mặc định: 1433
   - Username: `sa`
   - Password: `12345`

3. **Node.js** (version 18+)
   - Download: https://nodejs.org/
   - Kiểm tra: `node --version` và `npm --version`

4. **Ngrok** (để expose API)
   - Download: https://ngrok.com/download
   - Đăng ký tài khoản miễn phí để lấy authtoken

---

## 🗄️ BƯỚC 1: Setup Database

### 1.1. Khởi động SQL Server
Đảm bảo SQL Server đang chạy với:
- **Server**: `localhost,1433`
- **Username**: `sa`
- **Password**: `12345`

### 1.2. Tạo Databases
Project sẽ tự động tạo databases khi chạy lần đầu (nếu đã có Migrations), hoặc bạn có thể tạo thủ công:

```sql
CREATE DATABASE BookingService;
CREATE DATABASE UserService;
CREATE DATABASE StationService;
CREATE DATABASE TwoWheelVehicleService;
CREATE DATABASE AdminDashboard;
```

---

## 🔧 BƯỚC 2: Setup và Chạy Backend

### 2.1. Cấu trúc Backend Services

Backend gồm 6 services chạy trên các port:

| Service | Port | Mô tả |
|---------|------|-------|
| **ApiGateway** | 5000 | API Gateway (YARP Reverse Proxy) |
| BookingService | 5049 | Quản lý đặt xe, thanh toán |
| UserService | 5109 | Quản lý user, authentication |
| StationService | 5185 | Quản lý trạm |
| TwoWheelVehicleService | 5002 | Quản lý xe |
| AdminDashboardService | 5167 | Dashboard admin |

### 2.2. Chạy Backend Services

Có 2 cách:

#### **Cách 1: Chạy từ Visual Studio (Dễ nhất)**
1. Mở file `Backend/EV_Rental_System/EV_Rental_System.sln`
2. Nhấn F5 hoặc Start để chạy toàn bộ solution
3. Visual Studio sẽ tự động start tất cả services

#### **Cách 2: Chạy từ Terminal (Thủ công)**

Mở **6 terminal windows** và chạy từng service:

**Terminal 1 - ApiGateway:**
```bash
cd "Backend/EV_Rental_System/ApiGateway"
dotnet run
```

**Terminal 2 - BookingService:**
```bash
cd "Backend/EV_Rental_System/BookingService"
dotnet run
```

**Terminal 3 - UserService:**
```bash
cd "Backend/EV_Rental_System/UserService"
dotnet run
```

**Terminal 4 - StationService:**
```bash
cd "Backend/EV_Rental_System/StationService"
dotnet run
```

**Terminal 5 - TwoWheelVehicleService:**
```bash
cd "Backend/EV_Rental_System/TwoWheelVehicleService"
dotnet run
```

**Terminal 6 - AdminDashboardService:**
```bash
cd "Backend/EV_Rental_System/AdminDashboardService"
dotnet run
```

### 2.3. Kiểm tra Backend đã chạy

Sau khi start, kiểm tra:
- ApiGateway: http://localhost:5000 (sẽ trả về `{"ok":true,"at":"gateway"}`)
- BookingService Swagger: http://localhost:5049 (nếu có)
- UserService: http://localhost:5109
- StationService: http://localhost:5185
- TwoWheelVehicleService: http://localhost:5002
- AdminDashboardService: http://localhost:5167

---

## 🌐 BƯỚC 3: Setup và Chạy Frontend

### 3.1. Cài đặt Dependencies

```bash
cd "Frontend/EV Station-based Rental System"
npm install
```

### 3.2. Tạo file .env

Tạo file `.env` trong thư mục `Frontend/EV Station-based Rental System/`:

```env
# API Gateway URL (khi chạy local)
VITE_API_URL=http://localhost:5000

# Hoặc nếu dùng ngrok (sau khi setup ngrok)
# VITE_API_URL=https://your-ngrok-url.ngrok-free.app

# Các API URLs khác (optional - sẽ fallback về VITE_API_URL nếu không set)
VITE_BOOKING_API_URL=http://localhost:5000
VITE_STATION_API_URL=http://localhost:5000
VITE_VEHICLE_API_URL=http://localhost:5000
VITE_AdminDashboard_API_URL=http://localhost:5000

# Ngrok URL cho frontend (nếu cần)
VITE_ALLOWED_HOSTS=your-ngrok-url.ngrok-free.app
```

### 3.3. Chạy Frontend

```bash
npm run dev
```

Frontend sẽ chạy tại: **http://localhost:5173**

---

## 🚀 BƯỚC 4: Setup Ngrok (Để expose API ra ngoài)

Ngrok dùng để:
- Expose ApiGateway ra internet (cần cho payment callbacks như PayOS, VNPay)
- Cho phép test từ mobile/device khác

### 4.1. Cài đặt Ngrok

1. Download ngrok: https://ngrok.com/download
2. Giải nén và đặt vào thư mục dễ truy cập (hoặc thêm vào PATH)
3. Đăng ký tài khoản miễn phí tại: https://dashboard.ngrok.com/
4. Lấy authtoken từ dashboard
5. Chạy lệnh:
```bash
ngrok config add-authtoken YOUR_AUTHTOKEN
```

### 4.2. Start Ngrok

Chạy ngrok để expose ApiGateway (port 5000):

```bash
ngrok http 5000
```

Sau khi chạy, ngrok sẽ hiển thị URL dạng:
```
Forwarding: https://xxxx-xxx-xxx-xxx-xxx.ngrok-free.app -> http://localhost:5000
```

**Lưu lại URL này!** (Ví dụ: `https://f7afc723e0e3.ngrok-free.app`)

### 4.3. Cập nhật Cấu hình với Ngrok URL

#### a) Cập nhật ApiGateway CORS

File: `Backend/EV_Rental_System/ApiGateway/appsettings.json`

Thêm ngrok URL vào mảng `AllowedOrigins`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://localhost:5173",
    "https://YOUR_NGROK_URL.ngrok-free.app"
  ]
}
```

#### b) Cập nhật BookingService PayOS/VNPay Callbacks

File: `Backend/EV_Rental_System/BookingService/appsettings.json`

Cập nhật các URL trong `PayOSSettings` và `VNPaySettings`:

```json
"PayOSSettings": {
  "ReturnUrl": "https://YOUR_NGROK_URL.ngrok-free.app/booking/api/payment/payos-deposit-callback",
  "CancelUrl": "https://YOUR_NGROK_URL.ngrok-free.app/booking/api/payment/payos-deposit-callback",
  "WebhookUrl": "https://YOUR_NGROK_URL.ngrok-free.app/booking/api/payment/payos/webhook"
},
"FrontendSettings": {
  "BaseUrl": "https://YOUR_NGROK_URL.ngrok-free.app"
}
```

⚠️ **Lưu ý**: Mỗi lần restart ngrok, URL sẽ thay đổi (nếu dùng free plan). Bạn cần update lại các config này.

#### c) Cập nhật Frontend .env (nếu muốn dùng ngrok)

File: `Frontend/EV Station-based Rental System/.env`

```env
VITE_API_URL=https://YOUR_NGROK_URL.ngrok-free.app
```

Sau đó restart frontend: `npm run dev`

---

## ✅ CHECKLIST Chạy Project

### Trước khi chạy:
- [ ] SQL Server đang chạy (localhost:1433, sa/12345)
- [ ] .NET SDK đã cài đặt
- [ ] Node.js đã cài đặt
- [ ] Đã cài dependencies cho Frontend (`npm install`)

### Khi chạy:
- [ ] **Backend**: Start tất cả 6 services (ApiGateway + 5 microservices)
- [ ] Kiểm tra ApiGateway chạy tại http://localhost:5000
- [ ] **Frontend**: Chạy `npm run dev` tại port 5173
- [ ] **Ngrok** (nếu cần): `ngrok http 5000` và update configs

### Kiểm tra:
- [ ] ApiGateway: http://localhost:5000 → `{"ok":true,"at":"gateway"}`
- [ ] Frontend: http://localhost:5173 → Giao diện web
- [ ] Ngrok: https://your-url.ngrok-free.app → Forward đến localhost:5000

---

## 🔍 Troubleshooting

### Lỗi Database Connection
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `appsettings.json`: Server=localhost,1433; User Id=sa; Password=12345
- Kiểm tra SQL Server Authentication Mode: Mixed Mode (SQL + Windows)

### Lỗi Port đã được sử dụng
- Kiểm tra process nào đang dùng port: `netstat -ano | findstr :5000`
- Kill process hoặc đổi port trong `launchSettings.json`

### Lỗi CORS
- Kiểm tra `ApiGateway/appsettings.json` → `Cors:AllowedOrigins`
- Đảm bảo frontend URL đã được thêm vào (http://localhost:5173)

### Lỗi Ngrok
- Kiểm tra ngrok đã authenticated: `ngrok config check`
- Đảm bảo ApiGateway đang chạy tại port 5000
- Kiểm tra firewall không block ngrok

### Lỗi Frontend không kết nối API
- Kiểm tra `.env` file có đúng `VITE_API_URL`
- Restart frontend sau khi sửa `.env`
- Kiểm tra browser console xem lỗi gì

---

## 📝 Ghi chú quan trọng

1. **Thứ tự chạy**: Nên chạy Backend trước, sau đó mới chạy Frontend
2. **Ngrok URL thay đổi**: Mỗi lần restart ngrok (free plan) sẽ có URL mới → phải update configs
3. **Database Migrations**: Project có thể tự tạo DB, hoặc bạn cần chạy migrations nếu có
4. **Payment Callbacks**: PayOS/VNPay cần public URL (ngrok) để gọi callback về
5. **Environment Variables**: Frontend dùng `.env` file, Backend dùng `appsettings.json`

---

## 🎯 Tóm tắt nhanh

```bash
# 1. Start SQL Server

# 2. Start Backend (6 terminals hoặc Visual Studio)
cd Backend/EV_Rental_System/ApiGateway && dotnet run
cd Backend/EV_Rental_System/BookingService && dotnet run
# ... (4 services khác)

# 3. Start Ngrok (optional)
ngrok http 5000
# Update configs với ngrok URL

# 4. Start Frontend
cd "Frontend/EV Station-based Rental System"
npm install  # Lần đầu
npm run dev

# 5. Mở browser: http://localhost:5173
```

---

**Chúc bạn code vui vẻ! 🚀**

