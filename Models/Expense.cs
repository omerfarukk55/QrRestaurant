using System.ComponentModel.DataAnnotations;

public class Expense
{
    public int Id { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;
    public ExpenseCategory Category { get; set; }
}