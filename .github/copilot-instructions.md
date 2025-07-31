# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

This mod enhances map generation in RimWorld by introducing advanced blueprint generation functionality. It aims to provide players with varied and immersive village and ruin structures that enrich the gameplay experience. By utilizing custom map generation steps, the mod scatters detailed blueprints across the map, ensuring unique encounters every time a new game is started.

## Key Features and Systems

1. **Blueprint Generation:**
   - Generate single blueprints or entire villages (as seen in `GenStep_CreateBlueprintSingle` and `GenStep_CreateBlueprintVillage`).
   - Ability to create ruins with customizable elements (featured in `GenStep_CreateRuinVillage`).

2. **Terrain and Building Definitions:**
   - Dynamic terrain and building selection to match the blueprint data.
   - Use of `TryGetTerrainDefFromFloorData`, `TryGetThingDefFromBuildingData`, and similar methods to fetch game definitions.

3. **Cell Manipulation:**
   - Methods like `ClearCell`, `CheckCell`, and `TrySetCellAs` efficiently handle map cells during generation.

4. **Randomization:**
   - Randomly pick wall and tile materials to diversify the structures (`RandomWallStuff` and `CorrespondingTileDef`).

5. **Unfogging Areas:**
   - The `RectTrigger_UnfogArea` class allows for dynamically generated areas to be revealed to the player, increasing engagement.

## Coding Patterns and Conventions

- **Class Design:**
  - Each feature or functionality is encapsulated within its class following class naming conventions like `GenStep_`.

- **Methods Naming:**
  - Preference for `Try` prefixing methods that return definitions, indicating potential failure (e.g., `TryGetThingDefFromItemData`).

- **Modular Methods:**
  - Emphasis on breaking down tasks into smaller methods, allowing for easier maintenance and logic reuse.

- **Scanning and Validation:**
  - Use of helper methods to verify condition prerequisites, improving reliability (`CheckCell`, `IsPositionValidForBlueprint`).

## XML Integration

This mod heavily depends on XML for asset definition:

- Blueprint data is parsed and cleaned using methods such as `CleanUpBlueprintData`.
- XML is crucial for defining structure templates and item placement, interfacing directly with the code via `MapGeneratorBlueprintDef`.

## Harmony Patching

- Ensure compatibility and dynamic modifications across game mechanics. 
- Patches can be strategically applied to replace native game methods, integrating deeply with RimWorld's core systems.
- Ideal for injecting custom logic where built-in functions are inadequate.

## Suggestions for GitHub Copilot

1. **Logic Extension:**
   - Suggest snippets for additional custom GenSteps if needed, based on existing patterns in `GenStep_CreateBlueprintSingle` and others.

2. **Error Handling:**
   - Provide suggestions for robust error checking around blueprint data parsing and cell positioning.

3. **Enhanced Customization:**
   - Recommend potential options for further randomization parameters (e.g., advanced terrain types) within blueprint generation.

4. **Performance Optimizations:**
   - Propose optimizations in repetitive methods cycles, leveraging LINQ for processing collections when appropriate.

5. **Documentation:**
   - Insert comments and documentation suggestions throughout to maintain clarity on method purposes and usage examples.

By following this comprehensive instruction guide, GitHub Copilot can significantly enhance mod development productivity, ensuring that the generated suggestions align well with the project's goals and coding practices.
