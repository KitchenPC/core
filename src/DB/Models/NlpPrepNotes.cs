using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpPrepNotes
   {
      public virtual string Name { get; set; }
   }

   public class NlpPrepNotesMap : ClassMap<NlpPrepNotes>
   {
      public NlpPrepNotesMap()
      {
         Id(x => x.Name).Length(50);
      }
   }
}