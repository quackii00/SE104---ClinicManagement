# Private Clinic Management System

## 1. Giới thiệu

Private Clinic Management System là phần mềm quản lý phòng mạch tư được xây dựng theo mô hình Client – Server nhằm hỗ trợ quản lý bệnh nhân, phiếu khám bệnh, kê toa thuốc, hóa đơn và báo cáo doanh thu.

Hệ thống sử dụng WPF để xây dựng ứng dụng Desktop và ASP.NET Core Web API để xử lý backend nghiệp vụ.

---

# 2. Công nghệ sử dụng

| Thành phần      | Công nghệ             |
| --------------- | --------------------- |
| Frontend        | WPF (.NET 10)          |
| Backend         | ASP.NET Core Web API  |
| Database        | Supabase PostgreSQL   |
| Ngôn ngữ        | C#                    |
| UI              | XAML                  |
| ORM             | Entity Framework Core |
| Version Control | Git & GitHub          |

---

# 3. Kiến trúc hệ thống

```text
WPF Frontend
      ↓ HTTP REST API
ASP.NET Core Backend
      ↓
Supabase PostgreSQL
```

Frontend:

* xử lý giao diện
* nhận thao tác người dùng
* gọi API đến backend.

Backend:

* xử lý business logic
* validation dữ liệu
* xác thực người dùng
* truy vấn cơ sở dữ liệu.

---

# 4. Cấu trúc project

```text
PrivateClinicManagement/
│
├── Frontend/
│   ├── Views/
│   ├── ViewModels/
│   ├── Models/
│   ├── Services/
│   ├── Helpers/
│   ├── Resources/
│   ├── Assets/
│   └── App.xaml
│
├── Backend/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── DTOs/
│   ├── Middleware/
│   ├── Data/
│   ├── Configurations/
│   ├── Models/
│   └── Program.cs
│
├── Docs/
│   ├── ProjectDocuments/
│   └── Report/
│
├── README.md
├── .gitignore
└── LICENSE
```

---

# 5. Chức năng chính

* Đăng nhập hệ thống
* Quản lý bệnh nhân
* Tra cứu bệnh nhân
* Lập phiếu khám bệnh
* Kê toa thuốc
* Lập hóa đơn
* Tra cứu lịch sử khám bệnh
* Báo cáo doanh thu
* Báo cáo sử dụng thuốc
* Quản lý quy định

---

# 6. Quy tắc làm việc nhóm

## 6.1. Quy tắc branch

| Branch    | Mục đích             |
| --------- | -------------------- |
| main      | Source ổn định       |
| feature/* | Phát triển chức năng |

Ví dụ:

```text
feature/login
feature/invoice
feature/report
feature/patient-management
```

---

## 6.2. Quy trình làm việc

1. Pull source mới nhất từ `main`
2. Tạo branch riêng cho chức năng
3. Thực hiện code
4. Commit thường xuyên
5. Push branch lên GitHub
6. Merge vào `main`

---

## 6.3. Quy tắc commit

Cấu trúc commit:

```text
<type>: <message>
```

Ví dụ:

```text
feat: thêm chức năng đăng nhập
fix: sửa lỗi lập hóa đơn
docs: cập nhật README
style: chỉnh giao diện dashboard
refactor: tối ưu AuthService
```

| Type     | Ý nghĩa           |
| -------- | ----------------- |
| feat     | Thêm chức năng    |
| fix      | Sửa lỗi           |
| docs     | Cập nhật tài liệu |
| style    | Chỉnh giao diện   |
| refactor | Tối ưu code       |

---

# 7. Quy tắc code

## 7.1. Quy tắc đặt tên

| Thành phần | Quy tắc    |
| ---------- | ---------- |
| Class      | PascalCase |
| Method     | PascalCase |
| Property   | PascalCase |
| Variable   | camelCase  |
| Constant   | UPPER_CASE |

Ví dụ:

```csharp
public class PatientService
{
    private int patientCount;

    public void CreateInvoice()
    {

    }
}
```

---

## 7.2. Quy tắc code frontend

* Áp dụng mô hình MVVM
* Không viết business logic trong View
* ViewModel chỉ xử lý dữ liệu giao diện
* Service dùng để gọi API backend
* Không truy cập database trực tiếp từ frontend

---

## 7.3. Quy tắc code backend

* Tách Controller, Service và Repository rõ ràng
* Controller chỉ nhận request và trả response
* Service xử lý business logic
* Repository thao tác dữ liệu
* Validate dữ liệu trước khi lưu database

---

## 7.4. Quy tắc bảo mật

KHÔNG commit:

* Supabase API key
* JWT secret
* mật khẩu database
* file `.env`
* `appsettings.Development.json`

---

# 8. Hướng dẫn chạy project

## 8.1. Frontend

Mở solution:

```text
Frontend/PrivateClinicManagement.UI.sln
```

Run WPF project.

---

## 8.2. Backend

Mở solution:

```text
Backend/PrivateClinicManagement.API.sln
```

Run ASP.NET Core Web API.

---

# 9. Cấu hình môi trường

Tạo file:

```text
appsettings.Development.json
```

Ví dụ:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  }
}
```

---

# 10. Triển khai hệ thống

| Thành phần | Nền tảng                |
| ---------- | ----------------------- |
| Frontend   | WPF Desktop Application |
| Backend    | Render / Railway        |
| Database   | Supabase Cloud          |

Frontend được build thành file `.exe`.

Backend ASP.NET Core Web API được deploy lên cloud để frontend có thể gọi API thông qua Internet.

---

# 11. License

Project phục vụ mục đích học tập và nghiên cứu.
