using KitchenPC.Core.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class IngredientController : ControllerBase
{
   private readonly DBContext kpcContext;
   private readonly ILogger<IngredientController> logger;

   public IngredientController(ILogger<IngredientController> logger, DBContext kpcContext)
   {
      this.logger = logger;
      this.kpcContext = kpcContext;
   }

   [HttpGet]
   public IActionResult ParseIngredient(string ing)
   {
      logger.LogInformation($"Parsing ingredient {ing}");

      var ingredient = kpcContext.ParseIngredient(ing?.Trim());
      return ingredient == null
         ? NotFound()
         : Ok(
            new
            {
               ingredient.Id,
               ingredient.Name,
               ingredient.ConversionType,
               ingredient.UnitName,
               ingredient.UnitWeight,
            }
         );
   }
}
