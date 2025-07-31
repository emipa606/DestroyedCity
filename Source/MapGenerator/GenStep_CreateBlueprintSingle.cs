using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MapGenerator;

public class GenStep_CreateBlueprintSingle : GenStep_Scatterer
{
    private List<Pawn> allSpawnedPawns;

    private Faction faction;

    private bool mapCenterBlueprintUsed;

    private ThingDef selectedWallStuff;

    public override int SeedPart => 1158116085;

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
    {
        if (mapCenterBlueprintUsed)
        {
            return;
        }

        var mapGeneratorBlueprintDef =
            DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading.RandomElementByWeight(b => b.chance);
        if (!mapGeneratorBlueprintDef.mapCenterBlueprint &&
            mapGeneratorBlueprintDef.pawnLegend is { Count: > 0 })
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

        scatterBlueprintAt(loc, mapGeneratorBlueprintDef, map, ref selectedWallStuff);
        selectedWallStuff = null;
    }

    protected override bool CanScatterAt(IntVec3 loc, Map map)
    {
        return base.CanScatterAt(loc, map) && loc.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy);
    }

    private void scatterBlueprintAt(IntVec3 loc, MapGeneratorBlueprintDef blueprint, Map map,
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

        wallStuff ??= randomWallStuff();

        makeBlueprintRoom(cellRect, blueprint, map, wallStuff);

        if (!blueprint.createTrigger)
        {
            return;
        }

        var rectTrigger_UnfogArea =
            (RectTrigger_UnfogArea)ThingMaker.MakeThing(ThingDef.Named("RectTrigger_UnfogArea"));
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

    private void makeBlueprintRoom(CellRect mapRect, MapGeneratorBlueprintDef blueprint, Map map, ThingDef stuffDef)
    {
        blueprint.buildingData = cleanUpBlueprintData(blueprint.buildingData);
        blueprint.floorData = cleanUpBlueprintData(blueprint.floorData);
        blueprint.pawnData = cleanUpBlueprintData(blueprint.pawnData);
        blueprint.itemData = cleanUpBlueprintData(blueprint.itemData);
        if (blueprint.buildingData == null && blueprint.floorData == null)
        {
            Log.ErrorOnce(
                $"After cleaning the BlueprintData and FloorData of blueprint {blueprint.defName} -> both are null, nothing will be done!",
                313001);
        }
        else
        {
            var a = new IntVec3(mapRect.minX, 0, mapRect.maxZ);
            foreach (var c in mapRect)
            {
                if (!checkCell(c, map))
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
                    var thingDef = tryGetThingDefFromBuildingData(blueprint, itemPos);
                    var thingRot = tryGetRotationFromBuildingData(blueprint, itemPos);
                    var terrainDef = tryGetTerrainDefFromFloorData(blueprint, itemPos);
                    var pawnKindDef = tryGetPawnKindDefFromPawnData(blueprint, itemPos);
                    var thingDef2 = tryGetItemDefFromItemData(blueprint, itemPos);
                    var list = map.thingGrid.ThingsListAt(c2);
                    foreach (var thing in list)
                    {
                        if (thing.def == thingDef)
                        {
                        }
                    }

                    if (thingDef != null || terrainDef != null || pawnKindDef != null || thingDef2 != null)
                    {
                        clearCell(c2, map);
                    }

                    if (!(blueprint.canHaveHoles && Rand.Value < 0.08f))
                    {
                        trySetCellAs(c2, thingDef, thingRot, map, stuffDef, terrainDef, pawnKindDef, thingDef2,
                            blueprint);
                    }
                }
            }

            if (allSpawnedPawns is { Count: > 0 })
            {
                var rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
                rectTrigger.Rect = mapRect.ExpandedBy(1).ClipInsideMap(map);
                //rectTrigger.letter = new Letter(Translator.Translate("LetterLabelAncientShrineWarning"), Translator.Translate("AncientShrineWarning"), 1, mapRect.CenterCell);
                rectTrigger.destroyIfUnfogged = false;
                GenSpawn.Spawn(rectTrigger, mapRect.CenterCell, map);
            }

            if (allSpawnedPawns is not { Count: > 0 })
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

    private static string cleanUpBlueprintData(string data)
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
                if (char.IsLetterOrDigit(c) || c is ',' or '.' or '#' or '~' or '?' or '!' or '-' or '+' or '*' or '@')
                {
                    text += c.ToString();
                }
            }

            result = text.NullOrEmpty() ? null : text;
        }

        return result;
    }

    private static TerrainDef tryGetTerrainDefFromFloorData(MapGeneratorBlueprintDef blueprint, int itemPos)
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

            result = blueprint.floorLegend.GetValueOrDefault(key);
        }

        return result;
    }

    private static ThingDef tryGetThingDefFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
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
            result = blueprint.buildingLegend.GetValueOrDefault(key);
        }

        return result;
    }

    private static Rot4 tryGetRotationFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
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
            result = !blueprint.rotationLegend.TryGetValue(key, out var value) ? Rot4.Invalid : value;
        }

        return result;
    }

    private static ThingDef tryGetItemDefFromItemData(MapGeneratorBlueprintDef blueprint, int itemPos)
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
            result = blueprint.itemLegend.GetValueOrDefault(key);
        }

        return result;
    }

    private static PawnKindDef tryGetPawnKindDefFromPawnData(MapGeneratorBlueprintDef blueprint, int itemPos)
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
            result = blueprint.pawnLegend.GetValueOrDefault(key);
        }

        return result;
    }

    private static void clearCell(IntVec3 c, Map map)
    {
        var thingList = c.GetThingList(map);
        if (!checkCell(c, map))
        {
            return;
        }

        for (var i = thingList.Count - 1; i >= 0; i--)
        {
            thingList[i].Destroy();
        }
    }

    private static bool checkCell(IntVec3 c, Map map)
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

    private void trySetCellAs(IntVec3 c, ThingDef thingDef, Rot4 thingRot, Map map, ThingDef stuffDef = null,
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
                    map.terrainGrid.SetTerrain(c, correspondingTileDef(stuffDef));
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

                var thing2 = tryGetTreasure(itemDef, thingDef2);
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

            faction ??= Find.FactionManager.FirstFactionOfDef(blueprint.factionDef);

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

            allSpawnedPawns ??= [];

            allSpawnedPawns.Add(pawn);
        }
    }

    private static Thing tryGetTreasure(ThingDef treasureDef, ThingDef stuffDef)
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
                thing.HitPoints = Rand.RangeInclusive((int)(thing.MaxHitPoints * 0.4), thing.MaxHitPoints);
            }

            result = thing;
        }

        return result;
    }

    private static ThingDef randomWallStuff()
    {
        return (from def in DefDatabase<ThingDef>.AllDefs
            where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f &&
                  def.BaseMarketValue / def.VolumePerUnit < 15f
            select def).RandomElement();
    }

    private static TerrainDef correspondingTileDef(ThingDef stuffDef)
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

        terrainDef ??= TerrainDefOf.Concrete;

        return terrainDef;
    }
}