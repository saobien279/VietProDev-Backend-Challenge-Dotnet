# Tài liệu Phân tích Kiến trúc Database (Mini ERP System)

## 1. Giới thiệu tổng quan
Hệ thống Mini ERP tập trung vào quản lý Bán hàng, Nhập hàng, Tồn kho và Phân quyền nội bộ. Toàn bộ hệ thống cơ sở dữ liệu được thiết kế và chuẩn hóa tối ưu cho hệ quản trị cơ sở dữ liệu **PostgreSQL**, đảm bảo tính toàn vẹn dữ liệu thông qua các ràng buộc khóa chính, khóa ngoại, chỉ mục (index) và các ràng buộc duy nhất (unique constraints).

## 2. Giải pháp cho các bài toán nghiệp vụ đặc thù

### 2.1. Quản lý Danh mục hàng hóa Cha - Con (Hierarchical Categories)
* **Bài toán:** Danh mục sản phẩm cần phân cấp không giới hạn (ví dụ: *Nước ➔ Nước ngọt ➔ Coca Cola*).
* **Giải pháp:** Áp dụng mô hình **Self-Referencing** (Tự tham chiếu). Trong bảng `categories`, thêm cột `parent_id` làm khóa ngoại trỏ ngược lại chính cột `id` của bảng `categories`. Nếu `parent_id IS NULL`, danh mục đó là danh mục gốc.

### 2.2. Đóng băng giá trị tại thời điểm giao dịch (Data Snapshot)
* **Bài toán:** Giá của sản phẩm (`selling_price`, `cost_price`) trong bảng `products` thay đổi theo thời gian. Nếu người dùng xem lại hóa đơn cũ, doanh thu/chi phí sẽ bị sai lệch nếu lấy giá hiện tại của sản phẩm.
* **Giải pháp:** Hai bảng chi tiết đơn hàng `sales_order_items` và `purchase_order_items` bắt buộc phải có cột `unit_price` để lưu lại mức giá thực tế tại thời điểm thực hiện giao dịch.

### 2.3. Quản lý biến động kho (Inventory & Stock Transactions)
* **Bảng `inventory`:** Lưu trữ trạng thái số lượng tồn kho hiện tại của sản phẩm (Quan hệ 1-1 với `products`). 
  * *Quy định bắt buộc:* Số lượng không được phép âm (`quantity >= 0`).
* **Bảng `stock_transactions`:** Lưu trữ nhật ký chi tiết (Audit Log) của mọi hành vi làm thay đổi kho (Nhập hàng, xuất hàng, bán hàng, hủy đơn). Bắt buộc ghi rõ lý do giao dịch để phục vụ đối soát.
