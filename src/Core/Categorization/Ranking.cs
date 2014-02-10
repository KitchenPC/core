namespace KitchenPC.Categorization
{
   internal class Ranking
   {
      public float Score { get; set; }
      public Category Type { get; private set; }

      public Ranking(Category type)
      {
         this.Type = type;
         this.Score = 0f;
      }
   }
}