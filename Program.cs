using BAL.Interface;
using BAL.Repository;
using DAL.DbModels;
using DAL.Interface;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<HprojectContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));
builder.Services.AddScoped<IBookingDA, BookingDA>();
builder.Services.AddScoped<IBookingBA, BookingBA>();
builder.Services.AddScoped<IRoomDA, RoomDA>();
builder.Services.AddScoped<IRoomBA, RoomBA>();
builder.Services.AddScoped<ICustomerDA, CustomerDA>();
builder.Services.AddScoped<ICustomerBA, CustomerBA>();
builder.Services.AddScoped<IServiceDA, ServiceDA>();
builder.Services.AddScoped<IServiceBA, ServiceBA>();
builder.Services.AddScoped<IBillDA, BillDA>();
builder.Services.AddScoped<IBillBA, BillBA>();
builder.Services.AddScoped<IRegistrationDA, RegistrationDA>();
builder.Services.AddScoped<IRegistrationBA, RegistrationBA>();

//builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Configure services
//builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthentication("custom")
    .AddCookie("custom", options =>
    {
        options.Cookie.Name = "custom_auth_cookie"; // Customize the cookie settings
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.LoginPath = "/Login/Index"; // The login page URL
        // ... other options ...
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust this as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireRole("User"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
