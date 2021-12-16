﻿using BootstrapAdmin.Web.Services;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// 添加示例后台任务
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddBootstrapBlazorAdmin(this IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 增加后台任务服务
            //services.AddTaskServices();

            //services.AddLogging(logging => logging.AddFileLogger().AddCloudLogger().AddDBLogger(ExceptionsHelper.Log));
            services.AddCors();
            services.AddResponseCompression();
            //services.AddCodePageProvider();

            //services.AddCacheManager();
            //services.AddDbAdapter();
            //services.AddIPLocator(DictHelper.ConfigIPLocator);
            //services.AddOnlineUsers();
            //services.AddSignalR().AddJsonProtocol(op => op.PayloadSerializerOptions.AddDefaultConverters());
            //services.AddBootstrapAdminAuthentication(Configuration)
            //    .AddGitee(OAuthHelper.Configure)
            //    .AddGitHub(OAuthHelper.Configure)
            //    .AddTencent(OAuthHelper.Configure)
            //    .AddAlipay(OAuthHelper.Configure);
            //services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireBootstrapAdminAuthorizate().Build());
            //services.AddButtonAuthorization(MenuHelper.AuthorizateButtons);
            //services.AddBootstrapAdminBackgroundTask();
            //services.AddHttpClient<GiteeHttpClient>();
            //services.AddAdminHealthChecks();
            //services.AddSMSProvider();

            //services.AddSwagger();
            //services.AddApiVersioning(option =>
            //{
            //    option.DefaultApiVersion = new ApiVersion(1, 0);
            //    option.ReportApiVersions = true;
            //    option.AssumeDefaultVersionWhenUnspecified = true;
            //    option.ApiVersionReader = ApiVersionReader.Combine(new HeaderApiVersionReader("api-version"), new QueryStringApiVersionReader("api-version"));
            //});
            //services.AddExceptionless();
            //services.AddControllersWithViews(options =>
            //{
            //    options.Filters.Add<BootstrapAdminAuthorizeFilter>();
            //    options.Filters.Add<ExceptionFilter>();
            //    options.Filters.Add<SignalRExceptionFilter<SignalRHub>>();
            //}).AddJsonOptions(op => op.JsonSerializerOptions.AddDefaultConverters());
            //services.AddControllersWithViews().AddJsonOptions(op => op.JsonSerializerOptions.AddDefaultConverters());

            // 增加 BootstrapBlazor 组件
            services.AddBootstrapBlazor();

            // 增加认证授权服务
            services.AddBootstrapAdminSecurity<AdminService>();

            // 增加 BootstrapApp 上下文服务
            services.AddScoped<BootstrapAppContext>();

            // 增加 EFCore 数据服务
            services.AddEFCoreDataAccessServices((provider, option) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connString = configuration.GetConnectionString("bb");
                option.UseSqlite(connString);
            });

            // 增加 PetaPoco 数据服务
            services.AddPetaPocoDataAccessServices();

            return services;
        }
    }
}