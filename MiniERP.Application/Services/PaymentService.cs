using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.Payments;
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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ISalesOrderRepository salesOrderRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _salesOrderRepository = salesOrderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsBySalesOrderIdAsync(Guid salesOrderId, CancellationToken cancellationToken = default)
        {
            var so = await _salesOrderRepository.GetByIdAsync(salesOrderId, cancellationToken);
            if (so == null)
            {
                throw new NotFoundException("Sales order not found.");
            }

            var payments = await _paymentRepository.GetBySalesOrderIdAsync(salesOrderId, cancellationToken);

            return payments.Select(MapToResponse);
        }

        public async Task<PaymentResponse> AddPaymentAsync(Guid salesOrderId, CreatePaymentRequest request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Khóa và lấy SalesOrder theo ID
                var so = await _salesOrderRepository.GetByIdAsync(salesOrderId, cancellationToken);
                if (so == null)
                {
                    throw new NotFoundException("Sales order not found.");
                }

                // 2. Kiểm tra tính hợp lệ
                if (so.Status != SalesOrderStatus.CONFIRMED)
                {
                    throw new BusinessValidationException("Can only add payment to CONFIRMED sales orders.");
                }

                // 3. Kiểm tra số tiền
                var currentPayments = await _paymentRepository.GetBySalesOrderIdAsync(salesOrderId, cancellationToken);
                var totalPaid = currentPayments.Sum(p => p.PaymentAmount);

                if (totalPaid + request.PaymentAmount > so.TotalAmount)
                {
                    throw new BusinessValidationException($"Payment amount exceeds total order amount. Remaining balance: {so.TotalAmount - totalPaid}.");
                }

                // 4. Tạo bản ghi Payment
                var payment = new Payment
                {
                    SalesOrderId = salesOrderId,
                    PaymentAmount = request.PaymentAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = DateTime.UtcNow
                };

                _paymentRepository.Add(payment);

                // 5. Cập nhật PaymentStatus cho SalesOrder
                var newTotalPaid = totalPaid + request.PaymentAmount;
                if (newTotalPaid >= so.TotalAmount)
                {
                    so.PaymentStatus = PaymentStatus.PAID;
                }
                else
                {
                    so.PaymentStatus = PaymentStatus.PARTIAL;
                }

                _salesOrderRepository.Update(so);

                // 6. Lưu xuống DB với xử lý Concurrency (xmin)
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Commit transaction nếu thành công
                await transaction.CommitAsync(cancellationToken);

                return MapToResponse(payment);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new ConcurrencyConflictException("Concurrent payment detected. Please retry.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                SalesOrderId = payment.SalesOrderId,
                PaymentAmount = payment.PaymentAmount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod.ToString(),
                    CreatedBy = payment.CreatedBy,
                    CreatedAt = payment.CreatedAt
            };
        }
    }
}
