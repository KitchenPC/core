using System;
using Microsoft.Extensions.Logging;

namespace KitchenPC.Core.Context;

public class StaticContextBuilder : IConfigurationBuilder<StaticContext>
{
   private readonly StaticContext context;

   public StaticContextBuilder(StaticContext context)
   {
      this.context = context;
   }

   /// <summary>A path on the file system that contains a KitchenPC data file.</summary>
   public StaticContextBuilder DataDirectory(string path)
   {
      context.DataDirectory = path;
      return this;
   }

   /// <summary>Configures context to compress the store file on disk to save space.</summary>
   public StaticContextBuilder CompressedStore
   {
      get
      {
         context.CompressedStore = true;
         return this;
      }
   }

   public StaticContextBuilder Identity(Func<AuthIdentity> getIdentity)
   {
      context.GetIdentity = getIdentity;
      return this;
   }

   public StaticContextBuilder Logging(ILoggerFactory loggerFactory)
   {
      context.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
      return this;
   }

   public StaticContext Create() => context;
}
