# Flow 1: Create Booking Order - API Documentation

## Overview
Flow 1 implements the complete booking creation flow from vehicle selection to deposit payment.

## API Endpoints

### 1. Check Vehicle Availability
**Endpoint:** `POST /api/vehicle/check-availability`
**Service:** VehicleService (Port 5003)
**Auth:** Not required

**Request Body:**
```json
{
  "vehicleId": 101
}
```

**Response:**
```json
{
  "success": true,
  "isAvailable": true,
  "vehicle": {
    "vehicleId": 101,
    "modelId": 1,
    "color": "Red",
    "status": "Available",
    "isActive": true
  }
}
```

---

### 2. Check Order Availability (Date Overlap)
**Endpoint:** `POST /api/order/check-availability`
**Service:** BookingService (Port 5002)
**Auth:** Not required

**Request Body:**
```json
{
  "vehicleId": 101,
  "fromDate": "2024-10-05T08:00:00",
  "toDate": "2024-10-08T13:00:00"
}
```

**Response:**
```json
{
  "success": true,
  "isAvailable": true,
  "overlappingOrdersCount": 0
}
```

---

### 3. Create Order
**Endpoint:** `POST /api/order/create`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Request Body:**
```json
{
  "userId": 123,
  "vehicleId": 101,
  "modelPrice": 25000,
  "fromDate": "2024-10-05T08:00:00",
  "toDate": "2024-10-08T13:00:00",
  "totalTime": 77,
  "totalCost": 1925000
}
```

**Response:**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "orderId": 501,
    "userId": 123,
    "vehicleId": 101,
    "fromDate": "2024-10-05T08:00:00",
    "toDate": "2024-10-08T13:00:00",
    "totalDays": 3,
    "modelPrice": 25000,
    "totalCost": 1925000,
    "depositAmount": 577500,
    "status": "Pending",
    "createdAt": "2024-10-05T10:00:00"
  }
}
```

**Status:** Order status is set to "Pending"

---

### 4. Generate Contract
**Endpoint:** `POST /api/contract/generate/{orderId}`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Request Body (Optional):**
```json
{
  "templateVersion": 1
}
```

**Response:**
```json
{
  "success": true,
  "message": "Contract generated successfully",
  "data": {
    "contractId": 301,
    "orderId": 501,
    "contractNumber": "CT-2024-00301",
    "terms": "<html>...contract HTML...</html>",
    "status": "Draft",
    "createdAt": "2024-10-05T10:01:00",
    "expiresAt": "2024-10-12T10:01:00"
  }
}
```

**Status Updates:**
- Order status: "Pending" → "AwaitingContract"

---

### 5. Get Contract Terms
**Endpoint:** `GET /api/contract/terms/{orderId}`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Response:**
```json
{
  "success": true,
  "data": {
    "terms": "<html>...contract HTML content...</html>"
  }
}
```

---

### 6. Sign Contract
**Endpoint:** `POST /api/contract/{contractId}/sign`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Request Body:**
```json
{
  "signatureData": "data:image/png;base64,iVBORw0KGgo..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Contract signed successfully",
  "data": {
    "contractId": 301,
    "orderId": 501,
    "contractNumber": "CT-2024-00301",
    "status": "Signed",
    "signedAt": "2024-10-05T10:15:00",
    "signatureData": "data:image/png;base64,...",
    "signedFromIpAddress": "123.45.67.89"
  }
}
```

**Status Updates:**
- Contract status: "Draft" → "Signed"
- Order status: "AwaitingContract" → "ContractSigned"

---

### 7. Create Deposit Payment
**Endpoint:** `POST /api/payment/create-deposit`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Request Body:**
```json
{
  "orderId": 501
}
```

**Response:**
```json
{
  "success": true,
  "message": "Deposit payment created",
  "data": {
    "paymentId": 601,
    "orderId": 501,
    "depositAmount": 577500,
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=57750000&..."
  }
}
```

**Status Updates:**
- Payment record created with status "Pending"
- Order status: "ContractSigned" → "AwaitingDeposit"

**Action:** Redirect user to `paymentUrl` to complete VNPay payment

---

### 8. VNPay Deposit Callback
**Endpoint:** `GET /api/payment/vnpay-deposit-callback`
**Service:** BookingService (Port 5002)
**Auth:** Not required (called by VNPay)

**Query Parameters:** (Sent by VNPay)
```
?vnp_Amount=57750000
&vnp_BankCode=NCB
&vnp_ResponseCode=00
&vnp_TransactionNo=123456789
&vnp_SecureHash=...
&...
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Deposit payment successful. Order confirmed!",
  "data": {
    "paymentId": 601,
    "orderId": 501,
    "depositedAmount": 577500,
    "transactionCode": "VNP123456789",
    "orderStatus": "Confirmed"
  }
}
```

**Status Updates:**
- Payment: IsDeposited = true, Status = "DepositPaid"
- Order status: "AwaitingDeposit" → "Confirmed"

---

### 9. Get Payment Status
**Endpoint:** `GET /api/payment/{paymentId}/status`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Response:**
```json
{
  "success": true,
  "data": {
    "paymentId": 601,
    "status": "DepositPaid",
    "isDeposited": true,
    "isFullyPaid": false,
    "depositedAmount": 577500,
    "paidAmount": 577500
  }
}
```

---

### 10. Get Order Details
**Endpoint:** `GET /api/order/{orderId}`
**Service:** BookingService (Port 5002)
**Auth:** Required (JWT Bearer Token)

**Response:**
```json
{
  "success": true,
  "data": {
    "orderId": 501,
    "userId": 123,
    "vehicleId": 101,
    "fromDate": "2024-10-05T08:00:00",
    "toDate": "2024-10-08T13:00:00",
    "totalDays": 3,
    "modelPrice": 25000,
    "totalCost": 1925000,
    "depositAmount": 577500,
    "status": "Confirmed",
    "createdAt": "2024-10-05T10:00:00",
    "updatedAt": "2024-10-05T10:30:00"
  }
}
```

---

## Complete Flow 1 Sequence

```
1. Check Vehicle Availability
   POST /api/vehicle/check-availability
   → Returns: { isAvailable: true }

