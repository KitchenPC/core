using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using KitchenPC.Recipes;

namespace KitchenPC.DB.Models
{
   public class Recipes
   {
      public virtual Guid RecipeId { get; set; }
      public virtual short? CookTime { get; set; }
      public virtual string Steps { get; set; }
      public virtual short? PrepTime { get; set; }
      public virtual short Rating { get; set; }
      public virtual string Description { get; set; }
      public virtual string Title { get; set; }
      public virtual bool Hidden { get; set; }
      public virtual string Credit { get; set; }
      public virtual string CreditUrl { get; set; }
      public virtual DateTime DateEntered { get; set; }
      public virtual short ServingSize { get; set; }
      public virtual string ImageUrl { get; set; }

      public virtual IList<RecipeIngredients> Ingredients { get; set; }
      public virtual RecipeMetadata RecipeMetadata { get; set; }

      public virtual RecipeBrief AsRecipeBrief()
      {
         return new RecipeBrief
         {
            Id = RecipeId,
            ImageUrl = ImageUrl,
            AvgRating = Rating,
            CookTime = CookTime,
            PrepTime = PrepTime,
            Description = Description,
            Title = Title
         };
      }

      public static Recipes FromId(Guid id)
      {
         return new Recipes
         {
            RecipeId = id
         };
      }
   }

   public class RecipesMap : ClassMap<Recipes>
   {
      public RecipesMap()
      {
         Id(x => x.RecipeId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.CookTime).Index("IDX_Recipes_Cooktime");
         Map(x => x.Steps).Length(10000);
         Map(x => x.PrepTime).Index("IDX_Recipes_Preptime");
         Map(x => x.Rating).Not.Nullable().Index("IDX_Recipes_Rating");
         Map(x => x.Description).Length(512);
         Map(x => x.Title).Not.Nullable().Length(100);
         Map(x => x.Hidden).Not.Nullable().Index("IDX_Recipes_Hidden");
         Map(x => x.Credit).Length(100);
         Map(x => x.CreditUrl).Length(1024);
         Map(x => x.DateEntered).Not.Nullable();
         Map(x => x.ServingSize).Not.Nullable().Check("ServingSize > 0");
         Map(x => x.ImageUrl).Length(100);

         HasMany(x => x.Ingredients).KeyColumn("RecipeId");
         HasOne(x => x.RecipeMetadata).PropertyRef(x => x.Recipe).Cascade.All();
      }
   }
}