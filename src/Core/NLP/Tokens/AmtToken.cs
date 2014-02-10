using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KitchenPC.NLP.Tokens
{
   public class AmtToken : IGrammar
   {
      /// <summary>
      /// Reads stream to match it against an amount, number or indefinite article.  Sets any parsed value to usage.Amount
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="matchdata"></param>
      /// <returns></returns>
      public bool Read(Stream stream, MatchData matchdata)
      {
         //We probably have to detect a potential numeric match up front and then parsewith Validate, otherwise scan till first space and check if it's a number (do we support numbers with spaces?)
         var matchPos = stream.Position;
         var origPos = matchPos;
         int curByte;

         matchdata.Amount = null;
         var curBuffer = String.Empty;
         var invalidNumeric = new Regex(@"[^0-9 /.-]");

         while ((curByte = stream.ReadByte()) >= 0)
         {
            float parsedAmtHigh;
            float? parsedAmtLow;

            curBuffer += (char) curByte;
            if (Validate(curBuffer, out parsedAmtLow, out parsedAmtHigh)) //Try to parse buffer as a valid number or fraction
            {
               matchPos = stream.Position;
               matchdata.Amount = new Amount();
               matchdata.Amount.SizeLow = parsedAmtLow;
               matchdata.Amount.SizeHigh = parsedAmtHigh;
            }
            else //Stop processing as soon as we hit something that can no longer be an amount
            {
               if (invalidNumeric.IsMatch(curBuffer)) //Buffer has something in it other than 0-9 /.
               {
                  //Hold on, let's try parsing this buffer using the NumericToken parser
                  stream.Seek(origPos, SeekOrigin.Begin);
                  var numericParser = new NumericToken();
                  if (numericParser.Read(stream, matchdata))
                  {
                     matchPos = stream.Position;
                  }

                  break;
               }
            }
         }

         stream.Seek(matchPos, SeekOrigin.Begin);
         return (matchdata.Amount != null); //We found a valid amount here
      }

      static bool Validate(string buffer, out float? amountLow, out float amountHigh)
      {
         decimal resultHigh;
         amountLow = null;

         var parts = buffer.Split('-'); //Look for potential range
         if (parts.Length == 1)
         {
            if (Fractions.TryParseFraction(buffer, out resultHigh))
            {
               amountHigh = (float) resultHigh;
               return true;
            }
         }

         if (parts.Length == 2)
         {
            decimal resultLow;

            if (Fractions.TryParseFraction(parts[0], out resultLow) &&
                Fractions.TryParseFraction(parts[1], out resultHigh))
            {
               amountLow = (float?) resultLow;
               amountHigh = (float) resultHigh;
               return true;
            }
         }

         amountHigh = 0;
         return false;
      }
   }
}