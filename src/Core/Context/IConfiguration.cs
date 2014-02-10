namespace KitchenPC.Context
{
   /// <summary>
   /// Implements an object which holds the configuration for a certain type of context.
   /// </summary>
   /// <typeparam name="T">A type which implements IKPCContext</typeparam>
   public interface IConfiguration<T> where T : IKPCContext
   {
      T Context { get; set; }

      T InitializeContext();
   }
}