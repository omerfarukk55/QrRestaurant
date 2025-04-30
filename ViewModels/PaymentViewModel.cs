using System;
using System.Collections.Generic;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;

namespace RestaurantQRSystem.ViewModels
{
    public class PaymentViewModel
    {
        public class PaymentIndexViewModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public PaymentStatus? Status { get; set; }
            public List<Payment> Payments { get; set; }
            public PaymentStatisticsViewModel Statistics { get; set; }
        }

        public class PaymentStatisticsViewModel
        {
            public decimal TotalAmount { get; set; }
            public decimal CashAmount { get; set; }
            public decimal CardAmount { get; set; }
            public decimal OtherAmount { get; set; }
            public int CompletedCount { get; set; }
            public int PendingCount { get; set; }
            public int RefundedCount { get; set; }
        }

        public class CreatePaymentViewModel
        {
            public int OrderId { get; set; }
            public string TableName { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; }
            public string CustomerName { get; set; }
            public string Notes { get; set; }
            public bool CompleteOrder { get; set; } = true;
            public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        }

        public class PaymentReportViewModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int TotalPayments { get; set; }
            public decimal TotalAmount { get; set; }
            public int RefundedPayments { get; set; }
            public decimal RefundedAmount { get; set; }
            public decimal NetAmount { get; set; }
            public List<PaymentMethodSummary> PaymentsByMethod { get; set; }
            public List<DailyPaymentSummary> DailyPayments { get; set; }
            public List<Payment> RefundedPaymentsList { get; set; }
        }

        public class PaymentMethodSummary
        {
            public string Method { get; set; }
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class DailyPaymentSummary
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class HourlyPaymentSummary
        {
            public int Hour { get; set; }
            public string TimeSlot { get; set; }
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class DailyPaymentViewModel
        {
            public DateTime Date { get; set; }
            public List<Payment> Payments { get; set; }
            public int CompletedPaymentCount { get; set; }
            public decimal CompletedPaymentAmount { get; set; }
            public int RefundedPaymentCount { get; set; }
            public decimal RefundedPaymentAmount { get; set; }
            public decimal NetAmount { get; set; }
            public List<PaymentMethodSummary> PaymentsByMethod { get; set; }
            public List<HourlyPaymentSummary> HourlyPayments { get; set; }
        }
    }
}