using System;

namespace KitchenPC.Core.Recipes;

public class RecipeBrief
{
    private Uri recipeimg;

    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public String Permalink { get; set; }
    public String Title { get; set; }
    public String Description { get; set; }
    public String Author { get; set; }
    public short? PrepTime { get; set; }
    public short? CookTime { get; set; }
    public short AvgRating { get; set; } = 0;

    public String ImageUrl
    {
        get
        {
            return (recipeimg == null ? "/images/img_placeholder.png" : recipeimg.ToString());
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