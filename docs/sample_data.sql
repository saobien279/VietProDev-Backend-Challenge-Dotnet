-- 1. Bảng Roles (Vai trò)
-- Tạo vai trò Quản trị viên (ADMIN)
INSERT INTO "Roles" ("Id", "RoleName") 
VALUES (1, 'ADMIN');

-- 2. Bảng Users (Người dùng)
-- Tạo tài khoản nhân viên quản trị (Mật khẩu đã được mã hóa sẵn)
INSERT INTO "Users" ("Id", "Username", "PasswordHash", "FullName", "Email", "IsActive", "CreatedAt") 
VALUES ('7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', 'admin_erp', 'AQAAAAIAAYagAAAAEI...', 'Nguyen Van Admin', 'admin@minierp.com', true, NOW());

-- 3. Bảng UserRoles (Phân quyền người dùng)
-- Gán vai trò ADMIN cho tài khoản admin_erp
INSERT INTO "UserRoles" ("UserId", "RoleId") 
VALUES ('7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', 1);

-- 4. Bảng Customers (Khách hàng)
-- Khách hàng mua lẻ
INSERT INTO "Customers" ("Id", "CustomerName", "Email", "Phone", "Address", "CreatedAt") 
VALUES ('e3c4d5e6-7f8a-9b0c-1d2e-3f4a5b6c7d8e', 'Khách Hàng Mua Lẻ A', 'khachhangA@gmail.com', '0987654321', '123 Đường ABC, Quận 1, TP.HCM', NOW());

-- 5. Bảng Suppliers (Nhà cung cấp)
-- Nhà cung cấp nước giải khát
INSERT INTO "Suppliers" ("Id", "SupplierName", "Email", "Phone", "Address", "CreatedAt") 
VALUES ('a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 'Công Ty TNHH Nước Giải Khát Coca-Cola', 'contact@cocacola.com.vn', '0281234567', 'Khu Công Nghiệp Linh Trung, Thủ Đức', NOW());

-- 6. Bảng Units (Đơn vị tính)
-- Tạo đơn vị tính cơ bản là 'Lon'
INSERT INTO "Units" ("Id", "UnitName") 
VALUES (1, 'Lon');

-- 7. Bảng Categories (Danh mục hàng hóa - Tự tham chiếu)
-- Tạo danh mục cha 'Đồ uống' và danh mục con 'Nước ngọt' trỏ vào cha
INSERT INTO "Categories" ("Id", "CategoryName", "ParentId") 
VALUES (1, 'Đồ Uống', NULL);

INSERT INTO "Categories" ("Id", "CategoryName", "ParentId") 
VALUES (2, 'Nước Ngọt', 1);

-- 8. Bảng Products (Sản phẩm)
-- Tạo sản phẩm Coca-Cola Lon 320ml, thuộc danh mục Nước Ngọt (Id=2), đơn vị tính Lon (Id=1)
INSERT INTO "Products" ("Id", "Sku", "ProductName", "CostPrice", "SellingPrice", "IsActive", "CreatedAt", "CategoryId", "UnitId") 
VALUES ('c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 'COCA320ML', 'Nước Ngọt Coca-Cola Lon 320ml', 8000.00, 10000.00, true, NOW(), 2, 1);

-- 9. Bảng Inventory (Tồn kho hiện tại - Quan hệ 1-1 với Product)
-- Hiện tại trong kho đang có sẵn 100 lon Coca-Cola
INSERT INTO "Inventory" ("Id", "ProductId", "Quantity", "LastUpdated") 
VALUES ('12345678-1234-1234-1234-1234567890ab', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 100, NOW());

-- 10. Bảng PurchaseOrders (Đơn nhập hàng từ nhà cung cấp)
-- Đơn nhập hàng giá trị 800,000đ do Admin tạo
INSERT INTO "PurchaseOrders" ("Id", "SupplierId", "TotalAmount", "Status", "CreatedBy", "CreatedAt") 
VALUES ('b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 800000.00, 'CONFIRMED', '7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', NOW());

-- 11. Bảng PurchaseOrderItems (Chi tiết đơn nhập)
-- Nhập 100 lon Coca-Cola với giá gốc 8,000đ/lon tại thời điểm nhập
INSERT INTO "PurchaseOrderItems" ("Id", "PurchaseOrderId", "ProductId", "Quantity", "UnitPrice") 
VALUES (GEN_RANDOM_UUID(), 'b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 100, 8000.00);

-- 12. Bảng SalesOrders (Đơn bán hàng cho khách hàng)
-- Đơn bán trị giá 20,000đ do Admin tạo cho khách lẻ
INSERT INTO "SalesOrders" ("Id", "CustomerId", "TotalAmount", "Status", "PaymentStatus", "CreatedBy", "CreatedAt") 
VALUES ('d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 'e3c4d5e6-7f8a-9b0c-1d2e-3f4a5b6c7d8e', 20000.00, 'CONFIRMED', 'PAID', '7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', NOW());

-- 13. Bảng SalesOrderItems (Chi tiết đơn bán)
-- Bán lẻ 2 lon Coca-Cola với giá 10,000đ/lon tại thời điểm bán
INSERT INTO "SalesOrderItems" ("Id", "SalesOrderId", "ProductId", "Quantity", "UnitPrice") 
VALUES (GEN_RANDOM_UUID(), 'd1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 2, 10000.00);

-- 14. Bảng Payments (Thanh toán đơn bán)
-- Khách thanh toán 20,000đ bằng chuyển khoản ngân hàng
INSERT INTO "Payments" ("Id", "SalesOrderId", "PaymentAmount", "PaymentDate", "PaymentMethod") 
VALUES ('f1a2b3c4-d5e6-7f8a-9b0c-1d2e3f4a5b6c', 'd1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 20000.00, NOW(), 'BANK_TRANSFER');

-- 15. Bảng StockTransactions (Lịch sử biến động kho)
-- Ghi nhận lịch sử nhập 100 lon (TransactionType = 'IMPORT')
INSERT INTO "StockTransactions" ("Id", "ProductId", "TransactionType", "Quantity", "Reason", "ReferenceId", "CreatedAt") 
VALUES (GEN_RANDOM_UUID(), 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 'IMPORT', 100, 'Nhập hàng từ đơn mua b5c6d7e8', 'b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', NOW());
