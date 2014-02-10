namespace KitchenPC.Data
{
   /// <summary>Represents a source in which a KitchenPC Context can be provisioned from.</summary>
   public interface IProvisionSource
   {
      DataStore Export();
   }
}