namespace KitchenPC.Context
{
   /// <summary>Fluent interface to create configuration objects</summary>
   public class ConfigurationBuilder<T> : IConfigurationBuilder<IConfiguration<T>> where T : IKPCContext
   {
      readonly IConfiguration<T> configuration;

      public ConfigurationBuilder(IConfiguration<T> config)
      {
         configuration = config;
      }

      public ConfigurationBuilder<T> Context(IConfigurationBuilder<T> context)
      {
         configuration.Context = context.Create();
         return this;
      }

      public IConfiguration<T> Create()
      {
         return configuration;
      }
   }
}