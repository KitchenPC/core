using log4net;

namespace KitchenPC.Context
{
   public static class KPCContext
   {
      public static IKPCContext Current { get; private set; }
      public static ILog Log = LogManager.GetLogger(typeof (KPCContext));

      /// <summary>
      /// Initializes a given KitchenPC context.  After calling this method, KPCContext.Current will refer to that context.
      /// </summary>
      /// <param name="context">An instance of a KitchenPC context.</param>
      public static void Initialize(IKPCContext context)
      {
         Log.InfoFormat("Initializing global KitchenPC Context of type {0}", context.GetType().Name);

         context.Initialize();
         Current = context;
      }

      /// <summary>
      /// Initializes a given KitchenPC context.  After calling this method, KPCContext.Current will refer to that context.
      /// </summary>
      /// <typeparam name="T">A type of KitchenPC context to initialize.</typeparam>
      /// <param name="configuration">A configuration able to build a context of the specified type.  A configuration builder can be obtained using Configuration&lt;T&gt;.Build</param>
      public static void Initialize<T>(IConfiguration<T> configuration) where T : IKPCContext
      {
         Initialize(configuration.Context);
      }
   }
}