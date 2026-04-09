using System;
using System.Collections.Generic;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using KitchenPC.Core.Context;

namespace KitchenPC.DB;

public class DatabaseAdapterBuilder : IConfigurationBuilder<DatabaseAdapter>
{
   private readonly DatabaseAdapter adapter;

   public DatabaseAdapterBuilder(DatabaseAdapter adapter)
   {
      this.adapter = adapter;
   }

   public DatabaseAdapterBuilder DatabaseConfiguration(IPersistenceConfigurer config)
   {
      adapter.DatabaseConfiguration = config;
      return this;
   }

   public DatabaseAdapterBuilder AddConvention(IConvention convention)
   {
      adapter.DatabaseConventions ??= new List<IConvention>();

      adapter.DatabaseConventions.Add(convention);

      return this;
   }

   public DatabaseAdapterBuilder SearchProvider<T>(Func<DatabaseAdapter, T> createProvider)
      where T : ISearchProvider
   {
      adapter.SearchProvider = createProvider(adapter);
      return this;
   }

   public DatabaseAdapter Create() => adapter;
}
