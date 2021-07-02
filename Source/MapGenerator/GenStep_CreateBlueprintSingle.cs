using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MapGenerator
{
    // Token: 0x02000002 RID: 2
    public class GenStep_CreateBlueprintSingle : GenStep_Scatterer
    {
        // Token: 0x04000003 RID: 3
        private List<Pawn> allSpawnedPawns;

        // Token: 0x04000002 RID: 2
        private Faction faction;

        // Token: 0x04000004 RID: 4
        private bool mapCenterBlueprintUsed;

        // Token: 0x04000001 RID: 1
        private ThingDef selectedWallStuff;

        public override int SeedPart => 1158116085;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
        {
            if (mapCenterBlueprintUsed)
            {
                return;
            }

            var mapGeneratorBlueprintDef =
                DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading.RandomElementByWeight(b => b.chance);
            if (!mapGeneratorBlueprintDef.mapCenterBlueprint &&
                mapGeneratorBlueprintDef.pawnLegend != null &&
                mapGeneratorBlueprintDef.pawnLegend.Count > 0)
            {
                var intVec = new IntVec2(map.Size.x / 2, map.Size.z / 2);
                var cellRect = new CellRect(intVec.x, intVec.z, 1, 1);
                cellRect = cellRect.ExpandedBy(20);
                if (cellRect.Contains(loc))
                {
                    mapGeneratorBlueprintDef =
                        (from b in DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading
                            where b.pawnLegend == null || b.pawnLegend.Count == 0
                            select b).RandomElementByWeight(b => b.chance);
                }
            }

            if (mapGeneratorBlueprintDef == null)
            {
                return;
            }

            var mapCenterBlueprint = mapGeneratorBlueprintDef.mapCenterBlueprint;
            if (mapCenterBlueprint)
            {
                loc = new IntVec3(map.Center.x - (mapGeneratorBlueprintDef.size.x / 2), map.Center.y,
                    map.Center.z - (mapGeneratorBlueprintDef.size.z / 2));
                mapCenterBlueprintUsed = true;
            }

            ScatterBlueprintAt(loc, mapGeneratorBlueprintDef, map, ref selectedWallStuff);
            selectedWallStuff = null;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000021F0 File Offset: 0x000003F0
        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return base.CanScatterAt(loc, map) && loc.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002218 File Offset: 0x00000418
        private void ScatterBlueprintAt(IntVec3 loc, MapGeneratorBlueprintDef blueprint, Map map,
            ref ThingDef wallStuff)
        {
            var cellRect = new CellRect(loc.x, loc.z, blueprint.size.x, blueprint.size.z);
            cellRect.ClipInsideMap(map);
            if (cellRect.Width != blueprint.size.x || cellRect.Height != blueprint.size.z)
            {
                return;
            }

            foreach (var intVec in cellRect.Cells)
            {
                var list = map.thingGrid.ThingsListAt(intVec);
                foreach (var thing in list)
                {
                    if (thing.def == ThingDefOf.AncientCryptosleepCasket)
                    {
                        return;
                    }
                }

                usedSpots.Add(intVec);
            }

            if (blueprint.buildingMaterial != null)
            {
                wallStuff = blueprint.buildingMaterial;
            }

            if (wallStuff == null)
            {
                wallStuff = RandomWallStuff();
            }

            MakeBlueprintRoom(cellRect, blueprint, map, wallStuff);

            if (!blueprint.createTrigger)
            {
                return;
            }

            var rectTrigger_UnfogArea =
                (RectTrigger_UnfogArea) ThingMaker.MakeThing(ThingDef.Named("RectTrigger_UnfogArea"));
            rectTrigger_UnfogArea.destroyIfUnfogged = true;
            rectTrigger_UnfogArea.Rect = cellRect;
            //bool flag5 = blueprint.TriggerLetterMessageText != null;
            //if (flag5)
            //{
            //	bool flag6 = blueprint.TriggerLetterLabel != null;
            //	if (flag6)
            //	{
            //		rectTrigger_UnfogArea.letter = new Letter(Translator.Translate(blueprint.TriggerLetterLabel), Translator.Translate(blueprint.TriggerLetterMessageText), blueprint.TriggerLetterType, cellRect.CenterCell);
            //	}
            //	else
            //	{
            //		rectTrigger_UnfogArea.letter = new Letter("", Translator.Translate(blueprint.TriggerLetterMessageText), blueprint.TriggerLetterType, cellRect.CenterCell);
            //	}
            //}
            GenSpawn.Spawn(rectTrigger_UnfogArea, cellRect.CenterCell, map);
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002428 File Offset: 0x00000628
        private void MakeBlueprintRoom(CellRect mapRect, MapGeneratorBlueprintDef blueprint, Map map, ThingDef stuffDef)
        {
            blueprint.buildingData = CleanUpBlueprintData(blueprint.buildingData);
            blueprint.floorData = CleanUpBlueprintData(blueprint.floorData);
            blueprint.pawnData = CleanUpBlueprintData(blueprint.pawnData);
            blueprint.itemData = CleanUpBlueprintData(blueprint.itemData);
            if (blueprint.buildingData == null && blueprint.floorData == null)
            {
                Log.ErrorOnce(
                    $"After cleaning the BlueprintData and FloorData of blueprint {blueprint.defName} -> both are null, nothing will be done!",
                    313001);
            }
            else
            {
                var a = new IntVec3(mapRect.BottomLeft.x, mapRect.TopRight.y, mapRect.TopRight.z);
                foreach (var c in mapRect)
                {
                    if (!CheckCell(c, map))
                    {
                        return;
                    }
                }

                allSpawnedPawns = null;
                for (var i = 0; i < blueprint.size.z; i++)
                {
                    for (var j = 0; j < blueprint.size.x; j++)
                    {
                        var c2 = a + new IntVec3(j, 0, -i);
                        var itemPos = j + (blueprint.size.x * i);
                        var thingDef = TryGetThingDefFromBuildingData(blueprint, itemPos);
                        var thingRot = TryGetRotationFromBuildingData(blueprint, itemPos);
                        var terrainDef = TryGetTerrainDefFromFloorData(blueprint, itemPos);
                        var pawnKindDef = TryGetPawnKindDefFromPawnData(blueprint, itemPos);
                        var thingDef2 = TryGetItemDefFromItemData(blueprint, itemPos);
                        var list = map.thingGrid.ThingsListAt(c2);
                        foreach (var thing in list)
                        {
                            if (thing.def == thingDef)
                            {
                            }
                        }

                        if (thingDef != null || terrainDef != null || pawnKindDef != null || thingDef2 != null)
                        {
                            ClearCell(c2, map);
                        }

                        if (!(blueprint.canHaveHoles && Rand.Value < 0.08f))
                        {
                            TrySetCellAs(c2, thingDef, thingRot, map, stuffDef, terrainDef, pawnKindDef, thingDef2,
                                blueprint);
                        }
                    }
                }

                if (allSpawnedPawns != null && allSpawnedPawns.Count > 0)
                {
                    var rectTrigger = (RectTrigger) ThingMaker.MakeThing(ThingDefOf.RectTrigger);
                    rectTrigger.Rect = mapRect.ExpandedBy(1).ClipInsideMap(map);
                    //rectTrigger.letter = new Letter(Translator.Translate("LetterLabelAncientShrineWarning"), Translator.Translate("AncientShrineWarning"), 1, mapRect.CenterCell);
                    rectTrigger.destroyIfUnfogged = false;
                    GenSpawn.Spawn(rectTrigger, mapRect.CenterCell, map);
                }

                if (allSpawnedPawns == null || allSpawnedPawns.Count <= 0)
                {
                    return;
                }

                LordJob lordJob;
                if (blueprint.factionSelection == FactionSelection.friendly)
                {
                    lordJob = new LordJob_AssistColony(allSpawnedPawns[0].Faction, allSpawnedPawns[0].Position);
                }
                else
                {
                    if (Rand.Value < 0.5f)
                    {
                        lordJob = new LordJob_DefendPoint(allSpawnedPawns[0].Position);
                    }
                    else
                    {
                        lordJob = new LordJob_AssaultColony(allSpawnedPawns[0].Faction, false, false);
                    }
                }

                LordMaker.MakeNewLord(allSpawnedPawns[0].Faction, lordJob, map, allSpawnedPawns);
                allSpawnedPawns = null;
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000027E8 File Offset: 0x000009E8
        private string CleanUpBlueprintData(string data)
        {
            string result;
            if (data.NullOrEmpty())
            {
                result = null;
            }
            else
            {
                var text = "";
                foreach (var c in data)
                {
                    if (char.IsLetterOrDigit(c) || c == ',' || c == '.' || c == '#' || c == '~' || c == '?' ||
                        c == '!' || c == '-' || c == '+' || c == '*' || c == '@')
                    {
                        text += c.ToString();
                    }
                }

                result = text.NullOrEmpty() ? null : text;
            }

            return result;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000028A4 File Offset: 0x00000AA4
        private TerrainDef TryGetTerrainDefFromFloorData(MapGeneratorBlueprintDef blueprint, int itemPos)
        {
            TerrainDef result;
            if (blueprint.floorData == null || blueprint.floorData.Length - 1 < itemPos ||
                blueprint.floorLegend == null)
            {
                result = null;
            }
            else
            {
                var key = blueprint.floorData.ElementAt(itemPos).ToString();

                result = !blueprint.floorLegend.ContainsKey(key) ? null : blueprint.floorLegend[key];
            }

            return result;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x0000291C File Offset: 0x00000B1C
        private ThingDef TryGetThingDefFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
        {
            ThingDef result;
            if (blueprint.buildingData == null || blueprint.buildingData.Length - 1 < itemPos ||
                blueprint.buildingLegend == null)
            {
                result = null;
            }
            else
            {
                var key = blueprint.buildingData.ElementAt(itemPos).ToString();
                result = !blueprint.buildingLegend.ContainsKey(key) ? null : blueprint.buildingLegend[key];
            }

            return result;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002994 File Offset: 0x00000B94
        private Rot4 TryGetRotationFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
        {
            Rot4 result;
            if (blueprint.buildingData == null || blueprint.buildingData.Length - 1 < itemPos ||
                blueprint.rotationLegend == null)
            {
                result = Rot4.Invalid;
            }
            else
            {
                var key = blueprint.buildingData.ElementAt(itemPos).ToString();
                result = !blueprint.rotationLegend.ContainsKey(key) ? Rot4.Invalid : blueprint.rotationLegend[key];
            }

            return result;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002A14 File Offset: 0x00000C14
        private ThingDef TryGetItemDefFromItemData(MapGeneratorBlueprintDef blueprint, int itemPos)
        {
            ThingDef result;
            if (blueprint.itemData == null || blueprint.itemData.Length - 1 < itemPos ||
                blueprint.itemLegend == null)
            {
                result = null;
            }
            else
            {
                var key = blueprint.itemData.ElementAt(itemPos).ToString();
                result = !blueprint.itemLegend.ContainsKey(key) ? null : blueprint.itemLegend[key];
            }

            return result;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002A8C File Offset: 0x00000C8C
        private PawnKindDef TryGetPawnKindDefFromPawnData(MapGeneratorBlueprintDef blueprint, int itemPos)
        {
            PawnKindDef result;
            if (blueprint.pawnData == null || blueprint.pawnData.Length - 1 < itemPos ||
                blueprint.pawnLegend == null)
            {
                result = null;
            }
            else
            {
                var key = blueprint.pawnData.ElementAt(itemPos).ToString();
                result = !blueprint.pawnLegend.ContainsKey(key) ? null : blueprint.pawnLegend[key];
            }

            return result;
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002B04 File Offset: 0x00000D04
        private void ClearCell(IntVec3 c, Map map)
        {
            var thingList = c.GetThingList(map);
            if (!CheckCell(c, map))
            {
                return;
            }

            for (var i = thingList.Count - 1; i >= 0; i--)
            {
                thingList[i].Destroy();
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002B5C File Offset: 0x00000D5C
        private bool CheckCell(IntVec3 c, Map map)
        {
            var thingList = c.GetThingList(map);
            foreach (var thing in thingList)
            {
                if (!thing.def.destroyable)
                {
                    return false;
                }
            }

            return true;
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002BAC File Offset: 0x00000DAC
        private void TrySetCellAs(IntVec3 c, ThingDef thingDef, Rot4 thingRot, Map map, ThingDef stuffDef = null,
            TerrainDef terrainDef = null, PawnKindDef pawnKindDef = null, ThingDef itemDef = null,
            MapGeneratorBlueprintDef blueprint = null)
        {
            if (!c.InBounds(map))
            {
                Log.Warning("GenStep_CreateBlueprint: Invalid Target-Cell: cell is null or out of bounds.");
            }
            else
            {
                var thingList = c.GetThingList(map);
                foreach (var thing in thingList)
                {
                    if (!thing.def.destroyable)
                    {
                        return;
                    }
                }

                if (terrainDef != null)
                {
                    map.terrainGrid.SetTerrain(c, terrainDef);
                }
                else
                {
                    if (thingDef != null && stuffDef != null)
                    {
                        map.terrainGrid.SetTerrain(c, CorrespondingTileDef(stuffDef));
                    }
                }

                if (thingDef != null)
                {
                    var stuff = stuffDef;
                    if (!thingDef.MadeFromStuff)
                    {
                        stuff = null;
                    }

                    var thing = ThingMaker.MakeThing(thingDef, stuff);
                    if (thingRot == Rot4.Invalid)
                    {
                        GenSpawn.Spawn(thing, c, map);
                    }
                    else
                    {
                        GenSpawn.Spawn(thing, c, map, thingRot);
                    }
                }

                if (blueprint == null)
                {
                    return;
                }

                if (itemDef != null && blueprint.itemSpawnChance / 100f > Rand.Value)
                {
                    var isApparel = itemDef.IsApparel;
                    ThingDef thingDef2;
                    if (isApparel)
                    {
                        var num = 0;
                        for (;;)
                        {
                            thingDef2 = DefDatabase<ThingDef>.GetRandom();
                            if (thingDef2.IsStuff &&
                                thingDef2.stuffCategories.Contains(StuffCategoryDefOf.Fabric))
                            {
                                break;
                            }

                            num++;
                            if (num > 100)
                            {
                                goto Block_17;
                            }
                        }

                        goto IL_18F;
                        Block_17:
                        thingDef2 = DefDatabase<ThingDef>.GetNamedSilentFail("Synthread");
                        IL_18F: ;
                    }
                    else
                    {
                        var source = new List<string>
                        {
                            "Steel",
                            "Steel",
                            "Steel",
                            "Steel",
                            "Steel",
                            "Plasteel"
                        };
                        thingDef2 = DefDatabase<ThingDef>.GetNamedSilentFail(source.RandomElement());
                    }

                    if (!itemDef.MadeFromStuff)
                    {
                        thingDef2 = null;
                    }

                    var thing2 = TryGetTreasure(itemDef, thingDef2);
                    thing2 = GenSpawn.Spawn(thing2, c, map);
                    if (thing2.TryGetComp<CompForbiddable>() != null)
                    {
                        thing2.SetForbidden(true);
                    }

                    if (thing2 is Hive hive)
                    {
                        hive.CompDormant.WakeUp();
                    }
                }

                if (pawnKindDef == null || !(blueprint.pawnSpawnChance / 100f > Rand.Value))
                {
                    return;
                }

                if (faction == null)
                {
                    faction = Find.FactionManager.FirstFactionOfDef(blueprint.factionDef);
                }

                if (faction == null)
                {
                    switch (blueprint.factionSelection)
                    {
                        case FactionSelection.none:
                        {
                            faction = Find.FactionManager.AllFactions.RandomElementByWeight(fac =>
                                fac.def.settlementGenerationWeight);
                            if (faction == null)
                            {
                                faction = Faction.OfMechanoids;
                            }

                            break;
                        }
                        case FactionSelection.hostile:
                        {
                            faction = (from fac in Find.FactionManager.AllFactions
                                where fac.HostileTo(Faction.OfPlayer)
                                select fac).RandomElementByWeight(fac =>
                                101f - fac.def.settlementGenerationWeight);
                            if (faction == null)
                            {
                                faction = Faction.OfMechanoids;
                            }

                            break;
                        }
                        case FactionSelection.friendly:
                        {
                            faction = (from fac in Find.FactionManager.AllFactions
                                where !fac.HostileTo(Faction.OfPlayer) && fac.PlayerGoodwill > 0f &&
                                      fac != Faction.OfPlayer
                                select fac).RandomElementByWeight(fac =>
                                101f - fac.def.settlementGenerationWeight);
                            if (faction == null)
                            {
                                faction = Find.FactionManager.AllFactions.RandomElementByWeight(fac =>
                                    fac.def.settlementGenerationWeight);
                            }

                            break;
                        }
                    }
                }

                var pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
                pawn.mindState.Active = false;
                pawn = GenSpawn.Spawn(pawn, c, map) as Pawn;

                if (pawn == null)
                {
                    return;
                }

                if (allSpawnedPawns == null)
                {
                    allSpawnedPawns = new List<Pawn>();
                }

                allSpawnedPawns.Add(pawn);
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00003054 File Offset: 0x00001254
        private Thing TryGetTreasure(ThingDef treasureDef, ThingDef stuffDef)
        {
            Thing result;
            if (treasureDef == null)
            {
                result = null;
            }
            else
            {
                var thing = ThingMaker.MakeThing(treasureDef, stuffDef);
                var compQuality = thing.TryGetComp<CompQuality>();
                compQuality?.SetQuality(QualityUtility.AllQualityCategories.RandomElement(),
                    ArtGenerationContext.Outsider);

                if (thing.def.stackLimit > 1)
                {
                    thing.stackCount = Rand.RangeInclusive(1, thing.def.stackLimit);
                }

                if (thing.stackCount == 1)
                {
                    thing.HitPoints = Rand.RangeInclusive((int) (thing.MaxHitPoints * 0.4), thing.MaxHitPoints);
                }

                result = thing;
            }

            return result;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00003108 File Offset: 0x00001308
        private ThingDef RandomWallStuff()
        {
            return (from def in DefDatabase<ThingDef>.AllDefs
                where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f &&
                      def.BaseMarketValue / def.VolumePerUnit < 15f
                select def).RandomElement();
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00003148 File Offset: 0x00001348
        public TerrainDef CorrespondingTileDef(ThingDef stuffDef)
        {
            TerrainDef terrainDef = null;
            var allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
            foreach (var terrainDef1 in allDefsListForReading)
            {
                if (terrainDef1.costList != null)
                {
                    foreach (var thingDefCountClass in terrainDef1.costList)
                    {
                        if (thingDefCountClass.thingDef != stuffDef)
                        {
                            continue;
                        }

                        terrainDef = terrainDef1;
                        break;
                    }
                }

                if (terrainDef != null)
                {
                    break;
                }
            }

            if (terrainDef == null)
            {
                terrainDef = TerrainDefOf.Concrete;
            }

            return terrainDef;
        }
    }
}