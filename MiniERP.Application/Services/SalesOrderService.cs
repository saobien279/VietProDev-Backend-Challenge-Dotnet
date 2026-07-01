using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.SalesOrders;
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
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public SalesOrderService(
            ISalesOrderRepository salesOrderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IStockTransactionRepository stockTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _salesOrderRepository = salesOrderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _stockTransactionRepository = stockTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<SalesOrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var sos = await _salesOrderRepository.GetAllAsync(cancellationToken);
            return sos.Select(MapToResponse);
        }

        public async Task<SalesOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var so = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);
            if (so == null)
            {
                throw new NotFoundException("Sales order not found.");
            }
            return MapToResponse(so);
        }

        public async Task<SalesOrderResponse> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra Customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                throw new NotFoundException("Customer not found.");
            }


            // 2. Gom nhóm Items trùng lặp ProductId và kiểm tra Product & Tồn kho (Bảo vệ sớm - Batch Load chống N+1 query)
            var groupedItems = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                }).ToList();

            var productIds = groupedItems.Select(x => x.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var productMap = products.ToDictionary(p => p.Id);

            var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds, cancellationToken);
            var inventoryMap = inventories.ToDictionary(i => i.ProductId);

            var orderItems = new List<SalesOrderItem>();

            foreach (var group in groupedItems)
            {
                // Kiểm tra Product tồn tại và active
                if (!productMap.TryGetValue(group.ProductId, out var product) || !product.IsActive)
                {
                    throw new BusinessValidationException($"Product with ID {group.ProductId} does not exist or is inactive.");
                }

                // Kiểm tra Tồn kho khả dụng
                inventoryMap.TryGetValue(group.ProductId, out var inventory);
                if (inventory == null || inventory.Quantity < group.TotalQuantity)
                {
                    throw new BusinessValidationException($"Insufficient stock for product '{product.ProductName}'. Requested: {group.TotalQuantity}, Available: {inventory?.Quantity ?? 0}.");
                }

                // Đóng băng giá bán tại thời điểm tạo đơn (lấy SellingPrice từ DB)
                orderItems.Add(new SalesOrderItem
                {
                    ProductId = group.ProductId,
                    Quantity = group.TotalQuantity,
                    UnitPrice = product.SellingPrice
                });
            }

            // 3. Tính tổng số tiền
            var totalAmount = orderItems.Sum(item => item.Quantity * item.UnitPrice);

            // 4. Tạo SalesOrder
            var so = new SalesOrder
            {
                CustomerId = request.CustomerId,
                Status = SalesOrderStatus.DRAFT,
                PaymentStatus = PaymentStatus.UNPAID,
                TotalAmount = totalAmount,
                Items = orderItems
            };

            _salesOrderRepository.Add(so);
            await _salesOrderRepository.SaveChangesAsync(cancellationToken); 

            // Fetch lại để có đầy đủ thông tin Customer và Product
            var createdSo = await _salesOrderRepository.GetByIdAsync(so.Id, cancellationToken);
            return MapToResponse(createdSo!);
        }

        public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // 1. Tìm SO
            var so = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);
            if (so == null)
            {
                throw new NotFoundException("Sales order not found.");
            }

            // 2. Guard clause
            if (so.Status != SalesOrderStatus.DRAFT)
            {
                throw new BusinessValidationException("Only draft sales orders can be confirmed.");
            }

            // 3. Đổi trạng thái sang CONFIRMED
            so.Status = SalesOrderStatus.CONFIRMED;

            // 4. Trừ kho & tạo Stock Transaction
            // [CHỐNG DEADLOCK DB]: Sắp xếp theo ProductId
            var sortedItems = so.Items
                .OrderBy(x => x.ProductId)
                .ToList();

            foreach (var item in sortedItems)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                
                // Kiểm tra kép trước khi trừ kho (phòng ngừa Race Condition)
                if (inventory == null || inventory.Quantity < item.Quantity)
                {
                    throw new BusinessValidationException($"Insufficient stock for product ID {item.ProductId} during confirmation.");
                }

                var stockBefore = inventory.Quantity;
                var stockAfter = stockBefore - item.Quantity;

                inventory.Quantity = stockAfter;
                inventory.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);

                // Ghi StockTransaction
                var tx = new StockTransaction
                {
                    ProductId = item.ProductId,
                    TransactionType = TransactionType.EXPORT,
                    Quantity = item.Quantity,
                    Reason = $"Export for Sales Order: {so.Id}",
                    ReferenceId = so.Id,
                    ReferenceType = ReferenceType.SalesOrder,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                };
                _stockTransactionRepository.Add(tx);
            }

            // 5. Lưu với kiểm tra tranh chấp (xmin)
            try
            {
                await _salesOrderRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("Concurrent modification detected. Please retry.");
            }
        }

        public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // 1. Tìm SO
            var so = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);
            if (so == null)
            {
                throw new NotFoundException("Sales order not found.");
            }

            // 2. Guard clause
            if (so.Status != SalesOrderStatus.DRAFT)
            {
                throw new BusinessValidationException("Only draft sales orders can be cancelled.");
            }

            // 3. Đổi trạng thái
            so.Status = SalesOrderStatus.CANCELLED;

            await _salesOrderRepository.SaveChangesAsync(cancellationToken);
        }

        private SalesOrderResponse MapToResponse(SalesOrder so)
        {
            return new SalesOrderResponse
            {
                Id = so.Id,
                CustomerId = so.CustomerId,
                CustomerName = so.Customer?.CustomerName ?? "Unknown",
                TotalAmount = so.TotalAmount,
                Status = so.Status.ToString(),
                PaymentStatus = so.PaymentStatus.ToString(),
                CreatedAt = so.CreatedAt,
                CreatedBy = so.CreatedBy,
                UpdatedAt = so.UpdatedAt,
                UpdatedBy = so.UpdatedBy,
                Items = so.Items.Select(item => new SalesOrderItemResponse
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
