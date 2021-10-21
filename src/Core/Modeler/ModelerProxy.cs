using System;
using KitchenPC.Core.Context;

namespace KitchenPC.Core.Modeler;

public class ModelerProxy
{
   private DBSnapshot db;
   private readonly IKPCContext context;

   public ModelerProxy(IKPCContext context)
   {
      this.context = context;
   }

   public void LoadSnapshot()
   {
      db = new DBSnapshot(context);
   }

   public ModelingSession CreateSession(IUserProfile profile)
   {
      if (db == null)
         throw new Exception("ModelerProxy has not been initialized.");

      return new ModelingSession(context, db, profile);
   }

   public RecipeNode FindRecipe(Guid id)
   {
      if (db == null)
         throw new Exception("ModelerProxy has not been initialized.");

      return db.FindRecipe(id);
   }

   public IngredientNode FindIngredient(Guid id)
   {
      if (db == null)
         throw new Exception("ModelerProxy has not been initialized.");

      return db.FindIngredient(id);
   }
}
