-- =============================================================================
-- CHALLENGE 1: DATABASE INITIALIZATION SCRIPT (POSTGRESQL)
-- Dùng để lưu vào file: database/init.sql
-- =============================================================================

-- Kích hoạt tiện ích mở rộng để sinh UUID tự động nếu chưa có
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ==========================================
-- 1. PHÂN HỆ AUTH & CORE (users, roles, user_roles)
-- ==========================================

CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_roles (
    user_id UUID NOT NULL,
    role_id INT NOT NULL,
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

-- ==========================================
-- 2. PHÂN HỆ MASTER DATA (customers, suppliers, categories, units, products)
-- ==========================================

CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_name VARCHAR(100) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(20) NOT NULL UNIQUE,
    address VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE suppliers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    supplier_name VARCHAR(100) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(20) NOT NULL UNIQUE,
    address VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    parent_id INT,
    FOREIGN KEY (parent_id) REFERENCES categories(id) ON DELETE RESTRICT
);

CREATE TABLE units (
    id SERIAL PRIMARY KEY,
    unit_name VARCHAR(50) NOT NULL
);

CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sku VARCHAR(50) NOT NULL UNIQUE,
    product_name VARCHAR(150) NOT NULL,
    cost_price NUMERIC(18, 2) NOT NULL DEFAULT 0,
    selling_price NUMERIC(18, 2) NOT NULL DEFAULT 0,
    category_id INT NOT NULL,
    unit_id INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (unit_id) REFERENCES units(id) ON DELETE RESTRICT,
    CONSTRAINT "CK_Product_CostPrice" CHECK (cost_price >= 0),
    CONSTRAINT "CK_Product_SellingPrice" CHECK (selling_price >= 0)
);

-- ==========================================
-- 3. PHÂN HỆ QUẢN LÝ KHO (inventory, stock_transactions)
-- ==========================================

CREATE TABLE inventory (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL UNIQUE,
    quantity INT NOT NULL DEFAULT 0,
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    CONSTRAINT "CK_Inventory_Quantity" CHECK (quantity >= 0)
);

CREATE TABLE stock_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL,
    transaction_type VARCHAR(20) NOT NULL, -- IMPORT, EXPORT
    quantity INT NOT NULL,
    reason VARCHAR(255) NOT NULL,
    reference_id UUID,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT
);

-- ==========================================
-- 4. PHÂN HỆ ĐƠN HÀNG & THANH TOÁN (purchase_orders, purchase_order_items, sales_orders, sales_order_items, payments)
-- ==========================================

-- --- Phân hệ Nhập Hàng (Purchase) ---
CREATE TABLE purchase_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    supplier_id UUID NOT NULL,
    total_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT, CONFIRMED, CANCELLED
    created_by UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT
);

CREATE TABLE purchase_order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price NUMERIC(18, 2) NOT NULL,
    FOREIGN KEY (purchase_order_id) REFERENCES purchase_orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    CONSTRAINT "CK_PO_Item_Quantity" CHECK (quantity > 0),
    CONSTRAINT "CK_PO_Item_UnitPrice" CHECK (unit_price >= 0)
);

-- --- Phân hệ Bán Hàng (Sales) ---
CREATE TABLE sales_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL,
    total_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT, CONFIRMED, CANCELLED
    payment_status VARCHAR(20) NOT NULL DEFAULT 'UNPAID', -- UNPAID, PARTIAL, PAID
    created_by UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES customers(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT
);

CREATE TABLE sales_order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price NUMERIC(18, 2) NOT NULL,
    FOREIGN KEY (sales_order_id) REFERENCES sales_orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    CONSTRAINT "CK_SO_Item_Quantity" CHECK (quantity > 0),
    CONSTRAINT "CK_SO_Item_UnitPrice" CHECK (unit_price >= 0)
);

-- --- Phân hệ Thanh Toán (Payments) ---
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_order_id UUID NOT NULL,
    payment_amount NUMERIC(18, 2) NOT NULL,
    payment_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    payment_method VARCHAR(50) NOT NULL, -- CASH, BANK_TRANSFER
    FOREIGN KEY (sales_order_id) REFERENCES sales_orders(id) ON DELETE RESTRICT,
    CONSTRAINT "CK_Payment_Amount" CHECK (payment_amount > 0)
);

-- ==========================================
-- 5. CẤU HÌNH CÁC INDEX CƠ BẢN ĐỂ TỐI ƯU TÌM KIẾM
-- ==========================================
CREATE INDEX IX_products_sku ON products(sku);
CREATE INDEX IX_stock_transactions_product_id ON stock_transactions(product_id);
CREATE INDEX IX_sales_orders_created_at ON sales_orders(created_at);
CREATE INDEX IX_purchase_orders_created_at ON purchase_orders(created_at);