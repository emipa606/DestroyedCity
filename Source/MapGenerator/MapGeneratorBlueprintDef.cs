using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MapGenerator;

public class MapGeneratorBlueprintDef : Def
{
    public string buildingData;

    //public LetterType TriggerLetterType = 1;

    public Dictionary<string, ThingDef> buildingLegend;

    public ThingDef buildingMaterial = null;

    public bool canHaveHoles;

    public float chance = 1f;

    public bool createTrigger;

    public FactionDef factionDef = null;

    public FactionSelection factionSelection = FactionSelection.none;

    public string floorData;

    public Dictionary<string, TerrainDef> floorLegend;

    public string itemData;

    public Dictionary<string, ThingDef> itemLegend;

    public float itemSpawnChance = 0f;

    public bool mapCenterBlueprint = false;

    public string pawnData;

    public Dictionary<string, PawnKindDef> pawnLegend;

    public float pawnSpawnChance = 0f;

    public Dictionary<string, Rot4> rotationLegend;

    public IntVec2 size;

    public string TriggerLetterLabel = null;

    public string TriggerLetterMessageText = null;
}