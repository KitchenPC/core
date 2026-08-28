using System;

namespace KitchenPC.Core.Context;

public class DBContextBuilder : IConfigurationBuilder<DBContext>
{
   readonly DBContext context;

   public DBContextBuilder(DBContext context)
   {
      this.context = context;
   }

   public DBContextBuilder Adapter<T>(IConfigurationBuilder<T> adapter)
      where T : IDBAdapter
   {
      context.Adapter = adapter.Create();
      return this;
   }

   public DBContextBuilder Identity(Func<AuthIdentity> getIdentity)
   {
      context.GetIdentity = getIdentity;
      return this;
   }

   /// <summary>
   /// Selects the optional in-memory indexes initialized by this context. All capabilities are
   /// enabled by default for backward compatibility.
   /// </summary>
   public DBContextBuilder Capabilities(DBContextCapabilities capabilities)
   {
      if ((capabilities & ~DBContextCapabilities.All) != 0)
         throw new ArgumentOutOfRangeException(nameof(capabilities));

      context.Capabilities = capabilities;
      return this;
   }

   public DBContext Create()
   {
      return context;
   }
}
