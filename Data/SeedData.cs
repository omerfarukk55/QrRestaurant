using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Veritabanı oluşturuldu mu kontrol et
                context.Database.EnsureCreated();

                // Kategoriler
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category
                        {
                            Name = "Başlangıçlar",
                            Description = "Çorbalar ve başlangıçlar",
                            DisplayOrder = 1,
                            IsActive = true
                        },
                        new Category
                        {
                            Name = "Ana Yemekler",
                            Description = "Et ve tavuk yemekleri",
                            DisplayOrder = 2,
                            IsActive = true
                        },
                        new Category
                        {
                            Name = "Burgerler",
                            Description = "Özel burgerlerimiz",
                            DisplayOrder = 3,
                            IsActive = true
                        },
                        new Category
                        {
                            Name = "Pizzalar",
                            Description = "İtalyan pizzaları",
                            DisplayOrder = 4,
                            IsActive = true
                        },
                        new Category
                        {
                            Name = "Tatlılar",
                            Description = "Tatlılar ve pastalar",
                            DisplayOrder = 5,
                            IsActive = true
                        },
                        new Category
                        {
                            Name = "İçecekler",
                            Description = "Soğuk ve sıcak içecekler",
                            DisplayOrder = 6,
                            IsActive = true
                        }
                    );

                    await context.SaveChangesAsync();
                }

                // Ürünler
                if (!context.Products.Any())
                {
                    // Başlangıçlar kategorisine ürünler ekle
                    var startersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Başlangıçlar");
                    if (startersCategory != null)
                    {
                        context.Products.AddRange(
                            new Product
                            {
                                Name = "Mercimek Çorbası",
                                Description = "Geleneksel Türk mercimek çorbası",
                                Price = 25.00M,
                                ImageUrl = "mercimek.jpg",
                                IsAvailable = true,
                                CategoryId = startersCategory.Id
                            },
                            new Product
                            {
                                Name = "Domates Çorbası",
                                Description = "Taze domateslerden yapılmış çorba",
                                Price = 25.00M,
                                ImageUrl = "domates.jpg",
                                IsAvailable = true,
                                CategoryId = startersCategory.Id
                            },
                            new Product
                            {
                                Name = "Meze Tabağı",
                                Description = "Humus, cacık, patlıcan salatası ve yaprak sarma",
                                Price = 65.00M,
                                ImageUrl = "meze.jpg",
                                IsAvailable = true,
                                CategoryId = startersCategory.Id
                            }
                        );
                    }

                    // Ana Yemekler kategorisine ürünler ekle
                    var mainsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Ana Yemekler");
                    if (mainsCategory != null)
                    {
                        context.Products.AddRange(
                            new Product
                            {
                                Name = "Izgara Köfte",
                                Description = "Özel baharatlarla hazırlanmış ızgara köfte, pilav ve salata ile",
                                Price = 85.00M,
                                ImageUrl = "kofte.jpg",
                                IsAvailable = true,
                                CategoryId = mainsCategory.Id
                            },
                            new Product
                            {
                                Name = "Tavuk Şiş",
                                Description = "Marine edilmiş tavuk şiş, pilav ve salata ile",
                                Price = 75.00M,
                                ImageUrl = "tavuksis.jpg",
                                IsAvailable = true,
                                CategoryId = mainsCategory.Id
                            },
                            new Product
                            {
                                Name = "Karışık Izgara",
                                Description = "Köfte, pirzola, tavuk şiş ve kanat, pilav ve salata ile",
                                Price = 120.00M,
                                ImageUrl = "karisik.jpg",
                                IsAvailable = true,
                                CategoryId = mainsCategory.Id
                            }
                        );
                    }

                    // Burgerler kategorisine ürünler ekle
                    var burgersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Burgerler");
                    if (burgersCategory != null)
                    {
                        context.Products.AddRange(
                            new Product
                            {
                                Name = "Klasik Burger",
                                Description = "Dana eti, cheddar peyniri, domates, marul ve özel sos",
                                Price = 70.00M,
                                ImageUrl = "klasikburger.jpg",
                                IsAvailable = true,
                                CategoryId = burgersCategory.Id
                            },
                            new Product
                            {
                                Name = "Cheeseburger",
                                Description = "Dana eti, çift cheddar peyniri, turşu, soğan ve hardal",
                                Price = 75.00M,
                                ImageUrl = "cheeseburger.jpg",
                                IsAvailable = true,
                                CategoryId = burgersCategory.Id
                            },
                            new Product
                            {
                                Name = "Tavuk Burger",
                                Description = "Izgara tavuk göğsü, mozzarella peyniri, avokado ve ranch sos",
                                Price = 65.00M,
                                ImageUrl = "tavukburger.jpg",
                                IsAvailable = true,
                                CategoryId = burgersCategory.Id
                            }
                        );
                    }

                    // İçecekler kategorisine ürünler ekle
                    var drinksCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "İçecekler");
                    if (drinksCategory != null)
                    {
                        context.Products.AddRange(
                            new Product
                            {
                                Name = "Kola",
                                Description = "330ml",
                                Price = 15.00M,
                                ImageUrl = "kola.jpg",
                                IsAvailable = true,
                                CategoryId = drinksCategory.Id
                            },
                            new Product
                            {
                                Name = "Ayran",
                                Description = "250ml",
                                Price = 10.00M,
                                ImageUrl = "ayran.jpg",
                                IsAvailable = true,
                                CategoryId = drinksCategory.Id
                            },
                            new Product
                            {
                                Name = "Türk Kahvesi",
                                Description = "Geleneksel Türk kahvesi",
                                Price = 20.00M,
                                ImageUrl = "turkkahvesi.jpg",
                                IsAvailable = true,
                                CategoryId = drinksCategory.Id
                            },
                            new Product
                            {
                                Name = "Çay",
                                Description = "Demlik çay",
                                Price = 8.00M,
                                ImageUrl = "cay.jpg",
                                IsAvailable = true,
                                CategoryId = drinksCategory.Id
                            }
                        );
                    }

                    await context.SaveChangesAsync();
                }

                // Masalar
                if (!context.Tables.Any())
                {
                    context.Tables.AddRange(
                        new Table
                        {
                            Name = "Masa 1",
                            IsActive = true
                        },
                        new Table
                        {
                            Name = "Masa 2",
                            IsActive = true
                        },
                        new Table
                        {
                            Name = "Masa 3",
                            IsActive = true
                        },
                        new Table
                        {
                            Name = "Masa 4",
                            IsActive = true
                        },
                        new Table
                        {
                            Name = "Masa 5",
                            IsActive = true
                        },
                        new Table
                        {
                            Name = "Masa 6",
                            IsActive = true
                        }
                    );

                    await context.SaveChangesAsync();
                }

                // Admin kullanıcısı oluştur
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Rolleri oluştur
                string[] roleNames = { "Admin", "Staff" };
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Admin kullanıcısı oluştur
                var adminUser = await userManager.FindByEmailAsync("admin@restoran.com");
                if (adminUser == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = "admin@restoran.com",
                        Email = "admin@restoran.com",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        }
    }
}