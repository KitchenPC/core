using System;

namespace KitchenPC.Context
{
   public class DBContextBuilder : IConfigurationBuilder<DBContext>
   {
      readonly DBContext context;

      public DBContextBuilder(DBContext context)
      {
         this.context = context;
      }

      public DBContextBuilder Adapter<T>(IConfigurationBuilder<T> adapter) where T : IDBAdapter
      {
         context.Adapter = adapter.Create();
         return this;
      }

      public DBContextBuilder Identity(Func<AuthIdentity> getIdentity)
      {
         context.GetIdentity = getIdentity;
         return this;
      }

      public DBContext Create()
      {
         return context;
      }
   }
}