2. Check Date Availability
   POST /api/order/check-availability
   → Returns: { isAvailable: true }

3. Create Order
   POST /api/order/create
   → Order Status: "Pending"

4. Generate Contract
   POST /api/contract/generate/{orderId}
   → Order Status: "Pending" → "AwaitingContract"
   → Contract Status: "Draft"

5. Sign Contract
   POST /api/contract/{contractId}/sign
   → Contract Status: "Draft" → "Signed"
   → Order Status: "AwaitingContract" → "ContractSigned"

6. Create Deposit Payment
   POST /api/payment/create-deposit
   → Order Status: "ContractSigned" → "AwaitingDeposit"
   → Returns VNPay URL

7. User pays via VNPay → VNPay redirects back

8. VNPay Callback
   GET /api/payment/vnpay-deposit-callback
   → Payment Status: "DepositPaid"
   → Order Status: "AwaitingDeposit" → "Confirmed"

✓ Flow 1 Complete - Order Confirmed with Deposit Paid
```

---

## Order Status Flow

```
Pending
  ↓ (Generate Contract)
AwaitingContract
  ↓ (Sign Contract)
ContractSigned
  ↓ (Create Deposit Payment)
AwaitingDeposit
  ↓ (VNPay Callback Success)
Confirmed
  ↓ (Flow 3: Pickup)
InProgress
  ↓ (Flow 4: Return)
Completed
```

---

## Error Handling

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Error description here"
}
```

**Common HTTP Status Codes:**
- 200: Success
- 400: Bad Request (validation errors)
- 401: Unauthorized (missing/invalid token)
- 404: Not Found (resource doesn't exist)
- 500: Internal Server Error

---

## Authentication

Most endpoints require JWT Bearer token:

**Header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Get token from UserService:
```
POST http://localhost:5001/api/auth/login
```

---

## Service Ports

- **UserService:** http://localhost:5001
- **BookingService:** http://localhost:5002
- **VehicleService:** http://localhost:5003
- **StationService:** http://localhost:5004

---

## VNPay Configuration

Update `appsettings.json` in BookingService:

```json
{
  "VNPaySettings": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "http://localhost:5002/api/payment/vnpay-deposit-callback",
    "Version": "2.1.0",
    "Command": "pay",
    "CurrCode": "VND",
    "Locale": "vn"
  }
}
```

---

## Testing Flow 1

### Prerequisites
1. User must be logged in (have JWT token)
2. User documents must be approved
3. Vehicle must exist and be available

### Test Sequence

```bash
# 1. Login to get token
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'

# 2. Check vehicle availability
curl -X POST http://localhost:5003/api/vehicle/check-availability \
  -H "Content-Type: application/json" \
  -d '{"vehicleId":101}'

# 3. Create order
curl -X POST http://localhost:5002/api/order/create \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId":123,
    "vehicleId":101,
    "modelPrice":25000,
    "fromDate":"2024-10-05T08:00:00",
    "toDate":"2024-10-08T13:00:00",
    "totalTime":77,
    "totalCost":1925000
  }'

# 4. Generate contract
curl -X POST http://localhost:5002/api/contract/generate/501 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"

# 5. Sign contract
curl -X POST http://localhost:5002/api/contract/301/sign \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"signatureData":"data:image/png;base64,..."}'

# 6. Create deposit payment
curl -X POST http://localhost:5002/api/payment/create-deposit \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"orderId":501}'

# Response includes paymentUrl - redirect user to this URL
# VNPay will redirect back to vnpay-deposit-callback endpoint
```

---

## Database Tables Used

- **Orders** - Stores booking orders
- **OnlineContracts** - Stores rental contracts
- **Payments** - Stores payment records
- **Vehicles** (VehicleService DB) - Stores vehicle data

---

## Next Steps

After Flow 1 is complete (Order Confirmed):
- **Flow 3:** Vehicle Pickup (requires full payment or deposit based on trust score)
- **Flow 4:** Vehicle Return and inspection
