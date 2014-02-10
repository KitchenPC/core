using System;

namespace KitchenPC.Context
{
   public class Configuration<T> : IConfiguration<T> where T : IKPCContext
   {
      readonly ConfigurationBuilder<T> builder;

      public T Context { get; set; }

      public static ConfigurationBuilder<T> Build
      {
         get
         {
            return new Configuration<T>().builder;
         }
      }

      public static IConfiguration<T> Xml
      {
         get
         {
            // TODO: Read local XML configuration and return ConfigurationBuilder
            throw new NotImplementedException();
         }
      }

      Configuration()
      {
         builder = new ConfigurationBuilder<T>(this);
      }

      public T InitializeContext()
      {
         Context.Initialize();
         return Context;
      }
   }
}