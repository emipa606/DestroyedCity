using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MapGenerator
{
	// Token: 0x02000003 RID: 3
	public class GenStep_CreateBlueprintVillage : GenStep_Scatterer
	{
		public override int SeedPart
		{
			get
			{
				return 1158116084;
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00003214 File Offset: 0x00001414
		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
		{
			this.ruinCountDown = this.ruinCountRange.RandomInRange;
			while (this.ruinCountDown > 0)
			{
				MapGeneratorBlueprintDef mapGeneratorBlueprintDef = (from b in DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading
				where !b.mapCenterBlueprint
				select b).RandomElementByWeight((MapGeneratorBlueprintDef b) => b.chance);
				bool flag = mapGeneratorBlueprintDef.size.x > mapGeneratorBlueprintDef.size.z;
				if (flag)
				{
					this.ruinDistanceRange.min = mapGeneratorBlueprintDef.size.x / 2 + 1;
				}
				else
				{
					this.ruinDistanceRange.min = mapGeneratorBlueprintDef.size.z / 2 + 1;
				}
				bool flag2 = this.ruinDistanceRange.min > this.ruinDistanceRange.max;
				if (flag2)
				{
					this.ruinDistanceRange.max = this.ruinDistanceRange.min + 4;
				}
				IntVec3 intVec = this.TryFindValidScatterCellNear(loc, mapGeneratorBlueprintDef, map, this.usedCells);
				bool flag3 = intVec != IntVec3.Invalid;
				if (flag3)
				{
					this.ScatterBlueprintAt(intVec, mapGeneratorBlueprintDef, map, ref this.selectedWallStuff);
				}
				this.ruinCountDown--;
			}
			this.selectedWallStuff = null;
			this.usedCells.Clear();
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00003374 File Offset: 0x00001574
		protected override bool CanScatterAt(IntVec3 loc, Map map)
		{
			return base.CanScatterAt(loc, map) && GenGrid.SupportsStructureType(loc, map, TerrainAffordanceDefOf.Heavy);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000339C File Offset: 0x0000159C
		private IntVec3 TryFindValidScatterCellNear(IntVec3 loc, MapGeneratorBlueprintDef blueprint, Map map, List<IntVec3> invalidCells)
		{
			bool flag = this.usedCells.Count == 0;
			IntVec3 result;
			if (flag)
			{
				result = loc;
			}
			else
			{
				IntVec2 size = blueprint.size;
				bool mapCenterBlueprint = blueprint.mapCenterBlueprint;
				int i = 0;
				int num = 30;
				while (i < num)
				{
					bool flag2 = !mapCenterBlueprint;
					if (flag2)
					{
						IntVec2 intVec = new IntVec2(map.Size.x / 2, map.Size.z / 2);
						CellRect cellRect = new CellRect(intVec.x, intVec.z, 1, 1);
						cellRect = cellRect.ExpandedBy(10);
						bool flag3 = cellRect.Contains(loc);
						if (flag3)
						{
							loc = new IntVec3(loc.x + 20, loc.y, loc.z - 20);
						}
					}
					int num2 = Rand.RangeInclusive(0, 7);
					IntVec3 intVec2 = IntVec3.Invalid;
					foreach (IntVec3 intVec3 in this.usedCells)
					{
						switch (num2)
						{
						case 0:
							intVec2 = ((intVec2 == IntVec3.Invalid || intVec3.z > intVec2.z) ? intVec3 : intVec2);
							break;
						case 1:
							intVec2 = ((intVec2 == IntVec3.Invalid || (intVec3.z > intVec2.z && intVec3.x > intVec2.x)) ? intVec3 : intVec2);
							break;
						case 2:
							intVec2 = ((intVec2 == IntVec3.Invalid || intVec3.x > intVec2.x) ? intVec3 : intVec2);
							break;
						case 3:
							intVec2 = ((intVec2 == IntVec3.Invalid || (intVec3.z < intVec2.z && intVec3.x > intVec2.x)) ? intVec3 : intVec2);
							break;
						case 4:
							intVec2 = ((intVec2 == IntVec3.Invalid || intVec3.z < intVec2.z) ? intVec3 : intVec2);
							break;
						case 5:
							intVec2 = ((intVec2 == IntVec3.Invalid || (intVec3.z < intVec2.z && intVec3.x < intVec2.x)) ? intVec3 : intVec2);
							break;
						case 6:
							intVec2 = ((intVec2 == IntVec3.Invalid || intVec3.x < intVec2.x) ? intVec3 : intVec2);
							break;
						case 7:
							intVec2 = ((intVec2 == IntVec3.Invalid || (intVec3.z > intVec2.z && intVec3.x < intVec2.x)) ? intVec3 : intVec2);
							break;
						default:
							intVec2 = IntVec3.Invalid;
							break;
						}
					}
					bool flag4 = intVec2 == IntVec3.Invalid;
					if (flag4)
					{
						return IntVec3.Invalid;
					}
					IntVec3 invalid = IntVec3.Invalid;
					int num3 = this.ruinDistanceRange.RandomInRange;
					bool flag5 = size.x > size.z;
					if (flag5)
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
						invalid = new IntVec3(num3, 0, -num3 - (int)this.ruinOffsetVerticalRange.Average);
						intVec2 += invalid;
						break;
					case 4:
						invalid = new IntVec3(0, 0, -num3 - (int)this.ruinOffsetVerticalRange.Average);
						intVec2 += invalid;
						break;
					case 5:
						invalid = new IntVec3(-num3 - (int)this.ruinOffsetHorizontalRange.Average, 0, -num3 - (int)this.ruinOffsetVerticalRange.Average);
						intVec2 += invalid;
						break;
					case 6:
						invalid = new IntVec3(-num3 - (int)this.ruinOffsetHorizontalRange.Average, 0, 0);
						intVec2 += invalid;
						break;
					case 7:
						invalid = new IntVec3(-num3 - (int)this.ruinOffsetHorizontalRange.Average, 0, num3);
						intVec2 += invalid;
						break;
					default:
						intVec2 = IntVec3.Invalid;
						break;
					}
					bool isValid = invalid.IsValid;
					if (isValid)
					{
						bool flag6 = Math.Abs(invalid.x) / 2 < this.ruinDistanceRange.max && Math.Abs(invalid.z) / 2 < this.ruinDistanceRange.max;
						if (flag6)
						{
							bool flag7 = Math.Abs(invalid.x) > Math.Abs(invalid.z);
							if (flag7)
							{
								this.ruinDistanceRange.min = Math.Abs(invalid.x) / 2;
							}
							else
							{
								this.ruinDistanceRange.min = Math.Abs(invalid.z) / 2;
							}
							bool flag8 = this.ruinDistanceRange.min > this.ruinDistanceRange.max;
							if (flag8)
							{
								this.ruinDistanceRange.max = this.ruinDistanceRange.min;
							}
						}
					}
					bool flag9 = GenGrid.InBounds(intVec2, map) && this.CanScatterAt(intVec2, map) && this.IsPositionValidForBlueprint(intVec2, size, invalidCells);
					if (flag9)
					{
						return intVec2;
					}
					i++;
				}
				result = IntVec3.Invalid;
			}
			return result;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x0000399C File Offset: 0x00001B9C
		private bool IsPositionValidForBlueprint(IntVec3 cell, IntVec2 size, List<IntVec3> invalidCells)
		{
			CellRect cellRect = new CellRect(cell.x, cell.z, size.x, size.z);
			List<IntVec3> list = new List<IntVec3>(cellRect.Cells);
			foreach (IntVec3 a in invalidCells)
			{
				foreach (IntVec3 b in list)
				{
					bool flag = a == b;
					if (flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00003A6C File Offset: 0x00001C6C
		private void ScatterBlueprintAt(IntVec3 loc, MapGeneratorBlueprintDef blueprint, Map map, ref ThingDef wallStuff)
		{
			CellRect cellRect = new CellRect(loc.x, loc.z, blueprint.size.x, blueprint.size.z);
			cellRect.ClipInsideMap(map);
			bool flag = cellRect.Width != blueprint.size.x || cellRect.Height != blueprint.size.z;
			if (!flag)
			{
				foreach (IntVec3 intVec in cellRect.Cells)
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
				bool flag3 = blueprint.buildingMaterial != null;
				if (flag3)
				{
					wallStuff = blueprint.buildingMaterial;
				}
				bool flag4 = wallStuff == null;
				if (flag4)
				{
					wallStuff = this.RandomWallStuff();
				}
				this.MakeBlueprintRoom(cellRect, blueprint, map, wallStuff);
				bool createTrigger = blueprint.createTrigger;
				if (createTrigger)
				{
					RectTrigger_UnfogArea rectTrigger_UnfogArea = (RectTrigger_UnfogArea)ThingMaker.MakeThing(ThingDef.Named("RectTrigger_UnfogArea"), null);
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
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00003C88 File Offset: 0x00001E88
		private void MakeBlueprintRoom(CellRect mapRect, MapGeneratorBlueprintDef blueprint, Map map, ThingDef stuffDef)
		{
			blueprint.buildingData = this.CleanUpBlueprintData(blueprint.buildingData);
			blueprint.floorData = this.CleanUpBlueprintData(blueprint.floorData);
			blueprint.pawnData = this.CleanUpBlueprintData(blueprint.pawnData);
			blueprint.itemData = this.CleanUpBlueprintData(blueprint.itemData);
			bool flag = blueprint.buildingData == null && blueprint.floorData == null;
			if (flag)
			{
				Log.ErrorOnce(string.Format("After cleaning the BlueprintData and FloorData of blueprint {0} -> both are null, nothing will be done!", blueprint.defName), 313001);
			}
			else
			{
				IntVec3 a = new IntVec3(mapRect.BottomLeft.x, mapRect.TopRight.y, mapRect.TopRight.z);
				foreach (IntVec3 c in mapRect)
				{
					bool flag2 = !this.CheckCell(c, map);
					if (flag2)
					{
						return;
					}
				}
				this.allSpawnedPawns = null;
				for (int i = 0; i < blueprint.size.z; i++)
				{
					for (int j = 0; j < blueprint.size.x; j++)
					{
						IntVec3 c2 = a + new IntVec3(j, 0, -i);
						int itemPos = j + blueprint.size.x * i;
						ThingDef thingDef = this.TryGetThingDefFromBuildingData(blueprint, itemPos);
						Rot4 thingRot = this.TryGetRotationFromBuildingData(blueprint, itemPos);
						TerrainDef terrainDef = this.TryGetTerrainDefFromFloorData(blueprint, itemPos);
						PawnKindDef pawnKindDef = this.TryGetPawnKindDefFromPawnData(blueprint, itemPos);
						ThingDef thingDef2 = this.TryGetItemDefFromItemData(blueprint, itemPos);
						List<Thing> list = map.thingGrid.ThingsListAt(c2);
						for (int k = 0; k < list.Count; k++)
						{
							bool flag3 = list[k].def == thingDef;
							if (flag3)
							{
							}
						}
						bool flag4 = thingDef != null || terrainDef != null || pawnKindDef != null || thingDef2 != null;
						if (flag4)
						{
							this.ClearCell(c2, map);
						}
						bool flag5 = blueprint.canHaveHoles && Rand.Value < 0.08f;
						if (!flag5)
						{
							this.TrySetCellAs(c2, thingDef, thingRot, map, stuffDef, terrainDef, pawnKindDef, thingDef2, blueprint);
						}
					}
				}
				bool flag6 = this.allSpawnedPawns != null && this.allSpawnedPawns.Count > 0;
				if (flag6)
				{
					RectTrigger rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
					rectTrigger.Rect = mapRect.ExpandedBy(1).ClipInsideMap(map);
					//rectTrigger.letter = new Letter(Translator.Translate("LetterLabelAncientShrineWarning"), Translator.Translate("AncientShrineWarning"), 1, mapRect.CenterCell);
					rectTrigger.destroyIfUnfogged = false;
					GenSpawn.Spawn(rectTrigger, mapRect.CenterCell, map);
				}
				bool flag7 = this.allSpawnedPawns != null && this.allSpawnedPawns.Count > 0;
				if (flag7)
				{
					bool flag8 = blueprint.factionSelection == FactionSelection.friendly;
					LordJob lordJob;
					if (flag8)
					{
						lordJob = new LordJob_AssistColony(this.allSpawnedPawns[0].Faction, this.allSpawnedPawns[0].Position);
					}
					else
					{
						bool flag9 = Rand.Value < 0.5f;
						if (flag9)
						{
							lordJob = new LordJob_DefendPoint(this.allSpawnedPawns[0].Position);
						}
						else
						{
							lordJob = new LordJob_AssaultColony(this.allSpawnedPawns[0].Faction, false, false, false, false, true);
						}
					}
					LordMaker.MakeNewLord(this.allSpawnedPawns[0].Faction, lordJob, map, this.allSpawnedPawns);
					this.allSpawnedPawns = null;
				}
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00004048 File Offset: 0x00002248
		private string CleanUpBlueprintData(string data)
		{
			bool flag = data.NullOrEmpty();
			string result;
			if (flag)
			{
				result = null;
			}
			else
			{
				string text = "";
				foreach (char c in data)
				{
					bool flag2 = char.IsLetterOrDigit(c) || c == ',' || c == '.' || c == '#' || c == '~' || c == '?' || c == '!' || c == '-' || c == '+' || c == '*' || c == '@';
					if (flag2)
					{
						text += c.ToString();
					}
				}
				bool flag3 = text.NullOrEmpty();
				if (flag3)
				{
					result = null;
				}
				else
				{
					result = text;
				}
			}
			return result;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00004104 File Offset: 0x00002304
		private TerrainDef TryGetTerrainDefFromFloorData(MapGeneratorBlueprintDef blueprint, int itemPos)
		{
			bool flag = blueprint.floorData == null || blueprint.floorData.Count<char>() - 1 < itemPos || blueprint.floorLegend == null;
			TerrainDef result;
			if (flag)
			{
				result = null;
			}
			else
			{
				string key = blueprint.floorData.ElementAt(itemPos).ToString();
				bool flag2 = !blueprint.floorLegend.ContainsKey(key);
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = blueprint.floorLegend[key];
				}
			}
			return result;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000417C File Offset: 0x0000237C
		private ThingDef TryGetThingDefFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
		{
			bool flag = blueprint.buildingData == null || blueprint.buildingData.Count<char>() - 1 < itemPos || blueprint.buildingLegend == null;
			ThingDef result;
			if (flag)
			{
				result = null;
			}
			else
			{
				string key = blueprint.buildingData.ElementAt(itemPos).ToString();
				bool flag2 = !blueprint.buildingLegend.ContainsKey(key);
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = blueprint.buildingLegend[key];
				}
			}
			return result;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000041F4 File Offset: 0x000023F4
		private Rot4 TryGetRotationFromBuildingData(MapGeneratorBlueprintDef blueprint, int itemPos)
		{
			bool flag = blueprint.buildingData == null || blueprint.buildingData.Count<char>() - 1 < itemPos || blueprint.rotationLegend == null;
			Rot4 result;
			if (flag)
			{
				result = Rot4.Invalid;
			}
			else
			{
				string key = blueprint.buildingData.ElementAt(itemPos).ToString();
				bool flag2 = !blueprint.rotationLegend.ContainsKey(key);
				if (flag2)
				{
					result = Rot4.Invalid;
				}
				else
				{
					result = blueprint.rotationLegend[key];
				}
			}
			return result;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00004274 File Offset: 0x00002474
		private ThingDef TryGetItemDefFromItemData(MapGeneratorBlueprintDef blueprint, int itemPos)
		{
			bool flag = blueprint.itemData == null || blueprint.itemData.Count<char>() - 1 < itemPos || blueprint.itemLegend == null;
			ThingDef result;
			if (flag)
			{
				result = null;
			}
			else
			{
				string key = blueprint.itemData.ElementAt(itemPos).ToString();
				bool flag2 = !blueprint.itemLegend.ContainsKey(key);
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = blueprint.itemLegend[key];
				}
			}
			return result;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000042EC File Offset: 0x000024EC
		private PawnKindDef TryGetPawnKindDefFromPawnData(MapGeneratorBlueprintDef blueprint, int itemPos)
		{
			bool flag = blueprint.pawnData == null || blueprint.pawnData.Count<char>() - 1 < itemPos || blueprint.pawnLegend == null;
			PawnKindDef result;
			if (flag)
			{
				result = null;
			}
			else
			{
				string key = blueprint.pawnData.ElementAt(itemPos).ToString();
				bool flag2 = !blueprint.pawnLegend.ContainsKey(key);
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = blueprint.pawnLegend[key];
				}
			}
			return result;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00004364 File Offset: 0x00002564
		private bool ClearCell(IntVec3 c, Map map)
		{
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			bool flag = !this.CheckCell(c, map);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				for (int i = thingList.Count - 1; i >= 0; i--)
				{
					thingList[i].Destroy(DestroyMode.Vanish);
				}
				result = true;
			}
			return result;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000043BC File Offset: 0x000025BC
		private bool CheckCell(IntVec3 c, Map map)
		{
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			for (int i = 0; i < thingList.Count; i++)
			{
				bool flag = !thingList[i].def.destroyable;
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000440C File Offset: 0x0000260C
		private void TrySetCellAs(IntVec3 c, ThingDef thingDef, Rot4 thingRot, Map map, ThingDef stuffDef = null, TerrainDef terrainDef = null, PawnKindDef pawnKindDef = null, ThingDef itemDef = null, MapGeneratorBlueprintDef blueprint = null)
		{
			bool flag = !GenGrid.InBounds(c, map);
			if (flag)
			{
				Log.Warning("GenStep_CreateBlueprint: Invalid Target-Cell: cell is null or out of bounds.");
			}
			else
			{
				List<Thing> thingList = GridsUtility.GetThingList(c, map);
				for (int i = 0; i < thingList.Count; i++)
				{
					bool flag2 = !thingList[i].def.destroyable;
					if (flag2)
					{
						return;
					}
				}
				bool flag3 = terrainDef != null;
				if (flag3)
				{
					map.terrainGrid.SetTerrain(c, terrainDef);
				}
				else
				{
					bool flag4 = terrainDef == null && thingDef != null && stuffDef != null;
					if (flag4)
					{
						map.terrainGrid.SetTerrain(c, this.CorrespondingTileDef(stuffDef));
					}
				}
				bool flag5 = thingDef != null;
				if (flag5)
				{
					ThingDef stuff = stuffDef;
					bool flag6 = !thingDef.MadeFromStuff;
					if (flag6)
					{
						stuff = null;
					}
					Thing thing = ThingMaker.MakeThing(thingDef, stuff);
					bool flag7 = thingRot == Rot4.Invalid;
					if (flag7)
					{
						GenSpawn.Spawn(thing, c, map);
					}
					else
					{
						GenSpawn.Spawn(thing, c, map, thingRot);
					}
				}
				bool flag8 = blueprint == null;
				if (!flag8)
				{
					bool flag9 = itemDef != null && blueprint.itemSpawnChance / 100f > Rand.Value;
					if (flag9)
					{
						List<string> source = new List<string>
						{
							"Steel",
							"Steel"
						};
						ThingDef stuffDef2 = DefDatabase<ThingDef>.GetNamedSilentFail(source.RandomElement<string>());
						bool flag10 = !itemDef.MadeFromStuff;
						if (flag10)
						{
							stuffDef2 = null;
						}
						Thing thing2 = this.TryGetTreasure(itemDef, stuffDef2);
						thing2 = GenSpawn.Spawn(thing2, c, map);
						thing2.SetForbidden(true, true);
					}
					bool flag11 = pawnKindDef != null && blueprint.pawnSpawnChance / 100f > Rand.Value;
					if (flag11)
					{
						bool flag12 = this.faction == null;
						if (flag12)
						{
							this.faction = Find.FactionManager.FirstFactionOfDef(blueprint.factionDef);
						}
						bool flag13 = this.faction == null;
						if (flag13)
						{
							switch (blueprint.factionSelection)
							{
							case FactionSelection.none:
							{
								this.faction = Find.FactionManager.AllFactions.RandomElementByWeight((Faction fac) => fac.def.settlementGenerationWeight);
								bool flag14 = this.faction == null;
								if (flag14)
								{
									this.faction = Faction.OfMechanoids;
								}
								break;
							}
							case FactionSelection.hostile:
							{
								this.faction = (from fac in Find.FactionManager.AllFactions
								where fac.HostileTo(Faction.OfPlayer)
								select fac).RandomElementByWeight((Faction fac) => 101f - fac.def.settlementGenerationWeight);
								bool flag15 = this.faction == null;
								if (flag15)
								{
									this.faction = Faction.OfMechanoids;
								}
								break;
							}
							case FactionSelection.friendly:
							{
								this.faction = (from fac in Find.FactionManager.AllFactions
								where !fac.HostileTo(Faction.OfPlayer) && fac.PlayerGoodwill > 0f && fac != Faction.OfPlayer
								select fac).RandomElementByWeight((Faction fac) => 101f - fac.def.settlementGenerationWeight);
								bool flag16 = this.faction == null;
								if (flag16)
								{
									this.faction = Find.FactionManager.AllFactions.RandomElementByWeight((Faction fac) => fac.def.settlementGenerationWeight);
								}
								break;
							}
							}
						}
						Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, this.faction);
						pawn.mindState.Active = false;
						pawn = (GenSpawn.Spawn(pawn, c, map) as Pawn);
						bool flag17 = pawn != null;
						if (flag17)
						{
							bool flag18 = this.allSpawnedPawns == null;
							if (flag18)
							{
								this.allSpawnedPawns = new List<Pawn>();
							}
							this.allSpawnedPawns.Add(pawn);
						}
					}
				}
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000047E8 File Offset: 0x000029E8
		private Thing TryGetTreasure(ThingDef treasureDef, ThingDef stuffDef)
		{
			bool flag = treasureDef == null;
			Thing result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Thing thing = ThingMaker.MakeThing(treasureDef, stuffDef);
				CompQuality compQuality = thing.TryGetComp<CompQuality>();
				bool flag2 = compQuality != null;
				if (flag2)
				{
					compQuality.SetQuality(QualityUtility.AllQualityCategories.RandomElement(), ArtGenerationContext.Outsider);
				}
				bool flag3 = thing.def.stackLimit > 1;
				if (flag3)
				{
					thing.stackCount = Rand.RangeInclusive(1, thing.def.stackLimit);
				}
				bool flag4 = thing.stackCount == 1;
				if (flag4)
				{
					thing.HitPoints = Rand.RangeInclusive(40, thing.MaxHitPoints);
				}
				result = thing;
			}
			return result;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000488C File Offset: 0x00002A8C
		protected ThingDef RandomWallStuff()
		{
			return (from def in DefDatabase<ThingDef>.AllDefs
			where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f && def.BaseMarketValue / def.VolumePerUnit < 15f
			select def).RandomElement<ThingDef>();
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000048CC File Offset: 0x00002ACC
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

		// Token: 0x04000005 RID: 5
		public readonly IntRange ruinOffsetHorizontalRange = new IntRange(5, 15);

		// Token: 0x04000006 RID: 6
		public readonly IntRange ruinOffsetVerticalRange = new IntRange(5, 15);

		// Token: 0x04000007 RID: 7
		public IntRange ruinDistanceRange = new IntRange(4, 20);

		// Token: 0x04000008 RID: 8
		public IntRange ruinCountRange = new IntRange(3, 8);

		// Token: 0x04000009 RID: 9
		private int ruinCountDown;

		// Token: 0x0400000A RID: 10
		public IntRange villageCountRange = new IntRange(1, 1);

		// Token: 0x0400000B RID: 11
		private List<IntVec3> usedCells = new List<IntVec3>();

		// Token: 0x0400000C RID: 12
		private ThingDef selectedWallStuff;

		// Token: 0x0400000D RID: 13
		private Faction faction;

		// Token: 0x0400000E RID: 14
		private List<Pawn> allSpawnedPawns;
	}
}
