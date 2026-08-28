using System;
using System.Security.Claims;
using KitchenPC.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenPC.Core.Middleware;

public static class KPCMiddleware
{
   /// <summary>Adds a configured KitchenPC context to an ASP.NET Core service collection.</summary>
   public static void AddKPCContext<T>(
      this IServiceCollection services,
      IConfiguration<T> configuration
   )
      where T : class, IKPCContext
   {
      ArgumentNullException.ThrowIfNull(services);
      ArgumentNullException.ThrowIfNull(configuration);

      var kpcContext = configuration.InitializeContext();

      services.AddHttpContextAccessor();
      services.AddScoped(ctx =>
      {
         var contextAccessor = ctx.GetService<IHttpContextAccessor>();

         if (contextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
         {
            var id = contextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value;
            var alias = contextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            if (Guid.TryParse(id, out var guidId) && !string.IsNullOrWhiteSpace(alias))
            {
               return kpcContext.AsUserContext(new AuthIdentity(guidId, alias)) as T;
            }
         }

         return kpcContext;
      });
   }
}
