namespace KitchenPC.Context
{
   /// <summary>
   /// Implements an object which can build an IConfiguration for a certain type of context.
   /// </summary>
   /// <typeparam name="T">A KitchenPC context type this builder will create a configuration for.</typeparam>
   public interface IConfigurationBuilder<out T>
   {
      T Create();
   }
}