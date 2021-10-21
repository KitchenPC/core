using System;
using System.Security.Claims;
using KitchenPC.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenPC.Core.Middleware;

public static class KPCMiddleware
{
    /// <summary>
    /// Adds KitchenPC OWIN Middleware components into Service Collection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddKPCContext<T>(this IServiceCollection services, IConfiguration<T> configuration) where T : class, IKPCContext
    {
        var kpcContext = configuration.InitializeContext();
            
        services.AddHttpContextAccessor();
        services.AddScoped(ctx =>
        {
            IHttpContextAccessor contextAccessor = ctx.GetService<IHttpContextAccessor>();
                
            if (contextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                string id = contextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value;
                string alias = contextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            
                if (Guid.TryParse(id, out Guid guidId) && !string.IsNullOrWhiteSpace(alias))
                {
                    var identity = new AuthIdentity(guidId, alias);
                    return kpcContext.AsUserContext(identity) as T;
                }
            }
            
            return kpcContext;
        });
    }        
}