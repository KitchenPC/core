using FluentNHibernate.Cfg.Db;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Middleware;
using KitchenPC.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace WebApp;

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
         c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApp", Version = "v1" });
      });

      services.AddKPCContext(
         Configuration<DBContext>
            .Build.Context(
               DBContext
                  .Configure.Adapter(
                     DatabaseAdapter
                        .Configure.DatabaseConfiguration(
                           PostgreSQLConfiguration
                              .PostgreSQL82.ConnectionString(
                                 Configuration.GetConnectionString("KPCContext")
                              )
                              .ShowSql()
                        )
                        .SearchProvider(NHSearch.Instance)
                  )
                  .Identity(() => AuthIdentity.Anonymous) // TODO: This should be the default without having to explicitly say so
            )
            .Create()
      );
   }

   // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
   {
      if (env.IsDevelopment())
      {
         app.UseDeveloperExceptionPage();
         app.UseSwagger();
         app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApp v1"));
      }

      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
         endpoints.MapControllers();
      });
   }
}
