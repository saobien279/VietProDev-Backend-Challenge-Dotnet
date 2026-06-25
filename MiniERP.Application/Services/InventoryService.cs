using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.Inventory;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IStockTransactionRepository stockTransactionRepository)
        {
            _inventoryRepository = inventoryRepository;
            _stockTransactionRepository = stockTransactionRepository;
        }

        public async Task<InventoryResponse?> GetStockByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
            if (inventory == null) return null;

            return MapToResponse(inventory);
        }

        public async Task<IEnumerable<StockTransactionResponse>> GetTransactionsAsync(CancellationToken cancellationToken = default)
        {
            var transactions = await _stockTransactionRepository.GetAllAsync(cancellationToken);
            return transactions.Select(MapToTransactionResponse);
        }

        public async Task<InventoryResponse> ImportAsync(ImportStockRequest request, CancellationToken cancellationToken = default)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            
            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _inventoryRepository.Add(inventory);
            }
            else
            {
                inventory.Quantity += request.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);
            }

            var transaction = new StockTransaction
            {
                ProductId = request.ProductId,
                TransactionType = "IMPORT",
                Quantity = request.Quantity,
                Reason = request.Reason,
                ReferenceId = null
            };

            _stockTransactionRepository.Add(transaction);

            try
            {
                await _inventoryRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Concurrent modification detected. Please retry.");
            }

            // Re-fetch to populate navigation properties
            var updatedInventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            return MapToResponse(updatedInventory!);
        }

        public async Task<InventoryResponse> ExportAsync(ExportStockRequest request, CancellationToken cancellationToken = default)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

            if (inventory == null || inventory.Quantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock for export.");
            }

            inventory.Quantity -= request.Quantity;
            inventory.LastUpdated = DateTime.UtcNow;
            _inventoryRepository.Update(inventory);

            var transaction = new StockTransaction
            {
                ProductId = request.ProductId,
                TransactionType = "EXPORT",
                Quantity = request.Quantity,
                Reason = request.Reason,
                ReferenceId = null
            };

            _stockTransactionRepository.Add(transaction);

            try
            {
                await _inventoryRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Concurrent modification detected. Please retry.");
            }

            // Re-fetch to populate navigation properties
            var updatedInventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            return MapToResponse(updatedInventory!);
        }

        private static InventoryResponse MapToResponse(Inventory inventory)
        {
            return new InventoryResponse
            {
                ProductId = inventory.ProductId,
                ProductName = inventory.Product?.ProductName ?? string.Empty,
                Sku = inventory.Product?.Sku ?? string.Empty,
                Quantity = inventory.Quantity,
                LastUpdated = inventory.LastUpdated
            };
        }

        private static StockTransactionResponse MapToTransactionResponse(StockTransaction transaction)
        {
            return new StockTransactionResponse
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductName = transaction.Product?.ProductName ?? string.Empty,
                Sku = transaction.Product?.Sku ?? string.Empty,
                TransactionType = transaction.TransactionType,
                Quantity = transaction.Quantity,
                Reason = transaction.Reason,
                ReferenceId = transaction.ReferenceId,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
