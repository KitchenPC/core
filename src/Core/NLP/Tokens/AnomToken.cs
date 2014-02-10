using System;
using System.IO;

namespace KitchenPC.NLP.Tokens
{
   public class AnomToken : IGrammar
   {
      static Anomalies data;

      public bool Read(Stream stream, MatchData matchData)
      {
         if (data == null)
         {
            data = new Anomalies();
         }

         var buffer = String.Empty;
         AnomalousNode foundNode = null;
         var fFound = false;
         var matchPos = stream.Position;
         int curByte;

         while ((curByte = stream.ReadByte()) >= 0)
         {
            buffer += (char) curByte;

            AnomalousNode node;
            var match = data.Parse(buffer, out node);
            if (match == MatchPrecision.None)
            {
               break; //No reason to continue reading stream, let's see what we have..
            }

            if (match == MatchPrecision.Exact)
            {
               matchPos = stream.Position;
               foundNode = node;
               fFound = true;
            }
         }

         if (foundNode != null) //Initialize match data with values from this anomaly
         {
            matchData.Ingredient = foundNode.Ingredient;
         }

         stream.Seek(matchPos, SeekOrigin.Begin);
         return fFound;
      }
   }
}