using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MapGenerator
{
    // Token: 0x02000005 RID: 5
    public class MapGeneratorBlueprintDef : Def
    {
        // Token: 0x0400001C RID: 28
        public string buildingData;

        // Token: 0x04000024 RID: 36
        //public LetterType TriggerLetterType = 1;

        // Token: 0x04000025 RID: 37
        public Dictionary<string, ThingDef> buildingLegend;

        // Token: 0x0400001B RID: 27
        public ThingDef buildingMaterial = null;

        // Token: 0x04000020 RID: 32
        public bool canHaveHoles;

        // Token: 0x04000019 RID: 25
        public float chance = 1f;

        // Token: 0x04000021 RID: 33
        public bool createTrigger;

        // Token: 0x0400002C RID: 44
        public FactionDef factionDef = null;

        // Token: 0x0400002D RID: 45
        public FactionSelection factionSelection = FactionSelection.none;

        // Token: 0x0400001D RID: 29
        public string floorData;

        // Token: 0x04000027 RID: 39
        public Dictionary<string, TerrainDef> floorLegend;

        // Token: 0x0400001F RID: 31
        public string itemData;

        // Token: 0x04000029 RID: 41
        public Dictionary<string, ThingDef> itemLegend;

        // Token: 0x0400002B RID: 43
        public float itemSpawnChance = 0f;

        // Token: 0x04000018 RID: 24
        public bool mapCenterBlueprint = false;

        // Token: 0x0400001E RID: 30
        public string pawnData;

        // Token: 0x04000028 RID: 40
        public Dictionary<string, PawnKindDef> pawnLegend;

        // Token: 0x0400002A RID: 42
        public float pawnSpawnChance = 0f;

        // Token: 0x04000026 RID: 38
        public Dictionary<string, Rot4> rotationLegend;

        // Token: 0x0400001A RID: 26
        public IntVec2 size;

        // Token: 0x04000022 RID: 34
        public string TriggerLetterLabel = null;

        // Token: 0x04000023 RID: 35
        public string TriggerLetterMessageText = null;
    }
}