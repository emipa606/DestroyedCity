using System;
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
		public override int SeedPart
		{
			get
			{
				return 1158116085;
			}
		}

		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams genStepParams, int stackCount = 1)
		{
			bool flag = this.mapCenterBlueprintUsed;
			if (!flag)
			{
				MapGeneratorBlueprintDef mapGeneratorBlueprintDef = DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading.RandomElementByWeight((MapGeneratorBlueprintDef b) => b.chance);
				bool flag2 = !mapGeneratorBlueprintDef.mapCenterBlueprint && mapGeneratorBlueprintDef.pawnLegend != null && mapGeneratorBlueprintDef.pawnLegend.Count > 0;
				if (flag2)
				{
					IntVec2 intVec = new IntVec2(map.Size.x / 2, map.Size.z / 2);
					CellRect cellRect = new CellRect(intVec.x, intVec.z, 1, 1);
					cellRect = cellRect.ExpandedBy(20);
					bool flag3 = cellRect.Contains(loc);
					if (flag3)
					{
						mapGeneratorBlueprintDef = (from b in DefDatabase<MapGeneratorBlueprintDef>.AllDefsListForReading
						where b.pawnLegend == null || b.pawnLegend.Count == 0
						select b).RandomElementByWeight((MapGeneratorBlueprintDef b) => b.chance);
					}
				}
				bool flag4 = mapGeneratorBlueprintDef == null;
				if (!flag4)
				{
					bool mapCenterBlueprint = mapGeneratorBlueprintDef.mapCenterBlueprint;
					if (mapCenterBlueprint)
					{
						loc = new IntVec3(map.Center.x - mapGeneratorBlueprintDef.size.x / 2, map.Center.y, map.Center.z - mapGeneratorBlueprintDef.size.z / 2);
						this.mapCenterBlueprintUsed = true;
					}
					this.ScatterBlueprintAt(loc, mapGeneratorBlueprintDef, map, ref this.selectedWallStuff);
					this.selectedWallStuff = null;
				}
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000021F0 File Offset: 0x000003F0
		protected override bool CanScatterAt(IntVec3 loc, Map map)
		{
			return base.CanScatterAt(loc, map) && GenGrid.SupportsStructureType(loc, map, TerrainAffordanceDefOf.Heavy);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002218 File Offset: 0x00000418
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

		// Token: 0x06000004 RID: 4 RVA: 0x00002428 File Offset: 0x00000628
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

		// Token: 0x06000005 RID: 5 RVA: 0x000027E8 File Offset: 0x000009E8
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

		// Token: 0x06000006 RID: 6 RVA: 0x000028A4 File Offset: 0x00000AA4
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

		// Token: 0x06000007 RID: 7 RVA: 0x0000291C File Offset: 0x00000B1C
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

		// Token: 0x06000008 RID: 8 RVA: 0x00002994 File Offset: 0x00000B94
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

		// Token: 0x06000009 RID: 9 RVA: 0x00002A14 File Offset: 0x00000C14
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

		// Token: 0x0600000A RID: 10 RVA: 0x00002A8C File Offset: 0x00000C8C
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

		// Token: 0x0600000B RID: 11 RVA: 0x00002B04 File Offset: 0x00000D04
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

		// Token: 0x0600000C RID: 12 RVA: 0x00002B5C File Offset: 0x00000D5C
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

		// Token: 0x0600000D RID: 13 RVA: 0x00002BAC File Offset: 0x00000DAC
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
						bool isApparel = itemDef.IsApparel;
						ThingDef thingDef2;
						if (isApparel)
						{
							int num = 0;
							for (;;)
							{
								thingDef2 = DefDatabase<ThingDef>.GetRandom();
								bool flag10 = thingDef2.IsStuff && thingDef2.stuffCategories.Contains(StuffCategoryDefOf.Fabric);
								if (flag10)
								{
									break;
								}
								num++;
								bool flag11 = num > 100;
								if (flag11)
								{
									goto Block_17;
								}
							}
							goto IL_18F;
							Block_17:
							thingDef2 = DefDatabase<ThingDef>.GetNamedSilentFail("Synthread");
							IL_18F:;
						}
						else
						{
							List<string> source = new List<string>
							{
								"Steel",
								"Steel",
								"Steel",
								"Steel",
								"Steel",
								"Plasteel"
							};
							thingDef2 = DefDatabase<ThingDef>.GetNamedSilentFail(source.RandomElement<string>());
						}
						bool flag12 = !itemDef.MadeFromStuff;
						if (flag12)
						{
							thingDef2 = null;
						}
						Thing thing2 = this.TryGetTreasure(itemDef, thingDef2);
						thing2 = GenSpawn.Spawn(thing2, c, map);
						bool flag13 = thing2.TryGetComp<CompForbiddable>() != null;
						if (flag13)
						{
							thing2.SetForbidden(true, true);
						}
						Hive hive = thing2 as Hive;
						bool flag14 = hive != null;
						if (flag14)
						{
							hive.CompDormant.WakeUp();
						}
					}
					bool flag15 = pawnKindDef != null && blueprint.pawnSpawnChance / 100f > Rand.Value;
					if (flag15)
					{
						bool flag16 = this.faction == null;
						if (flag16)
						{
							this.faction = Find.FactionManager.FirstFactionOfDef(blueprint.factionDef);
						}
						bool flag17 = this.faction == null;
						if (flag17)
						{
							switch (blueprint.factionSelection)
							{
							case FactionSelection.none:
							{
								this.faction = Find.FactionManager.AllFactions.RandomElementByWeight((Faction fac) => fac.def.settlementGenerationWeight);
								bool flag18 = this.faction == null;
								if (flag18)
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
								bool flag19 = this.faction == null;
								if (flag19)
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
								bool flag20 = this.faction == null;
								if (flag20)
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
						bool flag21 = pawn != null;
						if (flag21)
						{
							bool flag22 = this.allSpawnedPawns == null;
							if (flag22)
							{
								this.allSpawnedPawns = new List<Pawn>();
							}
							this.allSpawnedPawns.Add(pawn);
						}
					}
				}
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00003054 File Offset: 0x00001254
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
					thing.HitPoints = Rand.RangeInclusive((int)((double)thing.MaxHitPoints * 0.4), thing.MaxHitPoints);
				}
				result = thing;
			}
			return result;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00003108 File Offset: 0x00001308
		protected ThingDef RandomWallStuff()
		{
			return (from def in DefDatabase<ThingDef>.AllDefs
			where def.IsStuff && def.stuffProps.CanMake(ThingDefOf.Wall) && def.BaseFlammability < 0.5f && def.BaseMarketValue / def.VolumePerUnit < 15f
			select def).RandomElement<ThingDef>();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00003148 File Offset: 0x00001348
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

		// Token: 0x04000001 RID: 1
		private ThingDef selectedWallStuff;

		// Token: 0x04000002 RID: 2
		private Faction faction;

		// Token: 0x04000003 RID: 3
		private List<Pawn> allSpawnedPawns;

		// Token: 0x04000004 RID: 4
		private bool mapCenterBlueprintUsed;
	}
}
