using BusinessLogic.Data;
using BusinessLogic.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure EF Core + MySQL
builder.Services.AddDbContext<DepositContext>(opts =>
    opts.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")!)
    )
);

// 2) Register your business logic
builder.Services.AddScoped<DepositCalculator>();

// 3) Add MVC controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4) Ensure the database and tables are created
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DepositContext>();
    ctx.Database.EnsureCreated();
}

// 5) Configure error pages
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Deposit/Error");

// 6) Static files, routing, and endpoints
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Deposit}/{action=Index}/{id?}"
);

app.Run();
