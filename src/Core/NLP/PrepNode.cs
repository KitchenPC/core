namespace KitchenPC.NLP
{
   public class PrepNode
   {
      public string Prep;

      public PrepNode(string prep)
      {
         Prep = prep.ToLower().Trim();
      }

      public static implicit operator PrepNode(string p)
      {
         return new PrepNode(p);
      }

      public static implicit operator string(PrepNode p)
      {
         return p.Prep;
      }

      public override string ToString()
      {
         return Prep;
      }
   }
}