using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.OpenApi.Models;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VA.TMP.Integration.Api.TelehealthSpecialtyLocation", Version = "v1" });
            });
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddApplicationInsightsTelemetry();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddLog4Net();
                builder.AddAzureWebAppDiagnostics();
            });

            services.AddMemoryCache();

            services.Configure<AzureFileLoggerOptions>(options =>
            {
                options.FileName = "azure-diagnostics-";
                options.FileSizeLimit = 50 * 1024;
                options.RetainedFileCountLimit = 5;
            });

            services.Configure<AzureBlobLoggerOptions>(options => {
                options.BlobName = "log.txt";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("localhost") || env.IsEnvironment("QA"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VA.TMP.Integration.Api.TelehealthSpecialtyLocation v1"));
                app.UseSwagger(s =>
                {
                    s.SerializeAsV2 = true;
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.MapWhen((ctx) =>
                !ctx.Request.Path.StartsWithSegments("api/ClinicLocations"), useApp => useApp.UseRouting()
            );
        }
    }
}
