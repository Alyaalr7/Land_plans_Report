using Microsoft.EntityFrameworkCore;
using Land_plans_Report.Data;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ValidateComplexTypesIfChildValidationFails = true;
});
// ربط قاعدة البيانات الفعلي (بدل البيانات الثابتة بالكود)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// الجلسة (Session) لتخزين هوية المستخدم بعد تسجيل الدخول
// حل مؤقت إلى حين ربط نظام نفاذ لاحقاً
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 100 * 1024 * 1024);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 100 * 1024 * 1024);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");   // ← جديد


app.UseStaticFiles();
app.UseRouting();

app.UseSession();          // لازم قبل UseAuthorization وقبل الوصول لأي Controller يستخدم Session
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
