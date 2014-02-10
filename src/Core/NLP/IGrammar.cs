using System.IO;

namespace KitchenPC.NLP
{
   public interface IGrammar
   {
      /// <summary>
      /// Reads stream of ingredient usage being parsed and modifies IngredientUsage as fit
      /// </summary>
      /// <param name="stream">Stream containing input</param>
      /// <param name="usage">Usage to modify when data becomes known</param>
      /// <returns></returns>
      bool Read(Stream stream, MatchData usage);
   }
}