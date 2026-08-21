using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PackControl.Application.Abstractions;
using PackControl.Application.Assets;
using PackControl.Application.Auth;
using PackControl.Application.Carriers;
using PackControl.Application.Customers;
using PackControl.Application.Dashboard;
using PackControl.Application.Finance;
using PackControl.Application.Fiscal;
using PackControl.Application.Inventory;
using PackControl.Application.Logistics;
using PackControl.Application.Orders;
using PackControl.Application.Products;
using PackControl.Application.Production;
using PackControl.Application.Registers;
using PackControl.Application.Settings;
using PackControl.Infrastructure.Persistence;
using PackControl.Infrastructure.Services;

namespace PackControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPackControlInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<FileSystemStorageOptions>(configuration.GetSection("FileStorage"));
        services.Configure<StatePersistenceOptions>(configuration.GetSection("StatePersistence"));
        services.Configure<UnimakeFiscalEngineOptions>(configuration.GetSection(UnimakeFiscalEngineOptions.SectionName));
        services.AddSingleton<AppStateStore>();
        services.AddSingleton<IAppStatePersistence, PostgresAppStatePersistence>();
        services.AddSingleton<PasswordService>();
        services.AddSingleton<PlatformHealthService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<TechnicalDocumentAnalyzer>();
        services.AddScoped<IFileStorage, FileSystemStorage>();
        services.AddSingleton<IFiscalNfeEngineAdapter, MockFiscalNfeEngine>();
        services.AddSingleton<IFiscalNfeEngineAdapter, UnimakeFiscalNfeEngine>();
        services.AddSingleton<IFiscalNfeEngine, RoutingFiscalNfeEngine>();
        services.AddScoped<IFiscalDocumentService, FiscalDocumentService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICarrierService, CarrierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProductionService, ProductionService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ILogisticsService, LogisticsService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IRegistersService, RegistersService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
