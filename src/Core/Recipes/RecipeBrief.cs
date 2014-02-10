using System;

namespace KitchenPC.Recipes
{
   public class RecipeBrief
   {
      Uri recipeimg;

      public Guid Id;
      public Guid OwnerId;
      public String Permalink;
      public String Title;
      public String Description;
      public String Author;
      public short? PrepTime;
      public short? CookTime;
      public short AvgRating = 0;

      public String ImageUrl
      {
         get
         {
            return (recipeimg == null ? "/Images/img_placeholder.png" : recipeimg.ToString());
         }

         set
         {
            if (String.IsNullOrEmpty(value))
            {
               recipeimg = null;
               return;
            }

            //UriBuilder builder = new UriBuilder(baseUri);
            var builder = new UriBuilder();
            builder.Path = "Thumb_" + value;
            recipeimg = builder.Uri;
         }
      }

      public RecipeBrief()
      {
      }

      public RecipeBrief(Recipe r)
      {
         this.Id = r.Id;
         this.OwnerId = r.OwnerId;
         this.Title = r.Title;
         this.Description = r.Description;
         this.ImageUrl = r.ImageUrl;
         this.Author = r.OwnerAlias;
         this.PrepTime = r.PrepTime;
         this.CookTime = r.CookTime;
         this.AvgRating = r.AvgRating;
      }

      public override string ToString()
      {
         return String.Format("{0} ({1})", Title, Id);
      }
   }
}