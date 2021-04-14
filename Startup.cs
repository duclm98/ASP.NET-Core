using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Data;
using TodoAPI.Methods;
using TodoAPI.Middleware;

namespace TodoAPI
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoAPI", Version = "v1" });
            });
            services.AddDbContext<DataContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("MSSQLConnection")),ServiceLifetime.Scoped);
            services.AddScoped<IAuthMethod, AuthMethod>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoAPI v1"));
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseExceptionHandler();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            // app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            // app.UseAuthentication();
            // app.UseAuthorization();
            // app.UseSession();
            // app.UseResponseCaching();
            // app.UseResponseCompression();

            // app.UseMiddleware<AuthMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => {
                  await context.Response.WriteAsync("Hello world!");
                });
                endpoints.MapControllers();
            });
        }
    }
}