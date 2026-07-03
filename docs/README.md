# Tài liệu Mô tả Nghiệp vụ Challenge 11: Report, Export & Background Job

Tài liệu này hướng dẫn chi tiết cách thức hoạt động, cấu trúc cơ sở dữ liệu, cơ chế vận hành của tác vụ nền (Background Job) và các API Báo cáo (Report) trong dự án Mini ERP.

---

## 1. Tác Vụ Nền Tổng Hợp Hằng Ngày (Daily Summary Job)

### 1.1 Mục tiêu nghiệp vụ
Để tránh việc truy vấn tính toán trực tiếp trên lượng dữ liệu giao dịch khổng lồ (Sales Orders, Purchase Orders) khi người quản lý cần xem báo cáo tổng quát, hệ thống được thiết lập một job chạy nền tự động tổng hợp kết quả của ngày hôm trước và lưu trữ vào bảng thống kê riêng biệt (`daily_summaries`).

### 1.2 Cơ sở dữ liệu (`daily_summaries`)
Bảng `daily_summaries` được thiết kế chuẩn hóa và đồng bộ với hệ thống:
*   `id` (uuid, Primary Key, tự động sinh bằng `gen_random_uuid()`).
*   `summary_date` (timestamp with time zone, chứa ngày tổng hợp - lưu ở dạng 00:00:00 UTC). **Thiết lập Unique Index** để đảm bảo không trùng ngày.
*   `total_sales_revenue` (numeric, tổng doanh thu của các đơn bán hàng có trạng thái `CONFIRMED` trong ngày).
*   `total_sales_orders_count` (integer, tổng số đơn bán hàng có trạng thái `CONFIRMED` trong ngày).
*   `total_purchase_cost` (integer/numeric, tổng chi phí của các đơn mua hàng có trạng thái `CONFIRMED` trong ngày).
*   `total_purchase_orders_count` (integer, tổng số đơn mua hàng có trạng thái `CONFIRMED` trong ngày).
*   `low_stock_products_count` (integer, tổng số mặt hàng có tồn kho ít hơn hoặc bằng mức cảnh báo `10` tại thời điểm tổng hợp).
*   Các trường Audit tự động: `created_at`, `updated_at`, `deleted_at`, v.v.

### 1.3 Cơ chế hoạt động của Job
*   **Công nghệ sử dụng**: **Hangfire** kết hợp với **PostgreSQL Storage** (`Hangfire.PostgreSql`), lưu trạng thái job trực tiếp vào DB hiện tại mà không cần thiết lập Redis.
*   **Lịch trình (Cron)**: Chạy vào **00:05 mỗi ngày (UTC)** (`5 0 * * *`) để đảm bảo toàn bộ các giao dịch của ngày hôm trước đã được chốt và không có độ trễ múi giờ làm sót dữ liệu.
*   **Idempotency (Tính luỹ đẳng)**: Job sử dụng cơ chế **Upsert** (nếu ngày đó chưa có bản ghi summary thì thêm mới; nếu đã có thì cập nhật lại các chỉ số). Điều này đảm bảo khi hệ thống chạy lại job hoặc admin trigger thủ công nhiều lần, dữ liệu không bao giờ bị nhân đôi hay sai lệch.
*   **Fail-safe & Tự động Retry**:
    *   Toàn bộ logic xử lý được bao bọc trong khối `try-catch`.
    *   Nếu có lỗi kết nối DB hoặc lỗi hệ thống khác, job sẽ ghi nhận lỗi chi tiết qua `ILogger.LogError()` và rethrow Exception để Hangfire kích hoạt cơ chế retry tự động (mặc định thử lại 10 lần với thời gian giãn cách tăng dần).

### 1.4 API kích hoạt thủ công (Manual Trigger)
*   **Endpoint**: `POST /api/jobs/daily-summary/run?targetDate=`
*   **Chức năng**: Cho phép quản trị viên trigger chạy job tổng hợp ngay lập tức cho một ngày bất kỳ (ví dụ: cần tính toán lại dữ liệu ngày hôm trước do có sự thay đổi đột xuất). Nếu không truyền `targetDate`, job mặc định chạy cho ngày hôm qua.

---

## 2. Danh Sách API Báo Cáo & Xuất Dữ Liệu (Reports)

### 2.1 Báo cáo Doanh thu (Revenue Report)
*   **Endpoint**: `GET /api/reports/revenue?fromDate=&toDate=`
*   **Chức năng**: Lấy doanh thu trong khoảng thời gian chỉ định (nhận định dạng UTC).
*   **Xử lý tối ưu**: Tính toán doanh thu bằng `SumAsync` và `CountAsync` trực tiếp tại PostgreSQL, đồng thời nhóm theo ngày để trả về mảng `dailyBreakdown` phục vụ cho việc vẽ biểu đồ trực quan ở Front-End.

### 2.2 Báo cáo hàng sắp hết (Low Stock Report)
*   **Endpoint**: `GET /api/reports/inventory-low-stock?threshold=10&page=1&limit=10`
*   **Chức năng**: Trả về danh sách sản phẩm có tồn kho thấp hơn hoặc bằng `threshold`.
*   **Xử lý tối ưu**: Hỗ trợ phân trang (`page`, `limit`), sắp xếp theo lượng tồn kho tăng/giảm dần, và tìm kiếm theo tên hoặc SKU.

### 2.3 Xuất file CSV (Export Sales Report)
*   **Endpoint**: `GET /api/reports/sales/export?fromDate=&toDate=`
*   **Chức năng**: Tải về file CSV báo cáo chi tiết từng dòng mặt hàng bán ra trong khoảng thời gian chỉ định.
*   **Tối ưu hóa chống OOM**:
    *   Sử dụng thư viện **CsvHelper** cấu hình viết trực tiếp vào luồng phản hồi mạng (`Response.Body`) mà không sinh mảng byte tạm trên RAM.
    *   Sử dụng **`IAsyncEnumerable<SalesOrderItem>`** để tải dữ liệu dạng stream từ DB, flush bộ ghi tuần tự theo từng lô nhỏ, giải phóng RAM liên tục, giúp server xử lý an toàn kể cả khi xuất hàng triệu bản ghi.

---

## 3. Hướng Dẫn Chạy & Kiểm Thử

### 3.1 Khởi động dự án
```bash
# 1. Di chuyển vào thư mục API và chạy
cd MiniERP
dotnet run
```

### 3.2 Kiểm tra giao diện theo dõi Job
*   Truy cập **Hangfire Dashboard**: `https://localhost:{port}/hangfire` để theo dõi tiến trình chạy job, lịch trình recurring jobs, trạng thái thành công/thất bại và số lần retry của các job nền.
*   Truy cập **Swagger UI**: `https://localhost:{port}/swagger` để gọi thử các API báo cáo và job thủ công.
