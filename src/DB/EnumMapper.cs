using System.Data;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace KitchenPC.DB
{
   public class EnumMapper<T> : EnumStringType<T>
   {
      public override SqlType SqlType
      {
         get
         {
            return new SqlType(DbType.Object);
         }
      }

      public static IPropertyConvention Convention
      {
         get
         {
            return ConventionBuilder.Property.When(
               c => c.Expect(x => x.Type == typeof (GenericEnumMapper<T>)),
               x =>
               {
                  x.CustomType<EnumMapper<T>>();
                  x.CustomSqlType((typeof (T).Name));
               });
         }
      }
   }
}