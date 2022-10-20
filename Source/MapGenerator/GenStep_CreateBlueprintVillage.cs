using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MapGenerator;

public class GenStep_CreateBlueprintVillage : GenStep_Scatterer
{
    private readonly IntRange ruinOffsetHorizontalRange = new IntRange(5, 15);

    private readonly IntRange ruinOffsetVerticalRange = new IntRange(5, 15);

    private readonly List<IntVec3> usedCells = new List<IntVec3>();

    private List<Pawn> allSpawnedPawns;

    private Faction faction;

    private int ruinCountDown;

    private IntRange ruinCountRange = new IntRange(3, 8);

    private IntRange ruinDistanceRange = new IntRange(4, 20);

    private ThingDef selectedWallStuff;

    public IntRange villageCountRange = new IntRange(1, 1);

    public override int SeedPart => 1158116084;

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
    {
        ruinCountDown = ruinCountRange.RandomInRange;
        while (ruinCountDown > 0)
        {
            var mapGeneratorBlueprintDef = (from b in DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading
                where !b.mapCenterBlueprint
                select b).RandomElementByWeight(b => b.chance);
            if (mapGeneratorBlueprintDef.size.x > mapGeneratorBlueprintDef.size.z)
            {
                ruinDistanceRange.min = (mapGeneratorBlueprintDef.size.x / 2) + 1;
            }
            else
            {
                ruinDistanceRange.min = (mapGeneratorBlueprintDef.size.z / 2) + 1;
            }

            if (ruinDistanceRange.min > ruinDistanceRange.max)
            {
                ruinDistanceRange.max = ruinDistanceRange.min + 4;
            }

            var intVec = TryFindValidScatterCellNear(loc, mapGeneratorBlueprintDef, map, usedCells);
            if (intVec != IntVec3.Invalid)
            {
                ScatterBlueprintAt(intVec, mapGeneratorBlueprintDef, map, ref selectedWallStuff);
            }

            ruinCountDown--;
        }

        selectedWallStuff = null;
        usedCells.Clear();
    }

