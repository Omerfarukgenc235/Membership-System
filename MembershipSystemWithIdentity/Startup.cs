using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MembershipSystemWithIdentity.CustomValidation;
using MembershipSystemWithIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MembershipSystemWithIdentity
{
    public class Startup
    {
        //appsettings'e ula�mak i�in bu y�ntemi kullan�r�z.
        public IConfiguration configuration { get; }
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => options.EnableEndpointRouting = false);
            services.AddControllersWithViews();

            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionStrings"]);
            });

            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();

            //Burada yazd���m�z Claim k�s�tlamalar�n� ayarl�yoruz.
            services.AddAuthorization(opts => 
            {
                //K�s�tlamam�z�n ismi "IstanbulPolicy" ve bu ismi hangi controller'a verirsek o contrroller i�in �al��acak.
                opts.AddPolicy("IstanbulPolicy", policy =>
                {
                    //K�s�tlaman�n Tipi "city". Bu tip kenti olu�turdu�umuz ClaimProvider i�indeki Tip'den gelir.
                    //�kinci de�i�ken olan �stanbul ise Value'dir. E�er databaseden gelen city de�eri ile bu de�i�ken ayn� ise sayfaya giri� yap�labilir.
                    policy.RequireClaim("city", "�stanbul");
                });

                opts.AddPolicy("ViolencePolicy", policy =>
                {
                    policy.RequireClaim("violence");
                });
                opts.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirement());                
                });
            });

            //secrets.json dosyas�ndan geliyor
            services.AddAuthentication()
                .AddFacebook(opts =>
                {
                    opts.AppId = configuration["Authentication:Facebook:AppId"];
                    opts.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                })
                .AddGoogle(opts =>
                {
                    opts.ClientId = configuration["Authentication:Google:ClientID"];
                    opts.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                })
                .AddMicrosoftAccount(opts =>
                {
                    opts.ClientId = configuration["Authentication:Microsoft:ClientId"];
                    opts.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                });

            services.AddIdentity<AppUser, AppRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abc�defg�h�ijklmno�pqrs�tu�vwxyzABC�DEFG�HI�JKLMNO�PQRS�TU�VWXYZ0123456789-._";
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireDigit = false;
            })
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddUserValidator<CustomUserValidator>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            CookieBuilder cookieBuilder = new CookieBuilder();

            cookieBuilder.Name = "MyBlog";
            //Cookie bilgisini k�t� ama�l� kullan�clar okumas�n diye False yap�yoruz.
            cookieBuilder.HttpOnly = false;
            //E�er proje kritik bilgiler i�erseydi �rne�in banka bilgileri gibi, bu �zleli�i Strict yapmam�z laz�m. K�t� ama�l� kullan�c�lar Cookie bilgilerini kullanamas�n diye.
            cookieBuilder.SameSite = SameSiteMode.Lax;
            //Always dersen b�t�n istekler HTTPS den g�nderilir. 
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;


            services.ConfigureApplicationCookie(opts =>
            {
                //Kullan�c� �ye olmadan, sadece �yelerin eri�ebildi�i sayfaya giderse biz onu Login sayfas�na y�nlendiriyoruz.
                opts.LoginPath = new PathString("/Home/Login");
                opts.LogoutPath = new PathString("/Member/LogOut");
                opts.Cookie = cookieBuilder;
                opts.SlidingExpiration = true;
                opts.ExpireTimeSpan = TimeSpan.FromDays(60);
                //E�er �ye olan kullan�c�, yetkisi olmayan sayfalara giri� yapmak isterse a�a��daki Path'e y�nlendiriyoruz.
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied");
            });

            //Her request iste�inde cookie olu�urken benim class'�m�nda �al��mas�n�da istiyorum.
            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();

            services.AddMvc(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Sayfada hata ald���m�zda o hata ile ilgili a��klay�c� bilgiler sunar
            app.UseDeveloperExceptionPage();
            // Bo� content d�nd���nde bize hatan�n nerede oldu�unu g�steren Status Code'lar�n� d�ner
            app.UseStatusCodePages();
            // JS gibi CSS gibi dosyalar�n y�klenebilmesini sa�lar
            app.UseStaticFiles();
            // Identity k�t�phanesi kullanaca��m�z i�in bunu ekledik
            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();

            



            //app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }
    }
}
