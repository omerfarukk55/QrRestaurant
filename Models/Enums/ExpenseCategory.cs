using System.ComponentModel.DataAnnotations;

public enum ExpenseCategory
{
    [Display(Name = "Gıda Alımı")]
    FoodPurchase = 1,

    [Display(Name = "İçecek Alımı")]
    BeveragePurchase = 2,

    [Display(Name = "Personel Maaşı")]
    Salary = 3,

    [Display(Name = "Kira")]
    Rent = 4,

    [Display(Name = "Elektrik")]
    Electricity = 5,

    [Display(Name = "Su")]
    Water = 6,

    [Display(Name = "Doğalgaz")]
    Gas = 7,

    [Display(Name = "İnternet")]
    Internet = 8,

    [Display(Name = "Reklam")]
    Advertising = 9,

    [Display(Name = "Bakım/Onarım")]
    Maintenance = 10,

    [Display(Name = "Diğer")]
    Other = 11
}