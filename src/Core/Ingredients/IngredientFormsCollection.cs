using System.Collections.Generic;

namespace KitchenPC.Ingredients
{
   public class IngredientFormsCollection
   {
      readonly List<IngredientForm> _forms;

      public IngredientForm[] Forms
      {
         get
         {
            return _forms.ToArray();
         }

         set
         {
            _forms.Clear();
            foreach (var form in value)
               _forms.Add(form);
         }
      }

      public IngredientFormsCollection()
      {
         _forms = new List<IngredientForm>();
      }

      public IngredientFormsCollection(IEnumerable<IngredientForm> forms)
      {
         _forms = new List<IngredientForm>(forms);
      }

      public void AddForm(IngredientForm form)
      {
         _forms.Add(form);
      }
   }
}