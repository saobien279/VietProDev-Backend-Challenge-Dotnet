namespace MiniERP.Domain.Enums
{
    public enum ReferenceType
    {
        PurchaseOrder,   // Đơn mua hàng (Nhập kho)
        SalesOrder,      // Đơn bán hàng (Xuất kho)
        StockAdjustment, // Điều chỉnh kho (Thủ kho cân đối lại khi kiểm kê lệch)
        StockTransfer,   // Chuyển kho nội bộ
        Manual           // Nhập/xuất thủ công qua API
    }
}
