using System;
using System.Collections;
using KitchenPC.Core;
using KitchenPC.Core.Modeler;
using NHibernate.Transform;

namespace KitchenPC.DB.NLP;

public class IngredientGraphTransformer : IResultTransformer
{
   public static IngredientGraphTransformer Create() => new();

   private IngredientGraphTransformer() { }

   public IList TransformList(IList collection) => collection;

   public object TransformTuple(object[] tuple, string[] aliases) =>
      IngredientBinding.Create(
         (Guid)tuple[0], //R.IngredientId
         (Guid)tuple[1], //R.RecipeId
         (Single?)tuple[2], //R.Qty
         (Units)tuple[3], //R.Unit
         (UnitType)tuple[4], //I.ConversionType
         (int)tuple[5], //I.UnitWeight
         tuple[6] is Units formUnit ? formUnit : null, //F.UnitType
         tuple[7] is float formAmount ? formAmount : null, //F.FormAmount
         tuple[8] is Units formAmountUnit ? formAmountUnit : null, //F.FormUnit
         (String)tuple[9] //I.DisplayName
      );
}
