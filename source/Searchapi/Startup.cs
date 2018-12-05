using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using Inkton.Nester;
using Codesearch.Database;
using Codesearch.Model;

namespace Codesearch
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
            services.AddMvc();
            services.AddNester(QueueMode.Server | QueueMode.Client);
            services.AddNesterMySQL<QueryContext>();
            services.AddScoped<IQueryRepository, QueryRepository>();
            services.AddApiVersioning(options => 
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1,0);
                    options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
                });
            services.AddSwaggerGen(options => {
               options.SwaggerDoc("v1", new Info { Title = "Demo Messages API", Version = "v1" });
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "Demo Messages API V1");
            });

            app.UseMvc();
            app.Run(context => {
                context.Response.Redirect("swagger/");
                return Task.CompletedTask;
            });


            DBinitialize.EnsureCreated(app.ApplicationServices);
        }
    }
}
