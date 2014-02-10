using System.IO;
using System.Text;

namespace KitchenPC.NLP.Tokens
{
   public class StaticToken : IGrammar
   {
      readonly string phrase;

      public StaticToken(string phrase)
      {
         this.phrase = phrase.TrimStart(' ');
      }

      /// <summary>
      /// Reads stream to match it against a fixed string ignoring any whitespace
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="matchdata"></param>
      /// <returns></returns>
      public bool Read(Stream stream, MatchData matchdata)
      {
         while (stream.ReadByte() == ' ')
         {
         } //Burn off any leading spaces, they should not affect the grammar
         stream.Seek(-1, SeekOrigin.Current); //Set stream to first character after any whitespace (kinda a hack, maybe a better way to write this)

         //Read the stream to make sure it matches the complete token, return false if not
         var count = this.phrase.Length;
         var readBytes = new byte[count];
         stream.Read(readBytes, 0, count);

         return (Encoding.Default.GetString(readBytes) == this.phrase);
      }
   }
}