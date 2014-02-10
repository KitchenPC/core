using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Menus;
using KitchenPC.Recipes;

namespace KitchenPC.Context.Fluent
{
   /// <summary>Provides the ability to fluently express menu related actions, such as creating, updating, or removing menus.</summary>
   public class MenuAction
   {
      readonly IKPCContext context;

      public MenuAction(IKPCContext context)
      {
         this.context = context;
      }

      public MenuLoader Load(Menu menu)
      {
         return new MenuLoader(context, menu);
      }

      public MenuLoader LoadAll
      {
         get
         {
            return new MenuLoader(context);
         }
      }

      public MenuCreator Create
      {
         get
         {
            return new MenuCreator(context);
         }
      }

      public MenuUpdater Update(Menu menu)
      {
         return new MenuUpdater(context, menu);
      }

      public MenuDeleter Delete(Menu menu)
      {
         return new MenuDeleter(context, menu);
      }
   }

   /// <summary>Represents one or more menus to be loaded.</summary>
   public class MenuLoader
   {
      readonly IKPCContext context;
      readonly IList<Menu> menusToLoad;
      readonly bool loadAll;
      bool loadRecipes;

      public MenuLoader WithRecipes
      {
         get
         {
            // BUGBUG: Technically, we should be creating a new instance and copying the menus over, as there might
            // be a reference to the old chain somewhere that we'd be updating.

            loadRecipes = true;
            return this;
         }
      }

      public MenuLoader(IKPCContext context)
      {
         this.context = context;
         this.loadAll = true;
      }

      public MenuLoader(IKPCContext context, Menu menu)
      {
         this.context = context;
         menusToLoad = new List<Menu>() {menu};
      }

      public MenuLoader Load(Menu menu)
      {
         if (loadAll)
            throw new FluentExpressionException("To specify individual menus to load, remove the LoadAll clause from your expression.");

         menusToLoad.Add(menu);
         return this;
      }

      public IList<Menu> List()
      {
         var options = new GetMenuOptions();
         options.LoadRecipes = loadRecipes;

         return context.GetMenus(menusToLoad, options);
      }
   }

   /// <summary>Represents a menu to be created.</summary>
   public class MenuCreator
   {
      readonly IKPCContext context;
      readonly IList<Recipe> recipes;
      string title;

      public MenuCreator(IKPCContext context)
      {
         this.context = context;
         this.recipes = new List<Recipe>();
         title = "New Menu";
      }

      public MenuCreator WithTitle(string name)
      {
         this.title = name;
         return this;
      }

      public MenuCreator AddRecipe(Recipe recipe)
      {
         this.recipes.Add(recipe);
         return this;
      }

      public MenuResult Commit()
      {
         var newMenu = new Menu(null, title);
         return context.CreateMenu(newMenu, recipes.Select(r => r.Id).ToArray());
      }
   }

   /// <summary>Represents a menu to be updated.</summary>
   public class MenuUpdater
   {
      readonly IKPCContext context;
      readonly Menu menu;
      readonly IList<Recipe> addQueue;
      readonly IList<Recipe> removeQueue;
      readonly IList<MenuMover> moveQueue;
      string newTitle;
      bool clearAll;

      public MenuUpdater(IKPCContext context, Menu menu)
      {
         this.context = context;
         this.menu = menu;
         this.addQueue = new List<Recipe>();
         this.removeQueue = new List<Recipe>();
         this.moveQueue = new List<MenuMover>();
      }

      public MenuUpdater Add(Recipe recipe)
      {
         if (!addQueue.Contains(recipe))
            addQueue.Add(recipe);

         return this;
      }

      public MenuUpdater Remove(Recipe recipe)
      {
         if (!removeQueue.Contains(recipe))
            removeQueue.Add(recipe);

         return this;
      }

      public MenuUpdater Rename(string newTitle)
      {
         this.newTitle = newTitle;
         return this;
      }

      public MenuUpdater Move(Func<MoveAction, MoveAction> moveAction)
      {
         var action = MenuMover.Create();
         var result = moveAction(action);

         moveQueue.Add(result.Mover);
         return this;
      }

      public MenuUpdater Move(MenuMover mover)
      {
         moveQueue.Add(mover);
         return this;
      }

      public MenuUpdater Clear
      {
         get
         {
            clearAll = true;
            return this;
         }
      }

      public MenuResult Commit()
      {
         return context.UpdateMenu(menu.Id,
            addQueue.Select(r => r.Id).ToArray(),
            removeQueue.Select(r => r.Id).ToArray(),
            moveQueue.Select(m => new MenuMove
            {
               MoveAll = m.All,
               RecipesToMove = m.Recipes.Select(r => r.Id).ToArray(),
               TargetMenu = m.TargetMenu.Id
            }).ToArray(),
            clearAll,
            newTitle
            );
      }
   }

   /// <summary>Represents one or more menus to be deleted.</summary>
   public class MenuDeleter
   {
      readonly IKPCContext context;
      readonly IList<Menu> menusToDelete;

      public MenuDeleter(IKPCContext context, Menu menu)
      {
         this.context = context;
         menusToDelete = new List<Menu>() {menu};
      }

      public MenuDeleter Delete(Menu menu)
      {
         if (!menu.Id.HasValue)
            throw new MenuIdRequiredException();

         menusToDelete.Add(menu);
         return this;
      }

      public void Commit()
      {
         context.DeleteMenus(menusToDelete.Select(m => m.Id.Value).ToArray());
      }
   }

   /// <summary>Represents one or more recipes to be moved from one menu to another.</summary>
   public class MenuMover
   {
      public IList<Recipe> Recipes { get; set; } // Recipes to move
      public Menu TargetMenu { get; set; } // Menu to move recipes to
      public bool All { get; set; } // Move all recipes in source menu to targetMenu

      public static MoveAction Create()
      {
         var mover = new MenuMover();
         return new MoveAction(mover);
      }

      public MenuMover()
      {
         Recipes = new List<Recipe>();
      }
   }

   /// <summary>Provides the ability to fluently express a MenuMover object.</summary>
   public class MoveAction
   {
      readonly MenuMover mover;

      public MoveAction(MenuMover mover)
      {
         this.mover = mover;
      }

      public MoveAction Recipe(Recipe recipe)
      {
         mover.Recipes.Add(recipe);
         return this;
      }

      public MoveAction To(Menu menu)
      {
         mover.TargetMenu = menu;
         return this;
      }

      public MoveAction AllRecipes
      {
         get
         {
            return new MoveAction(new MenuMover
            {
               All = true,
               TargetMenu = mover.TargetMenu
            });
         }
      }

      public MenuMover Mover
      {
         get
         {
            return mover;
         }
      }
   }
}