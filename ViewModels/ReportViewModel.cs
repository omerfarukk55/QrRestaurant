using RestaurantQRSystem.Models;
using RestaurantQRSystem.ViewModels;

public class ReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Order> Orders { get; set; }
    public List<Expense> Expenses { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Profit { get; set; }

    public int ReceivedCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CancelledCount { get; set; }

    public List<CategorySalesViewModel> SalesByCategory { get; set; }
    public List<TopProductViewModel> TopProducts { get; set; }
}