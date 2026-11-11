using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models;

/// <summary>A read-only view of the owner's public identity; account management belongs to the host.</summary>
public class RecipeOwner
{
   public virtual Guid UserId { get; set; }
   public virtual string Alias { get; set; }
}

public class RecipeOwnerMap : ClassMap<RecipeOwner>
{
   public RecipeOwnerMap()
   {
      Table("users");
      ReadOnly();
      Id(x => x.UserId).GeneratedBy.Assigned();
      Map(x => x.Alias).Length(50);
   }
}
