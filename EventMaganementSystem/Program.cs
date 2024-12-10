using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventMaganementSystem.Data;
using EventManagementSystem.Core;
using EventMaganementSystem;
using EventManagementSystem;
using Microsoft.AspNetCore.SignalR;
using EventManagementSystem.Core.Extensions;
using Stripe;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<EventDbContext>(options =>
        {
            if (builder.Environment.IsEnvironment("Test"))
            {
                options.UseInMemoryDatabase("TestDatabase"); // Use in-memory for testing
            }
            else
            {
                options.UseSqlServer(connectionString); // Use SQL Server for other environments
            }
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Configure Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<EventDbContext>()
        .AddDefaultTokenProviders();

        // Add authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("OrganizerPolicy", policy => policy.RequireRole("Organizer"));
        });

        // Add services and dependencies
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        RegisterCustomServices(builder.Services);

        // Build the app
        var app = builder.Build();

        // Initialize roles and admin user
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            await EnsureRolesAndAdminUser(roleManager, userManager);
        }

        // Configure the HTTP request pipeline
        ConfigureMiddleware(app);

        app.Run();
    }

    public static void RegisterCustomServices(IServiceCollection services)
    {
        services.AddTransient<IProfileService, ProfileService>();
        services.AddTransient<IEventService, EventManagementSystem.Core.Services.EventService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IUserEventService, UserEventService>();
        services.AddScoped<IEventInvitationService, EventInvitationService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.Configure<StripeSettings>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("Stripe"));
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISponsorshipService, SponsorshipService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<INotificationHub, NotificationHubService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSignalR();
        services.AddScoped<IDiscountService, EventManagementSystem.Core.Services.DiscountService>();
        services.AddScoped<IPaymentMethodServiceWrapper, PaymentMethodServiceWrapper>();
        services.AddScoped<IPaymentMethodService, EventManagementSystem.Core.Extensions.StripePaymentMethodService>();


    }

    public static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts(); // Use HSTS with default 30 days
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication(); // Ensure authentication middleware is added
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chathub");
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
    }

    public static async Task EnsureRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        const string adminRole = "Admin";
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin@123";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}