    protected override bool CanScatterAt(IntVec3 loc, Map map)
    {
        return base.CanScatterAt(loc, map) && loc.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy);
    }

    private IntVec3 TryFindValidScatterCellNear(IntVec3 loc, MapGeneratorBlueprintDef blueprint, Map map,
        List<IntVec3> invalidCells)
    {
        IntVec3 result;
        if (usedCells.Count == 0)
        {
            result = loc;
        }
        else
        {
            var size = blueprint.size;
            var mapCenterBlueprint = blueprint.mapCenterBlueprint;
            var i = 0;
            var num = 30;
            while (i < num)
            {
                if (!mapCenterBlueprint)
                {
                    var intVec = new IntVec2(map.Size.x / 2, map.Size.z / 2);
                    var cellRect = new CellRect(intVec.x, intVec.z, 1, 1);
                    cellRect = cellRect.ExpandedBy(10);
                    if (cellRect.Contains(loc))
                    {
                        loc = new IntVec3(loc.x + 20, loc.y, loc.z - 20);
                    }
                }

                var num2 = Rand.RangeInclusive(0, 7);
                var intVec2 = IntVec3.Invalid;
                foreach (var intVec3 in usedCells)
                {
                    switch (num2)
                    {
                        case 0:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z > intVec2.z ? intVec3 : intVec2;
                            break;
                        case 1:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z > intVec2.z && intVec3.x > intVec2.x
                                ? intVec3
                                : intVec2;
                            break;
                        case 2:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.x > intVec2.x ? intVec3 : intVec2;
                            break;
                        case 3:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z < intVec2.z && intVec3.x > intVec2.x
                                ? intVec3
                                : intVec2;
                            break;
                        case 4:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z < intVec2.z ? intVec3 : intVec2;
                            break;
                        case 5:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z < intVec2.z && intVec3.x < intVec2.x
                                ? intVec3
                                : intVec2;
                            break;
                        case 6:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.x < intVec2.x ? intVec3 : intVec2;
                            break;
                        case 7:
                            intVec2 = intVec2 == IntVec3.Invalid || intVec3.z > intVec2.z && intVec3.x < intVec2.x
                                ? intVec3
                                : intVec2;
                            break;
                        default:
                            intVec2 = IntVec3.Invalid;
                            break;
                    }
                }

                if (intVec2 == IntVec3.Invalid)
                {
                    return IntVec3.Invalid;
                }

                var invalid = IntVec3.Invalid;
                int num3;
                if (size.x > size.z)
                {
                    num3 = size.x + Rand.RangeInclusive(1, 5);
                }
                else
                {
                    num3 = size.z + Rand.RangeInclusive(1, 5);
                }

                switch (num2)
                {
                    case 0:
                        invalid = new IntVec3(0, 0, num3);
                        intVec2 += invalid;
                        break;
                    case 1:
                        invalid = new IntVec3(num3, 0, num3);
                        intVec2 += invalid;
                        break;
                    case 2:
                        invalid = new IntVec3(num3, 0, 0);
                        intVec2 += invalid;
                        break;
                    case 3:
                        invalid = new IntVec3(num3, 0, -num3 - (int)ruinOffsetVerticalRange.Average);
                        intVec2 += invalid;
                        break;
                    case 4:
                        invalid = new IntVec3(0, 0, -num3 - (int)ruinOffsetVerticalRange.Average);
                        intVec2 += invalid;
                        break;
                    case 5:
                        invalid = new IntVec3(-num3 - (int)ruinOffsetHorizontalRange.Average, 0,
                            -num3 - (int)ruinOffsetVerticalRange.Average);
                        intVec2 += invalid;
                        break;
                    case 6:
                        invalid = new IntVec3(-num3 - (int)ruinOffsetHorizontalRange.Average, 0, 0);
                        intVec2 += invalid;
                        break;
                    case 7:
                        invalid = new IntVec3(-num3 - (int)ruinOffsetHorizontalRange.Average, 0, num3);
                        intVec2 += invalid;
                        break;
                    default:
                        intVec2 = IntVec3.Invalid;
                        break;
                }

                var isValid = invalid.IsValid;
                if (isValid)
                {
                    if (Math.Abs(invalid.x) / 2 < ruinDistanceRange.max &&
                        Math.Abs(invalid.z) / 2 < ruinDistanceRange.max)
                    {
                        if (Math.Abs(invalid.x) > Math.Abs(invalid.z))
                        {
                            ruinDistanceRange.min = Math.Abs(invalid.x) / 2;
                        }
                        else
                        {
                            ruinDistanceRange.min = Math.Abs(invalid.z) / 2;
                        }

                        if (ruinDistanceRange.min > ruinDistanceRange.max)
                        {
                            ruinDistanceRange.max = ruinDistanceRange.min;
                        }
                    }
                }

                if (intVec2.InBounds(map) && CanScatterAt(intVec2, map) &&
                    IsPositionValidForBlueprint(intVec2, size, invalidCells))
                {
                    return intVec2;
                }

                i++;
            }

            result = IntVec3.Invalid;
        }

        return result;
    }

    private bool IsPositionValidForBlueprint(IntVec3 cell, IntVec2 size, List<IntVec3> invalidCells)
    {
        var cellRect = new CellRect(cell.x, cell.z, size.x, size.z);
        var list = new List<IntVec3>(cellRect.Cells);
        foreach (var a in invalidCells)
        {
            foreach (var b in list)
            {
                if (a == b)
                {
                    return false;
                }
            }
        }

        return true;
    }

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

            usedCells.Add(intVec);
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
        var createTrigger = blueprint.createTrigger;
        if (!createTrigger)
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

            if (allSpawnedPawns is { Count: > 0 })
            {
                var rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
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
                if (char.IsLetterOrDigit(c) || c is ',' or '.' or '#' or '~' or '?' or '!' or '-' or '+' or '*' or '@')
                {
                    text += c.ToString();
                }
            }

            result = text.NullOrEmpty() ? null : text;
        }

        return result;
    }

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

    private void ClearCell(IntVec3 c, Map map)
    {
        var thingList = c.GetThingList(map);
        if (!CheckCell(c, map))
        {
        }
        else
        {
            for (var i = thingList.Count - 1; i >= 0; i--)
            {
                thingList[i].Destroy();
            }
        }
    }

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
                var source = new List<string>
                {
                    "Steel",
                    "Steel"
                };
                var stuffDef2 = DefDatabase<ThingDef>.GetNamedSilentFail(source.RandomElement());
                if (!itemDef.MadeFromStuff)
                {
                    stuffDef2 = null;
                }

                var thing2 = TryGetTreasure(itemDef, stuffDef2);
                thing2 = GenSpawn.Spawn(thing2, c, map);
                thing2.SetForbidden(true);
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
                thing.HitPoints = Rand.RangeInclusive(40, thing.MaxHitPoints);
            }

            result = thing;
        }

        return result;
    }

    private ThingDef RandomWallStuff()
    {
        return (from def in DefDatabase<ThingDef>.AllDefs
            where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f &&
                  def.BaseMarketValue / def.VolumePerUnit < 15f
            select def).RandomElement();
    }

    private TerrainDef CorrespondingTileDef(ThingDef stuffDef)
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