using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.PurchaseOrders;
using MiniERP.Application.Exceptions;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public PurchaseOrderService(
            IPurchaseOrderRepository purchaseOrderRepository,
            ISupplierRepository supplierRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IStockTransactionRepository stockTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _supplierRepository = supplierRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _stockTransactionRepository = stockTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var pos = await _purchaseOrderRepository.GetAllAsync(cancellationToken);
            return pos.Select(MapToResponse);
        }

        public async Task<PurchaseOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var po = await _purchaseOrderRepository.GetByIdAsync(id, cancellationToken);
            if (po == null)
            {
                throw new NotFoundException("Purchase order not found.");
            }
            return MapToResponse(po);
        }

        public async Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra Supplier
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
            if (supplier == null)
            {
                throw new NotFoundException("Supplier not found.");
            }

            // 2. Kiểm tra Products
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            foreach (var productId in productIds)
            {
                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null || !product.IsActive)
                {
                    throw new BusinessValidationException($"Product with ID {productId} does not exist or is inactive.");
                }
            }

            // 3. Tính toán TotalAmount
            var totalAmount = request.Items.Sum(item => item.Quantity * item.UnitPrice);

            // 4. Tạo PO
            var po = new PurchaseOrder
            {
                SupplierId = request.SupplierId,
                Status = PurchaseOrderStatus.DRAFT,
                TotalAmount = totalAmount,
                Items = request.Items.Select(item => new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            // 5. Lưu PO
            _purchaseOrderRepository.Add(po);
            await _purchaseOrderRepository.SaveChangesAsync(cancellationToken);

            // Fetch lại để có tên Supplier và Tên Product
            var createdPo = await _purchaseOrderRepository.GetByIdAsync(po.Id, cancellationToken);
            return MapToResponse(createdPo!);
        }

        public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // 1. Tìm PO
            var po = await _purchaseOrderRepository.GetByIdAsync(id, cancellationToken);
            if (po == null)
            {
                throw new NotFoundException("Purchase order not found.");
            }

            // 2. Guard clause
            if (po.Status != PurchaseOrderStatus.DRAFT)
            {
                throw new BusinessValidationException("Only draft purchase orders can be confirmed.");
            }

            // 3. Đổi trạng thái
            po.Status = PurchaseOrderStatus.CONFIRMED;

            // 4. Cập nhật tồn kho & Stock Transaction
            // [GIẢI QUYẾT TRÙNG LẶP PRODUCT] + [CHỐNG DEADLOCK DB]: GroupBy và sắp xếp theo ProductId
            var groupedItems = po.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.ProductId)
                .ToList();

            foreach (var group in groupedItems)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(group.ProductId, cancellationToken);
                var stockBefore = inventory?.Quantity ?? 0;
                var stockAfter = stockBefore + group.TotalQuantity;

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = group.ProductId,
                        Quantity = stockAfter,
                        LastUpdated = DateTime.UtcNow
                    };
                    _inventoryRepository.Add(inventory);
                }
                else
                {
                    inventory.Quantity = stockAfter;
                    inventory.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.Update(inventory);
                }

                // Tạo StockTransaction
                var tx = new StockTransaction
                {
                    ProductId = group.ProductId,
                    TransactionType = TransactionType.IMPORT,
                    Quantity = group.TotalQuantity,
                    Reason = $"Import from Purchase Order: {po.Id}",
                    ReferenceId = po.Id,
                    ReferenceType = ReferenceType.PurchaseOrder,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                };
                _stockTransactionRepository.Add(tx);
            }

            // 5. Lưu với Optimistic Concurrency check
            try
            {
                await _purchaseOrderRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("Concurrent modification detected. Please retry.");
            }
        }

        public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // 1. Tìm PO
            var po = await _purchaseOrderRepository.GetByIdAsync(id, cancellationToken);
            if (po == null)
            {
                throw new NotFoundException("Purchase order not found.");
            }

            // 2. Guard clause
            if (po.Status != PurchaseOrderStatus.DRAFT)
            {
                throw new BusinessValidationException("Only draft purchase orders can be cancelled.");
            }

            // 3. Đổi trạng thái
            po.Status = PurchaseOrderStatus.CANCELLED;

            await _purchaseOrderRepository.SaveChangesAsync(cancellationToken);
        }

        private PurchaseOrderResponse MapToResponse(PurchaseOrder po)
        {
            return new PurchaseOrderResponse
            {
                Id = po.Id,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier?.SupplierName ?? "Unknown",
                TotalAmount = po.TotalAmount,
                Status = po.Status.ToString(),
                CreatedAt = po.CreatedAt,
                CreatedBy = po.CreatedBy,
                UpdatedAt = po.UpdatedAt,
                UpdatedBy = po.UpdatedBy,
                Items = po.Items.Select(item => new PurchaseOrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName ?? "Unknown",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }
    }
}
