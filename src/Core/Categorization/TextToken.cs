namespace KitchenPC.Categorization
{
   internal class TextToken : IToken
   {
      readonly string text;

      public TextToken(string text)
      {
         this.text = text.Trim().ToLower();
      }

      public override bool Equals(object obj)
      {
         var t1 = obj as TextToken;
         return (t1 != null && t1.text.Equals(text));
      }

      public override int GetHashCode()
      {
         return text.GetHashCode();
      }

      public override string ToString()
      {
         return text;
      }
   }
}