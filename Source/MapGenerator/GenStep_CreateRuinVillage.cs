using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MapGenerator
{
	// Token: 0x02000004 RID: 4
	public class GenStep_CreateRuinVillage : GenStep_Scatterer
	{
		public override int SeedPart
		{
			get
			{
				return 1158116083;
			}
		}
		// Token: 0x06000025 RID: 37 RVA: 0x000049F4 File Offset: 0x00002BF4
		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
		{
			this.ruinCountDown = this.ruinCountRange.RandomInRange;
			this.ruinsHaveBigHoles = (Rand.Value > 0.5f);
			bool flag = Rand.Value > 0.4f;
			bool flag2 = false;
			while (this.ruinCountDown > 0)
			{
				bool flag3 = flag && !flag2;
				IntVec3 intVec = this.TryFindValidScatterCellNear(loc, map);
				bool flag4 = intVec != IntVec3.Invalid;
				if (flag4)
				{
					this.ScatterRuinAt(intVec, flag3, map, ref this.selectedWallStuff);
					bool flag5 = flag3;
					if (flag5)
					{
						flag2 = true;
						continue;
					}
				}
				this.ruinCountDown--;
			}
			this.selectedWallStuff = null;
			this.usedCells.Clear();
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00004AAC File Offset: 0x00002CAC
		protected override bool CanScatterAt(IntVec3 loc, Map map)
		{
			return base.CanScatterAt(loc, map) && GenGrid.SupportsStructureType(loc, map, TerrainAffordanceDefOf.Heavy);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00004AD4 File Offset: 0x00002CD4
		private IntVec3 TryFindValidScatterCellNear(IntVec3 loc, Map map)
		{
			bool flag = this.usedCells.Count == 0;
			IntVec3 result;
			if (flag)
			{
				result = loc;
			}
			else
			{
				int i = 0;
				int num = 30;
				while (i < num)
				{
					int num2 = Rand.RangeInclusive(0, 7);
					IntVec3 intVec = IntVec3.Invalid;
					foreach (IntVec3 intVec2 in this.usedCells)
					{
						switch (num2)
						{
						case 0:
							intVec = ((intVec == IntVec3.Invalid || intVec2.z > intVec.z) ? intVec2 : intVec);
							break;
						case 1:
							intVec = ((intVec == IntVec3.Invalid || (intVec2.z > intVec.z && intVec2.x > intVec.x)) ? intVec2 : intVec);
							break;
						case 2:
							intVec = ((intVec == IntVec3.Invalid || intVec2.x > intVec.x) ? intVec2 : intVec);
							break;
						case 3:
							intVec = ((intVec == IntVec3.Invalid || (intVec2.z < intVec.z && intVec2.x > intVec.x)) ? intVec2 : intVec);
							break;
						case 4:
							intVec = ((intVec == IntVec3.Invalid || intVec2.z < intVec.z) ? intVec2 : intVec);
							break;
						case 5:
							intVec = ((intVec == IntVec3.Invalid || (intVec2.z < intVec.z && intVec2.x < intVec.x)) ? intVec2 : intVec);
							break;
						case 6:
							intVec = ((intVec == IntVec3.Invalid || intVec2.x < intVec.x) ? intVec2 : intVec);
							break;
						case 7:
							intVec = ((intVec == IntVec3.Invalid || (intVec2.z > intVec.z && intVec2.x < intVec.x)) ? intVec2 : intVec);
							break;
						default:
							intVec = IntVec3.Invalid;
							break;
						}
					}
					bool flag2 = intVec == IntVec3.Invalid;
					if (flag2)
					{
						return IntVec3.Invalid;
					}
					IntVec3 invalid = IntVec3.Invalid;
					switch (num2)
					{
					case 0:
						invalid = new IntVec3(0, 0, this.ruinDistanceRange.RandomInRange);
						intVec += invalid;
						break;
					case 1:
						invalid = new IntVec3(this.ruinDistanceRange.RandomInRange, 0, this.ruinDistanceRange.RandomInRange);
						intVec += invalid;
						break;
					case 2:
						invalid = new IntVec3(this.ruinDistanceRange.RandomInRange, 0, 0);
						intVec += invalid;
						break;
					case 3:
						invalid = new IntVec3(this.ruinDistanceRange.RandomInRange, 0, -this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetVerticalRange.Average);
						intVec += invalid;
						break;
					case 4:
						invalid = new IntVec3(0, 0, -this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetVerticalRange.Average);
						intVec += invalid;
						break;
					case 5:
						invalid = new IntVec3(-this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetHorizontalRange.Average, 0, -this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetVerticalRange.Average);
						intVec += invalid;
						break;
					case 6:
						invalid = new IntVec3(-this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetHorizontalRange.Average, 0, 0);
						intVec += invalid;
						break;
					case 7:
						invalid = new IntVec3(-this.ruinDistanceRange.RandomInRange - (int)this.ruinOffsetHorizontalRange.Average, 0, this.ruinDistanceRange.RandomInRange);
						intVec += invalid;
						break;
					default:
						intVec = IntVec3.Invalid;
						break;
					}
					bool isValid = invalid.IsValid;
					if (isValid)
					{
						bool flag3 = Math.Abs(invalid.x) / 2 < this.ruinDistanceRange.max && Math.Abs(invalid.z) / 2 < this.ruinDistanceRange.max;
						if (flag3)
						{
							bool flag4 = Math.Abs(invalid.x) > Math.Abs(invalid.z);
							if (flag4)
							{
								this.ruinDistanceRange.min = Math.Abs(invalid.x) / 2;
							}
							else
							{
								this.ruinDistanceRange.min = Math.Abs(invalid.z) / 2;
							}
							bool flag5 = this.ruinDistanceRange.min > this.ruinDistanceRange.max;
							if (flag5)
							{
								this.ruinDistanceRange.max = this.ruinDistanceRange.min + 4;
							}
						}
					}
					bool flag6 = GenGrid.InBounds(intVec, map) && this.CanScatterAt(intVec, map);
					if (flag6)
					{
						return intVec;
					}
					i++;
				}
				result = IntVec3.Invalid;
			}
			return result;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00005058 File Offset: 0x00003258
		private void ScatterRuinAt(IntVec3 loc, bool placeWell, Map map, ref ThingDef wallStuff)
		{
			int randomInRange = this.ruinOffsetHorizontalRange.RandomInRange;
			int randomInRange2 = this.ruinOffsetVerticalRange.RandomInRange;
			int num = randomInRange + 2;
			int num2 = randomInRange2 + 2;
			CellRect mapRect = new CellRect(loc.x, loc.z, num, num2);
			mapRect.ClipInsideMap(map);
			bool flag = mapRect.Width != num || mapRect.Height != num2;
			if (!flag)
			{
				foreach (IntVec3 intVec in mapRect.Cells)
				{
					List<Thing> list = map.thingGrid.ThingsListAt(intVec);
					for (int i = 0; i < list.Count; i++)
					{
						bool flag2 = list[i].def == ThingDefOf.AncientCryptosleepCasket;
						if (flag2)
						{
							return;
						}
					}
					this.usedCells.Add(intVec);
					this.usedSpots.Add(intVec);
				}
				bool flag3 = wallStuff == null;
				if (flag3)
				{
					wallStuff = this.RandomWallStuff();
				}
				if (placeWell)
				{
					this.MakeWell(mapRect, wallStuff, map);
				}
				else
				{
					bool flag4 = this.ruinsHaveBigHoles;
					if (flag4)
					{
						this.MakeShed(mapRect, wallStuff, map,true, true);
					}
					else
					{
						this.MakeShed(mapRect, wallStuff, map,Rand.Value > 0.5f, false);
					}
				}
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000051DC File Offset: 0x000033DC
		public void MakeWell(CellRect mapRect, ThingDef stuffDef, Map map)
		{
			IntVec3 centerCell = mapRect.CenterCell;
			mapRect = new CellRect(centerCell.x - 2, centerCell.z - 2, 5, 5);
			foreach (IntVec3 intVec in mapRect)
			{
				bool flag = intVec.x == mapRect.minX || intVec.x == mapRect.maxX || intVec.z == mapRect.minZ || intVec.z == mapRect.maxZ || (intVec.x == mapRect.minX + 1 && intVec.z == mapRect.minZ + 1) || (intVec.x == mapRect.minX + 1 && intVec.z == mapRect.maxZ - 1) || (intVec.x == mapRect.maxX - 1 && intVec.z == mapRect.minZ + 1) || (intVec.x == mapRect.maxX - 1 && intVec.z == mapRect.maxZ - 1);
				if (flag)
				{
					bool flag2 = (intVec.x != mapRect.minX || intVec.z != mapRect.minZ) && (intVec.x != mapRect.minX || intVec.z != mapRect.maxZ) && (intVec.x != mapRect.maxX || intVec.z != mapRect.minZ) && (intVec.x != mapRect.maxX || intVec.z != mapRect.maxZ);
					if (flag2)
					{
						this.TrySetCellAsTile(intVec, stuffDef, map);
					}
					else
					{
						Building edifice = GridsUtility.GetEdifice(intVec, map);
						bool flag3 = edifice != null;
						if (flag3)
						{
							edifice.Destroy(DestroyMode.Vanish);
						}
					}
				}
				else
				{
					Building edifice2 = GridsUtility.GetEdifice(intVec, map);
					bool flag4 = edifice2 != null;
					if (flag4)
					{
						edifice2.Destroy(DestroyMode.Vanish);
					}
					map.terrainGrid.SetTerrain(intVec, TerrainDef.Named("WaterShallow"));
				}
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00005410 File Offset: 0x00003610
		public void MakeShed(CellRect mapRect, ThingDef stuffDef, Map map, bool leaveDoorGaps = true, bool leaveBigHoles = false)
		{
			mapRect.ClipInsideMap(map);
			foreach (IntVec3 intVec in mapRect)
			{
				bool flag = intVec.x == mapRect.minX || intVec.x == mapRect.maxX || intVec.z == mapRect.minZ || intVec.z == mapRect.maxZ;
				if (flag)
				{
					bool flag2 = (!leaveDoorGaps && !leaveBigHoles) || (leaveBigHoles && Rand.Value >= 0.25f) || (!leaveBigHoles && Rand.Value >= 0.05f);
					if (flag2)
					{
						this.TrySetCellAsWall(intVec, map, stuffDef);
					}
				}
				else
				{
					bool flag3 = Rand.Value < 0.05f;
					if (!flag3)
					{
						Building edifice = GridsUtility.GetEdifice(intVec, map);
						bool flag4 = edifice != null;
						if (flag4)
						{
							edifice.Destroy(DestroyMode.Vanish);
						}
						map.terrainGrid.SetTerrain(intVec, this.CorrespondingTileDef(stuffDef));
					}
				}
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00005534 File Offset: 0x00003734
		private void TrySetCellAsWall(IntVec3 c, Map map, ThingDef stuffDef)
		{
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			for (int i = 0; i < thingList.Count; i++)
			{
				bool flag = !thingList[i].def.destroyable;
				if (flag)
				{
					return;
				}
			}
			for (int j = thingList.Count - 1; j >= 0; j--)
			{
				thingList[j].Destroy(DestroyMode.Vanish);
			}
			map.terrainGrid.SetTerrain(c, this.CorrespondingTileDef(stuffDef));
			ThingDef wall = ThingDefOf.Wall;
			Thing thing = ThingMaker.MakeThing(wall, stuffDef);
			GenSpawn.Spawn(thing, c, map);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000055DC File Offset: 0x000037DC
		private void TrySetCellAsTile(IntVec3 c, ThingDef stuffDef, Map map)
		{
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			for (int i = 0; i < thingList.Count; i++)
			{
				bool flag = !thingList[i].def.destroyable;
				if (flag)
				{
					return;
				}
			}
			for (int j = thingList.Count - 1; j >= 0; j--)
			{
				thingList[j].Destroy(DestroyMode.Vanish);
			}
			map.terrainGrid.SetTerrain(c, this.CorrespondingTileDef(stuffDef));
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00005668 File Offset: 0x00003868
		protected ThingDef RandomWallStuff()
		{
			return (from def in DefDatabase<ThingDef>.AllDefs
			where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f && def.BaseMarketValue / def.VolumePerUnit < 15f
			select def).RandomElement<ThingDef>();
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000056A8 File Offset: 0x000038A8
		protected TerrainDef CorrespondingTileDef(ThingDef stuffDef)
		{
			TerrainDef terrainDef = null;
			List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				bool flag = allDefsListForReading[i].costList != null;
				if (flag)
				{
					for (int j = 0; j < allDefsListForReading[i].costList.Count; j++)
					{
						bool flag2 = allDefsListForReading[i].costList[j].thingDef == stuffDef;
						if (flag2)
						{
							terrainDef = allDefsListForReading[i];
							break;
						}
					}
				}
				bool flag3 = terrainDef != null;
				if (flag3)
				{
					break;
				}
			}
			bool flag4 = terrainDef == null;
			if (flag4)
			{
				terrainDef = TerrainDefOf.Concrete;
			}
			return terrainDef;
		}

		// Token: 0x0400000F RID: 15
		public readonly IntRange ruinOffsetHorizontalRange = new IntRange(5, 15);

		// Token: 0x04000010 RID: 16
		public readonly IntRange ruinOffsetVerticalRange = new IntRange(5, 15);

		// Token: 0x04000011 RID: 17
		public IntRange ruinDistanceRange = new IntRange(4, 10);

		// Token: 0x04000012 RID: 18
		public IntRange ruinCountRange = new IntRange(3, 8);

		// Token: 0x04000013 RID: 19
		private int ruinCountDown;

		// Token: 0x04000014 RID: 20
		public IntRange villageCountRange = new IntRange(1, 1);

		// Token: 0x04000015 RID: 21
		private List<IntVec3> usedCells = new List<IntVec3>();

		// Token: 0x04000016 RID: 22
		private ThingDef selectedWallStuff;

		// Token: 0x04000017 RID: 23
		private bool ruinsHaveBigHoles;
	}
}
