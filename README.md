What is KitchenPC?
====

KitchenPC is a free, open-source framework written in C# for working with recipes, shopping lists, and menus.  It provides a standardized data model for representing normalized ingredient and recipe information, and allows ingredient usage to be aggregated across recipes automatically.  The core KitchenPC framework includes:

1. Base classes for describing and working with core recipe-related concepts.
2. A **recipe modeling engine** capable of building sets of recipes that efficiently use a set of ingredients and amounts.
3. A **natural language parser** which can convert human input ("a dozen eggs") to a normalized ingredient usage structure (whole eggs: 12)
4. A **categorization engine** which can take recipe objects and catagorize them as breakfast, lunch, dinner or dessert.  This engine can also derive nutrional information based on USDA data, a taste profile (sweet, savory, spicy, mild) based on ingredients and amounts used, dietary flags (vegetarian, gluten-free, low-calorie, etc) and other aspects of the recipe.
5. An extensible framework to define how data is loaded and saved to a persistence mechanism, such as a SQL database or full-text search engine.

How To Get Started
====

Getting started is simple, and data can be loaded locally from an XML file for testing.  Sample data is included with the source, which includes a few dozen recipes, along with sample menus and shopping lists.

The best way to get up and running is to read the blog post titled [Getting Started with KitchenPC](http://blog.kitchenpc.com/2014/02/10/getting-started-with-kitchenpc/) which includes an introduction to core concepts as well as several samples.

1. [Getting Started](http://blog.kitchenpc.com/2014/02/10/getting-started-with-kitchenpc/)
2. [Provisioning a Database](http://blog.kitchenpc.com/2014/02/11/kitchenpc-database-provisioning-101/)
3. [Logging](http://blog.kitchenpc.com/2014/02/13/kitchenpc-logging-101/)
4. [Creating a Recipe](http://blog.kitchenpc.com/2014/02/14/lets-make-a-recipe/)
