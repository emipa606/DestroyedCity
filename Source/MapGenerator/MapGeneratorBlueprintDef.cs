using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MapGenerator;

public class MapGeneratorBlueprintDef : Def
{
    public readonly ThingDef buildingMaterial = null;

    public readonly float chance = 1f;

    public readonly FactionDef factionDef = null;

    public readonly FactionSelection factionSelection = FactionSelection.none;

    public readonly float itemSpawnChance = 0f;

    public readonly bool mapCenterBlueprint = false;

    public readonly float pawnSpawnChance = 0f;
    public string buildingData;

    //public LetterType TriggerLetterType = 1;

    public Dictionary<string, ThingDef> buildingLegend;

    public bool canHaveHoles;

    public bool createTrigger;

    public string floorData;

    public Dictionary<string, TerrainDef> floorLegend;

    public string itemData;

    public Dictionary<string, ThingDef> itemLegend;

    public string pawnData;

    public Dictionary<string, PawnKindDef> pawnLegend;

    public Dictionary<string, Rot4> rotationLegend;

    public IntVec2 size;

    public string TriggerLetterLabel = null;

    public string TriggerLetterMessageText = null;
}