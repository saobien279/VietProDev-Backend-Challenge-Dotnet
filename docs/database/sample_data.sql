-- =============================================================================
-- SAMPLE DATA SCRIPT (POSTGRESQL - LOWERCASE SNAKE_CASE)
-- Dùng để lưu vào file: docs/database/sample_data.sql
-- =============================================================================

-- 1. Bảng roles (Vai trò)
INSERT INTO roles (id, role_name) 
VALUES (1, 'ADMIN');

-- 2. Bảng users (Người dùng)
INSERT INTO users (id, username, password_hash, full_name, email, is_active, created_at) 
VALUES ('7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', 'admin_erp', 'AQAAAAIAAYagAAAAEI...', 'Nguyen Van Admin', 'admin@minierp.com', true, NOW());

-- 3. Bảng user_roles (Phân quyền người dùng)
INSERT INTO user_roles (user_id, role_id) 
VALUES ('7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', 1);

-- 4. Bảng customers (Khách hàng)
INSERT INTO customers (id, customer_name, email, phone, address, created_at) 
VALUES ('e3c4d5e6-7f8a-9b0c-1d2e-3f4a5b6c7d8e', 'Khách Hàng Mua Lẻ A', 'khachhangA@gmail.com', '0987654321', '123 Đường ABC, Quận 1, TP.HCM', NOW());

-- 5. Bảng suppliers (Nhà cung cấp)
INSERT INTO suppliers (id, supplier_name, email, phone, address, created_at) 
VALUES ('a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 'Công Ty TNHH Nước Giải Khát Coca-Cola', 'contact@cocacola.com.vn', '0281234567', 'Khu Công Nghiệp Linh Trung, Thủ Đức', NOW());

-- 6. Bảng units (Đơn vị tính)
INSERT INTO units (id, unit_name) 
VALUES (1, 'Lon');

-- 7. Bảng categories (Danh mục hàng hóa)
INSERT INTO categories (id, category_name, parent_id) 
VALUES (1, 'Đồ Uống', NULL);

INSERT INTO categories (id, category_name, parent_id) 
VALUES (2, 'Nước Ngọt', 1);

-- 8. Bảng products (Sản phẩm)
INSERT INTO products (id, sku, product_name, cost_price, selling_price, is_active, created_at, category_id, unit_id) 
VALUES ('c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 'COCA320ML', 'Nước Ngọt Coca-Cola Lon 320ml', 8000.00, 10000.00, true, NOW(), 2, 1);

-- 9. Bảng inventories (Tồn kho hiện tại)
INSERT INTO inventories (id, product_id, quantity, last_updated) 
VALUES ('12345678-1234-1234-1234-1234567890ab', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 100, NOW());

-- 10. Bảng purchase_orders (Đơn nhập hàng)
INSERT INTO purchase_orders (id, supplier_id, total_amount, status, created_by, created_at) 
VALUES ('b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 800000.00, 'CONFIRMED', '7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', NOW());

-- 11. Bảng purchase_order_items (Chi tiết đơn nhập)
INSERT INTO purchase_order_items (id, purchase_order_id, product_id, quantity, unit_price) 
VALUES (gen_random_uuid(), 'b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 100, 8000.00);

-- 12. Bảng sales_orders (Đơn bán hàng)
INSERT INTO sales_orders (id, customer_id, total_amount, status, payment_status, created_by, created_at) 
VALUES ('d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 'e3c4d5e6-7f8a-9b0c-1d2e-3f4a5b6c7d8e', 20000.00, 'CONFIRMED', 'PAID', '7f9b8c2d-3a5e-4a6f-bd1a-9c7e8f5b4a3a', NOW());

-- 13. Bảng sales_order_items (Chi tiết đơn bán)
INSERT INTO sales_order_items (id, sales_order_id, product_id, quantity, unit_price) 
VALUES (gen_random_uuid(), 'd1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 2, 10000.00);

-- 14. Bảng payments (Thanh toán đơn bán)
INSERT INTO payments (id, sales_order_id, payment_amount, payment_date, payment_method) 
VALUES ('f1a2b3c4-d5e6-7f8a-9b0c-1d2e3f4a5b6c', 'd1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a', 20000.00, NOW(), 'BANK_TRANSFER');

-- 15. Bảng stock_transactions (Lịch sử biến động kho)
INSERT INTO stock_transactions (id, product_id, transaction_type, quantity, reason, reference_id, created_at) 
VALUES (gen_random_uuid(), 'c9a8b7c6-d5e4-3f2a-1b0c-9a8b7c6d5e4f', 'IMPORT', 100, 'Nhập hàng từ đơn mua b5c6d7e8', 'b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e', NOW());
