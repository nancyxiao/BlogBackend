using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlogBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });




            //�]�wCookie-based����
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "UserLoginCookie";
                    options.LoginPath = new PathString("/Login/UserLogin");
                    options.LogoutPath = new PathString("/Login/Logout");
                });





            services.AddScoped<DbContext, BloggingContext>();
            //dependency for IUnitOfWork
            services.AddScoped<IUnitOfWork, EFUnitOfWork>();
            services.AddTransient<IMenusBLL, MenusService>();
            services.AddTransient<IMembersBLL, MembersService>();
            services.AddTransient<IRolesBLL, RolesService>();
            services.AddTransient<ILoginBLL, LoginRecordService>();
            services.AddTransient<IClassAllBLL, ClassAllService>();
            services.AddTransient<ITagsBLL, TagsService>();
            services.AddTransient<IArticlesBLL, ArticlesService>();

            string azureStorageConn = "";

            if (!Env.IsDevelopment())
            {
                azureStorageConn = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            }
            else
            {
                azureStorageConn = this.Configuration.GetConnectionString("AzureStorageConnectionString");
            }
            
            services.AddTransient<IUserIntroBLL>(srv => new UserIntroService(
                    srv.GetRequiredService<ILogger<UserIntroService>>(),
                    srv.GetRequiredService<IAzureBlobBLL>(),
                    srv.GetRequiredService<IUnitOfWork>(),
                    azureStorageConn
                ));
            services.AddTransient<IAzureBlobBLL, AzureBlobService>();
            services.AddScoped<AccessVerifyAttribute>();
            services.AddScoped<MenuAttribute>();
            services.AddScoped<RolesViewModel>();
            services.AddScoped<List<RolesViewModel>>();



            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Permission", policy => policy.Requirements.Add(new AccessVerifyRequirement()));
            //});
            //services.AddScoped<IAuthorizationHandler, AccessVerifyHandler>();
            ////services.AddSingleton<IAuthorizationHandler, AccessVerifyHandler>();


            services.AddRazorPages();
            //services.AddControllersWithViews();
            services.AddControllersWithViews().AddNewtonsoftJson();


            services.AddDbContext<DBClassLibrary.BloggingContext>(options =>
                options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc(options =>
            {
                options.CacheProfiles.Add("Default",
                                new CacheProfile()
                                {
                                    Duration = 0, //���(��)
                                    Location = ResponseCacheLocation.None,
                                    NoStore = true //�]�w�o��, client���s�����~�L�k�^�W�@��
                                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();//Controller�BAction�����A�[�W[RequireHttps]�ݩ�

            app.UseStaticFiles();

            //app.UseMvc();
            //app.UseMvcWithDefaultRoute();

            app.UseRouting();
            //�d�N����������...
            app.UseAuthentication();//Helps us to check ��Who are you?��
            app.UseAuthorization(); //Helps to check ��Are you allowed to access an information?��, Controller�BAction�~��[�W [Authorize] �ݩ�
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
