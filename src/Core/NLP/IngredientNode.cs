using System;

namespace KitchenPC.NLP
{
   public class IngredientNode
   {
      readonly IngredientNode parent; //This node can shadow another node under another name
      readonly Guid id;
      readonly DefaultPairings pairings;
      readonly UnitType convtype;
      readonly Weight unitweight;

      public Guid Id
      {
         get
         {
            return (parent == null) ? id : parent.id;
         }
      }

      public DefaultPairings Pairings
      {
         get
         {
            return (parent == null) ? pairings : parent.pairings;
         }
      }

      public IngredientNode Parent
      {
         get
         {
            return parent;
         }
      }

      public string IngredientName; //Name of the ingredient or synonym
      public string PrepNote; //If this ingredient is an alias for another, it will use this as a prep note, eg: Ripe Bananas => Bananas (Ripe)

      public UnitType ConversionType
      {
         get
         {
            return (parent == null) ? convtype : parent.convtype;
         }
      } //Default conversion type for this ingredient (from ShoppingIngredients)

      public Weight UnitWeight
      {
         get
         {
            return (parent == null) ? unitweight : parent.unitweight;
         }
      } //How much a single unit weighs (from ShoppingIngredients)

      public IngredientNode(Guid id, string name, UnitType convtype, Weight unitweight, DefaultPairings pairings)
      {
         this.id = id;
         this.pairings = pairings;
         this.convtype = convtype;
         this.unitweight = unitweight;

         IngredientName = name;
      }

      public IngredientNode(IngredientNode root, string synonym, string prepnote)
      {
         this.parent = root;
         IngredientName = synonym;
         PrepNote = prepnote;
      }
   }
}