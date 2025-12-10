using AspNetCore.Unobtrusive.Ajax;
using DashBoard.Data;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string gerarCadastros = builder.Configuration["GerarCadastrosTeste"];
CacheSystem.SetGerarCadastrosTeste(string.IsNullOrWhiteSpace(gerarCadastros) ? false : gerarCadastros.ToLower() == "true");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Erro/HandleError/403";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
builder.Services.AddSession();
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddUnobtrusiveAjax();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IDataService, DataService>();
builder.Services.AddTransient<ApplicationDbContext>();
builder.Services.AddTransient<ICadastroRepository, CadastroRepository>();
builder.Services.AddTransient<INewsLetterRepository, NewsLetterRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IPermissoesRepository, PermissoesRepository>();
builder.Services.AddTransient<IEncryptData, EncryptData>();
builder.Services.AddTransient<ISendEmail, SendEmail>();
builder.Services.AddTransient<IEstadoRepository, EstadoRepository>();
builder.Services.AddTransient<ICidadeRepository, CidadeRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<IDataService>().InitDB();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Erro/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Erro/HandleError/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landingpage}/{action=Index}/{id?}"
);

app.MapRazorPages();
app.UseUnobtrusiveAjax();

CacheSystem.CarregaListasEnums();
var appScope = app.Services.CreateScope();
await CacheSystem.CarregaListaEstadosECidades(appScope.ServiceProvider.GetRequiredService<IEstadoRepository>()!, appScope.ServiceProvider.GetRequiredService<ICidadeRepository>()!);
await CacheSystem.AtualizaStatsInicio(appScope.ServiceProvider.GetRequiredService<ICadastroRepository>()!);
await appScope.ServiceProvider.GetRequiredService<ICadastroRepository>().CadastrosTeste();
app.Run();
