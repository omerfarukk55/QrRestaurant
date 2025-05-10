using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.Services;

namespace RestaurantQRSystem
{
    public class Startup
    {
        // IConfiguration nesnesini tanımla
        public IConfiguration Configuration { get; }

        // Constructor'da IConfiguration nesnesini al
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<OrderNotificationService>();

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
            // Yetkilendirme politikalarını yapılandırma
            services.AddAuthorization(options =>
            {
                // Admin rolü için politika
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));

                // Yönetici rolü için politika
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager", "Admin"));

                // Personel rolü için politika
                options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff", "Manager", "Admin"));
            });
            // Diğer servis yapılandırmaları...
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint(); // UseDatabaseErrorPage yerine bu kullanılıyor
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();

                // SignalR hub endpoint
                endpoints.MapHub<OrderHub>("/orderHub");
            });


        }
    }
}