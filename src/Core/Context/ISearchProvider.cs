using KitchenPC.Recipes;

namespace KitchenPC.Context
{
   /// <summary>
   /// Provides a KitchenPC database adapter with a way to search for recipes based on a query.
   /// </summary>
   public interface ISearchProvider
   {
      SearchResults Search(AuthIdentity identity, RecipeQuery query);
   }
}