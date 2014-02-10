using System;
using System.Diagnostics;
using KitchenPC.Context;
using KPCServer.UnitTests;
using NUnit.Framework;

namespace KitchenPC.UnitTests
{
   [TestFixture]
   internal class ModelerTests
   {
      public static Guid ING_EGGS = new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76");
      public static Guid ING_MILK = new Guid("5a698842-54a9-4ed2-b6c3-aea1bcd157cd");
      public static Guid ING_FLOUR = new Guid("daa8fbf6-3347-41b9-826d-078cd321402e");
      public static Guid ING_CHEESE = new Guid("5ee315ea-7ef6-4fa5-809a-dc9931a01ed1");
      public static Guid ING_CHICKEN = new Guid("55344339-8d1d-4892-a117-ec8018a5e483");

      public static Guid ING_CHINESECHESTNUTS = new Guid("b124f851-5a10-4432-9455-00d3471ab802");
      public static Guid ING_GREENTURTLE = new Guid("d81e16da-2de9-4184-92f5-066e2fab0b71");

      IKPCContext _context;

      [TestFixtureSetUp]
      public void Setup()
      {
         Trace.Write("Creating DB Snapshot from XML file... ");
         _context = new MockContext();
         _context.Initialize();
         Trace.WriteLine("Done!");
      }

      [Test]
      public void TestNoRatingModeler()
      {
         Trace.WriteLine("Running NoRating Test.");
         var profile = new MockNoRatingsUserProfile();
         var session = _context.CreateModelingSession(profile);

         var set = session.Generate(5, 3); //Test for balanced model
         Assert.AreEqual(5, set.RecipeIds.Length);
      }

      [Test]
      public void TestImpossibleFilterModeler()
      {
         Trace.WriteLine("Running ImpossibleFilter Test.");
         var profile = new MockImpossibleFilterUserProfile(); // Only No Pork recipes are allowed, of which there are none in our mock data
         Assert.Catch(typeof (ImpossibleQueryException),
            delegate
            {
               var session = _context.CreateModelingSession(profile);
               var set = session.Generate(5, 1);
            }
            );
      }

      [Test]
      public void TestImpossiblePantryModeler()
      {
         Trace.WriteLine("Running ImpossiblePantry Test.");
         var profile = new MockImpossiblePantryUserProfile();
         Assert.Catch(typeof (ImpossibleQueryException),
            delegate
            {
               var session = _context.CreateModelingSession(profile);
               session.Generate(5, 1);
            }
            );
      }

      [Test]
      public void TestNormalModeler()
      {
         var profile = new MockNormalUserProfile();
         var session = _context.CreateModelingSession(profile);

         Trace.WriteLine("Running NormalModel Test (Efficient)");
         var efficientSet = session.Generate(5, 1); //Test for most efficient set
         Assert.AreEqual(5, efficientSet.RecipeIds.Length);

         Trace.WriteLine("Running NormalModel Test (Balanced)");
         var balancedSet = session.Generate(5, 3); //Test for balanced model
         Assert.AreEqual(5, balancedSet.RecipeIds.Length);

         Trace.WriteLine("Running NormalModel Test (Recommended)");
         var ratedSet = session.Generate(5, 5); //Test for recipes user most likely to rate highly (basically get suggestions, ignore pantry)
         Assert.AreEqual(5, ratedSet.RecipeIds.Length);

         // - Test profile with specific AllowedTags, and AllowedTags == null
      }
   }
}