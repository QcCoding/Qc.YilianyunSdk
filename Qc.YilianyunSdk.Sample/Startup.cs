using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Qc.YilianyunSdk.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddYilianyunSdk<YilianyunSdk.DefaultYilianyunSdkHook>(opt =>
            {
                // Ӧ��ID������ǰ�� dev.10ss.net ��ȡ
                opt.ClientId = "click_id";
                opt.ClientSecret = "client_secret";
                opt.YilianyunClientType = YilianyunClientType.����Ӧ��;
            });
            //�����ýڵ� YilianyunConfig ��ȡ
            //services.AddYilianyunSdk<YilianyunSdk.DefaultYilianyunSdkHook>(opt=>Configuration.Bind("YilianyunConfig"));

            //services.AddYilianyunSdk<YilianyunSdk.DefaultYilianyunSdkHook>(opt =>
            //{
            //    opt.Client_Id = "click_id";
            //    opt.Client_Secret = "client_secret";
            //    opt.YilianyunClientType = YilianyunClientType.����Ӧ��;
            //});

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {


            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
