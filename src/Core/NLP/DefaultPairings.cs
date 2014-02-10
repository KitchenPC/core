using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public struct DefaultPairings
   {
      static readonly DefaultPairings empty = new DefaultPairings();

      public static DefaultPairings Empty
      {
         get
         {
            return empty;
         }
      }

      public IngredientForm Unit;
      public IngredientForm Volume;
      public IngredientForm Weight;

      public bool IsEmpty
      {
         get
         {
            return (Unit == null && Volume == null && Weight == null);
         }
      }

      public bool HasUnit
      {
         get
         {
            return Unit != null;
         }
      }

      public bool HasVolume
      {
         get
         {
            return Volume != null;
         }
      }

      public bool HasWeight
      {
         get
         {
            return Weight != null;
         }
      }
   }
}