using System;

namespace KitchenPC.Core.Context;

public class Configuration<T> : IConfiguration<T>
   where T : IKPCContext
{
   private readonly ConfigurationBuilder<T> builder;

   public T Context { get; set; }

   public static ConfigurationBuilder<T> Build => new Configuration<T>().builder;

   // TODO: Read local XML configuration and return ConfigurationBuilder
   public static IConfiguration<T> Xml => throw new NotImplementedException();

   private Configuration()
   {
      builder = new ConfigurationBuilder<T>(this);
   }

   public T InitializeContext()
   {
      Context.Initialize();
      return Context;
   }
}
