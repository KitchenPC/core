using System;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC
{
   public class KPCException : Exception
   {
      public KPCException()
      {
      }

      public KPCException(string message) : base(message)
      {
      }
   }

   public class InvalidConfigurationException : KPCException
   {
      public InvalidConfigurationException()
      {
      }

      public InvalidConfigurationException(string message) : base(message)
      {
      }
   }

   public class DataStoreException : KPCException
   {
      public DataStoreException(string message) : base(message)
      {
      }
   }

   public class IngredientMapNotInitializedException : KPCException
   {
   }

   public class IngredientMapInvalidIngredientException : KPCException
   {
   }

   public class RecipeNotFoundException : KPCException
   {
   }

   public class IngredientNotFoundException : KPCException
   {
   }

   public class MenuNotFoundException : KPCException
   {
      public MenuNotFoundException()
      {
      }

      public MenuNotFoundException(Guid menuId)
      {
         this.menuId = menuId;
      }

      public Guid? menuId { get; private set; }
   }

   public class MenuIdRequiredException : KPCException
   {
   }

   public class ShoppingListNotFoundException : KPCException
   {
   }

   public class InvalidRecipeDataException : KPCException
   {
      public InvalidRecipeDataException(string err) : base(err)
      {
      }
   }

   public class MenuAlreadyExistsException : KPCException
   {
   }

   public class UserDoesNotOwnMenuException : KPCException
   {
   }

   public class IncompatibleAmountException : KPCException
   {
   }

   public class FluentExpressionException : KPCException
   {
      public FluentExpressionException(string msg) : base(msg)
      {
      }
   }

   public class CouldNotParseUsageException : KPCException
   {
      public CouldNotParseUsageException(Result result, string usage)
      {
         Result = result;
         Usage = usage;
      }

      public Result Result { get; private set; }
      public string Usage { get; private set; }
   }

   public class InvalidFormException : KPCException
   {
      public InvalidFormException(Ingredient ing, IngredientForm form)
      {
         Ingredient = ing;
         Form = form;
      }

      public Ingredient Ingredient { get; private set; }
      public IngredientForm Form { get; private set; }
   }

   public class NoConfiguredSearchProvidersException : KPCException
   {
   }

   public class EmptyPantryException : Exception
   {
   }

   public class ImpossibleQueryException : Exception
   {
   }

   public class DuplicatePantryItemException : Exception
   {
   }

   public class DataLoadException : Exception
   {
      public DataLoadException(string message) : base(message)
      {
      }

      public DataLoadException(Exception inner) : base(inner.Message, inner)
      {
      }
   }

   public class UnknownTokenException : Exception
   {
      public string token { get; private set; }

      public UnknownTokenException(string token)
      {
         this.token = token;
      }
   }
}