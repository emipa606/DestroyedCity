# GitHub Copilot Instructions for Destroyed City (Continued) Mod

## Mod Overview and Purpose

Destroyed City (Continued) is an update to the RK Skullers mod, enhancing the RimWorld experience with more destroyed buildings, custom difficulty settings, and additional floor blocks. The mod leverages the latest features of RimWorld, including improved support for Asset Bundles, to deliver a richer and more varied gameplay experience.

## Key Features and Systems

- **Custom Difficulty**: Adds a custom difficulty that increases the raid population growth rate and lowers their health, making encounters more challenging.
- **Destroyed Buildings**: Introduces a variety of new destroyed buildings to the map generation process.
- **Floor Blocks**: Adds two new floor blocks, "Road" and "Sidewalk," with specific properties like beauty ratings and construction costs.
- **Increased Ore and Plant Vegetation**: Enhances the natural resources available on the map.
- **Seasonal Easter Eggs**: Incorporates seasonal elements into the map generation.
- **Custom Map Generation**: Includes new ruins, roads, and other objects that add to the overall map complexity and aesthetics.

## Coding Patterns and Conventions

This mod follows a structured coding pattern, with a focus on modularity and maintainability. Key aspects include:

- **Modular Design**: The mod is divided into separate classes and files, each responsible for a specific aspect of map generation or difficulty settings.
- **Harmony Patching**: Extensively uses Harmony patching to modify vanilla RimWorld behavior without altering the core game files.
- **XML Integration**: Utilizes XML files to define new game elements such as terrain, buildings, and map generation steps.
- **Commenting and Documentation**: Provides detailed comments and documentation within the codebase to ensure clarity and ease of maintenance.

## XML Integration

XML files are crucial for defining new game elements in RimWorld. Key XML files in this mod include:

- **TerrainDefs**: Defines new terrain types, such as the "Road" and "Sidewalk" floor blocks.
- **ThingDefs**: Contains definitions for buildings and other in-game objects, including damage definitions and minified versions.
- **MapGeneratorDefs**: Specifies new map generation steps, such as creating blueprint villages and ruins.

## Harmony Patching

Harmony patching is employed to modify existing game functionality without changing the original codebase. Key patches in this mod include:

- **Raid Population Growth**: Adjusts the growth rate of raid populations to increase difficulty.
- **Building Placement**: Ensures buildings do not spawn into or clip mountains.
- **Mechanoid Combat Power**: Reduces the combat power of mechanoids to make them more fair.
- **Resource Spawn**: Tweaks the spawn rates of ore chunks and plant vegetation.

## Suggestions for Copilot

To make the most of GitHub Copilot when working on the Destroyed City (Continued) mod, consider the following suggestions:

- **Use Descriptive File and Class Names**: Copilot performs better when it can understand the context and purpose of the code being written. Descriptive names help Copilot generate more relevant suggestions.
- **Comment Clearly**: Provide clear comments around complex logic or patches to guide Copilot in generating accurate and contextually appropriate code.
- **Leverage XML for New Elements**: When defining new game elements, use XML files to keep the mod organized and maintainable. Copilot can help generate and update XML definitions.
- **Review and Test Code**: Always review and test generated code thoroughly. Copilot suggestions are helpful but should be verified for correctness and compatibility with the mod's existing codebase.

By following these guidelines, developers can effectively utilize GitHub Copilot to enhance the Destroyed City (Continued) mod and continue to improve the RimWorld experience.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

