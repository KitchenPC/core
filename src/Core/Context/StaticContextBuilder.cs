using System;

namespace KitchenPC.Context
{
   public class StaticContextBuilder : IConfigurationBuilder<StaticContext>
   {
      readonly StaticContext context;

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

      public StaticContext Create()
      {
         return context;
      }
   }
}