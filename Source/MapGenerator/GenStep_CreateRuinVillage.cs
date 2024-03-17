using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MapGenerator;

public class GenStep_CreateRuinVillage : GenStep_Scatterer
{
    private readonly IntRange ruinOffsetHorizontalRange = new IntRange(5, 15);

    private readonly IntRange ruinOffsetVerticalRange = new IntRange(5, 15);

    private readonly List<IntVec3> usedCells = [];

    private int ruinCountDown;

    private IntRange ruinCountRange = new IntRange(3, 8);

    private IntRange ruinDistanceRange = new IntRange(4, 10);

    private bool ruinsHaveBigHoles;

    private ThingDef selectedWallStuff;

    public IntRange villageCountRange = new IntRange(1, 1);

    public override int SeedPart => 1158116083;

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
    {
        ruinCountDown = ruinCountRange.RandomInRange;
        ruinsHaveBigHoles = Rand.Value > 0.5f;
        var scatterDone = false;
        while (ruinCountDown > 0)
        {
            var intVec = TryFindValidScatterCellNear(loc, map);
            if (intVec != IntVec3.Invalid)
            {
                ScatterRuinAt(intVec, Rand.Value > 0.4f && !scatterDone, map, ref selectedWallStuff);
                if (Rand.Value > 0.4f && !scatterDone)
                {
                    scatterDone = true;
                    continue;
                }
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

    private IntVec3 TryFindValidScatterCellNear(IntVec3 loc, Map map)
    {
        IntVec3 result;
        if (usedCells.Count == 0)
        {
            result = loc;
        }
        else
        {
            var i = 0;
            var num = 30;
            while (i < num)
            {
                var num2 = Rand.RangeInclusive(0, 7);
                var intVec = IntVec3.Invalid;
                foreach (var intVec2 in usedCells)
                {
                    switch (num2)
                    {
                        case 0:
                            intVec = intVec == IntVec3.Invalid || intVec2.z > intVec.z ? intVec2 : intVec;
                            break;
                        case 1:
                            intVec = intVec == IntVec3.Invalid || intVec2.z > intVec.z && intVec2.x > intVec.x
                                ? intVec2
                                : intVec;
                            break;
                        case 2:
                            intVec = intVec == IntVec3.Invalid || intVec2.x > intVec.x ? intVec2 : intVec;
                            break;
                        case 3:
                            intVec = intVec == IntVec3.Invalid || intVec2.z < intVec.z && intVec2.x > intVec.x
                                ? intVec2
                                : intVec;
                            break;
                        case 4:
                            intVec = intVec == IntVec3.Invalid || intVec2.z < intVec.z ? intVec2 : intVec;
                            break;
                        case 5:
                            intVec = intVec == IntVec3.Invalid || intVec2.z < intVec.z && intVec2.x < intVec.x
                                ? intVec2
                                : intVec;
                            break;
                        case 6:
                            intVec = intVec == IntVec3.Invalid || intVec2.x < intVec.x ? intVec2 : intVec;
                            break;
                        case 7:
                            intVec = intVec == IntVec3.Invalid || intVec2.z > intVec.z && intVec2.x < intVec.x
                                ? intVec2
                                : intVec;
                            break;
                        default:
                            intVec = IntVec3.Invalid;
                            break;
                    }
                }

                if (intVec == IntVec3.Invalid)
                {
                    return IntVec3.Invalid;
                }

                var invalid = IntVec3.Invalid;
                switch (num2)
                {
                    case 0:
                        invalid = new IntVec3(0, 0, ruinDistanceRange.RandomInRange);
                        intVec += invalid;
                        break;
                    case 1:
                        invalid = new IntVec3(ruinDistanceRange.RandomInRange, 0, ruinDistanceRange.RandomInRange);
                        intVec += invalid;
                        break;
                    case 2:
                        invalid = new IntVec3(ruinDistanceRange.RandomInRange, 0, 0);
                        intVec += invalid;
                        break;
                    case 3:
                        invalid = new IntVec3(ruinDistanceRange.RandomInRange, 0,
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetVerticalRange.Average);
                        intVec += invalid;
                        break;
                    case 4:
                        invalid = new IntVec3(0, 0,
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetVerticalRange.Average);
                        intVec += invalid;
                        break;
                    case 5:
                        invalid = new IntVec3(
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetHorizontalRange.Average, 0,
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetVerticalRange.Average);
                        intVec += invalid;
                        break;
                    case 6:
                        invalid = new IntVec3(
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetHorizontalRange.Average, 0, 0);
                        intVec += invalid;
                        break;
                    case 7:
                        invalid = new IntVec3(
                            -ruinDistanceRange.RandomInRange - (int)ruinOffsetHorizontalRange.Average, 0,
                            ruinDistanceRange.RandomInRange);
                        intVec += invalid;
                        break;
                    default:
                        intVec = IntVec3.Invalid;
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
                            ruinDistanceRange.max = ruinDistanceRange.min + 4;
                        }
                    }
                }

                if (intVec.InBounds(map) && CanScatterAt(intVec, map))
                {
                    return intVec;
                }

                i++;
            }

            result = IntVec3.Invalid;
        }

        return result;
    }

    private void ScatterRuinAt(IntVec3 loc, bool placeWell, Map map, ref ThingDef wallStuff)
    {
        var randomInRange = ruinOffsetHorizontalRange.RandomInRange;
        var randomInRange2 = ruinOffsetVerticalRange.RandomInRange;
        var num = randomInRange + 2;
        var num2 = randomInRange2 + 2;
        var mapRect = new CellRect(loc.x, loc.z, num, num2);
        mapRect.ClipInsideMap(map);
        if (mapRect.Width != num || mapRect.Height != num2)
        {
            return;
        }

        foreach (var intVec in mapRect.Cells)
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

        if (wallStuff == null)
        {
            wallStuff = RandomWallStuff();
        }

        if (placeWell)
        {
            MakeWell(mapRect, wallStuff, map);
        }
        else
        {
            if (ruinsHaveBigHoles)
            {
                MakeShed(mapRect, wallStuff, map, true, true);
            }
            else
            {
                MakeShed(mapRect, wallStuff, map, Rand.Value > 0.5f);
            }
        }
    }

    private void MakeWell(CellRect mapRect, ThingDef stuffDef, Map map)
    {
        var centerCell = mapRect.CenterCell;
        mapRect = new CellRect(centerCell.x - 2, centerCell.z - 2, 5, 5);
        foreach (var intVec in mapRect)
        {
            if (intVec.x == mapRect.minX || intVec.x == mapRect.maxX || intVec.z == mapRect.minZ ||
                intVec.z == mapRect.maxZ || intVec.x == mapRect.minX + 1 && intVec.z == mapRect.minZ + 1 ||
                intVec.x == mapRect.minX + 1 && intVec.z == mapRect.maxZ - 1 ||
                intVec.x == mapRect.maxX - 1 && intVec.z == mapRect.minZ + 1 ||
                intVec.x == mapRect.maxX - 1 && intVec.z == mapRect.maxZ - 1)
            {
                if ((intVec.x != mapRect.minX || intVec.z != mapRect.minZ) &&
                    (intVec.x != mapRect.minX || intVec.z != mapRect.maxZ) &&
                    (intVec.x != mapRect.maxX || intVec.z != mapRect.minZ) &&
                    (intVec.x != mapRect.maxX || intVec.z != mapRect.maxZ))
                {
                    TrySetCellAsTile(intVec, stuffDef, map);
                }
                else
                {
                    var edifice = intVec.GetEdifice(map);
                    edifice?.Destroy();
                }
            }
            else
            {
                var edifice2 = intVec.GetEdifice(map);
                edifice2?.Destroy();

                map.terrainGrid.SetTerrain(intVec, TerrainDef.Named("WaterShallow"));
            }
        }
    }

    private void MakeShed(CellRect mapRect, ThingDef stuffDef, Map map, bool leaveDoorGaps = true,
        bool leaveBigHoles = false)
    {
        mapRect.ClipInsideMap(map);
        foreach (var intVec in mapRect)
        {
            if (intVec.x == mapRect.minX || intVec.x == mapRect.maxX || intVec.z == mapRect.minZ ||
                intVec.z == mapRect.maxZ)
            {
                if (!leaveDoorGaps && !leaveBigHoles || leaveBigHoles && Rand.Value >= 0.25f ||
                    !leaveBigHoles && Rand.Value >= 0.05f)
                {
                    TrySetCellAsWall(intVec, map, stuffDef);
                }
            }
            else
            {
                if (Rand.Value < 0.05f)
                {
                    continue;
                }

                var edifice = intVec.GetEdifice(map);
                edifice?.Destroy();

                map.terrainGrid.SetTerrain(intVec, CorrespondingTileDef(stuffDef));
            }
        }
    }

    private void TrySetCellAsWall(IntVec3 c, Map map, ThingDef stuffDef)
    {
        var thingList = c.GetThingList(map);
        foreach (var thing1 in thingList)
        {
            if (!thing1.def.destroyable)
            {
                return;
            }
        }

        for (var j = thingList.Count - 1; j >= 0; j--)
        {
            thingList[j].Destroy();
        }

        map.terrainGrid.SetTerrain(c, CorrespondingTileDef(stuffDef));
        var wall = ThingDefOf.Wall;
        var thing = ThingMaker.MakeThing(wall, stuffDef);
        GenSpawn.Spawn(thing, c, map);
    }

    private void TrySetCellAsTile(IntVec3 c, ThingDef stuffDef, Map map)
    {
        var thingList = c.GetThingList(map);
        foreach (var thing in thingList)
        {
            if (!thing.def.destroyable)
            {
                return;
            }
        }

        for (var j = thingList.Count - 1; j >= 0; j--)
        {
            thingList[j].Destroy();
        }

        map.terrainGrid.SetTerrain(c, CorrespondingTileDef(stuffDef));
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