# Recipe Model

In the model, note that question-marks (?) represent nice-to-haves.

## What do Restaurants need?

- (Display)Name
-  Ingredient list
   - Ingredient
     - Name
     - Supplier
     - Purchase Price 
     - Purchase Quantity
     - Cost (calculated)
   - Quantity
     - Amount
     - Unit
- Yield
- Requirements (Equipment, Tools, etc.)
- Cost (transient; calculated from ingredient list vs inventory records)
- Sale Price
- Directions/Procedures
- SKU
- ?History? -- ooh this could be a rad feature
- References
    - URNReference -OR- string

Now if I hit a /recipes endpoint, what do I generally care about?

If I'm showing a **_grid of recipes_**, I care about:
- DisplayName
- SKU
- Yield
- Sale Price
- Cost
- References 
(basically, not ingredients, directions, requirements, or history)

And I'd want an endpoint for populating a dropdown with a LIST of ingredients. Like a list of URNReferences.
- DisplayName
- URN

And maybe an endpoint for URNLinks:
- DisplayName
- URN (ID)
- URL (hyperlink)

If I'm showing a **_recipe editor_**, then I care about (probably all the things):
- Name
- SKU
- Cost
- Sale Price
- Ingredients

If I'm showing an **_ingredients grid_**, then I care about:
- Name
- Internal SKU
- Supplier
- Supplier SKU
- (most recent) Purchase Price
- (most recent) Purchase Amount
- (most recent) Cost
- Some kind of external reference list

Definitely want to allow multiple ingredients by the same name, from different suppliers.  Probably want to allow multiple ingredients of the same name from the same supplier.  How do we differentiate them?
Probably add a supplier-supplied ID (Supplier Item Number / Part Number / SKU)
