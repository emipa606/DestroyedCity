using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MapGenerator
{
	// Token: 0x02000007 RID: 7
	public class RectTrigger_UnfogArea : RectTrigger
	{
		// Token: 0x06000031 RID: 49 RVA: 0x00005836 File Offset: 0x00003A36
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.destroyIfUnfogged, "destroyIfUnfogged", true, false);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00005854 File Offset: 0x00003A54
		public override void Tick()
		{
			bool flag = this.IsHashIntervalTick(60);
			if (flag)
			{
				for (int i = base.Rect.minZ; i <= base.Rect.maxZ; i++)
				{
					for (int j = base.Rect.minX; j <= base.Rect.maxX; j++)
					{
						IntVec3 intVec = new IntVec3(j, 0, i);
						List<Thing> thingList = GridsUtility.GetThingList(intVec, Map);
						for (int k = 0; k < thingList.Count; k++)
						{
							bool flag2 = thingList[k].def.category == ThingCategory.Pawn && thingList[k].def.race.intelligence == Intelligence.Humanlike && thingList[k].Faction == Faction.OfPlayer;
							if (flag2)
							{
								this.ActivatedBy((Pawn)thingList[k]);
							}
						}
					}
					bool flag3 = this.destroyIfUnfogged && this.IsRectUnfogged(Map);
					if (flag3)
					{
						this.Destroy(DestroyMode.Vanish);
						break;
					}
				}
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00005990 File Offset: 0x00003B90
		public new void ActivatedBy(Pawn p)
		{
			Find.SignalManager.SendSignal(new Signal(this.signalTag, p.Named("SUBJECT")));
			FloodFillerFog.FloodUnfog(p.Position, p.Map);
			GridsUtility.GetRoom(p.Position, p.Map).Notify_RoomShapeOrContainedBedsChanged();
			bool flag2 = !this.destroyIfUnfogged && !base.Destroyed;
			if (flag2)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00005A28 File Offset: 0x00003C28
		private void UnfogRect(Map map)
		{
			HashSet<Room> hashSet = new HashSet<Room>();
			for (int i = base.Rect.minZ; i <= base.Rect.maxZ; i++)
			{
				for (int j = base.Rect.minX; j <= base.Rect.maxX; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					map.fogGrid.Unfog(intVec);
					hashSet.Add(GridsUtility.GetRoom(intVec, map));
				}
			}
			foreach (Room room in hashSet)
			{
				room.Notify_RoomShapeOrContainedBedsChanged();
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00005B00 File Offset: 0x00003D00
		private bool IsRectUnfogged(Map map)
		{
			foreach (IntVec3 c in base.Rect.Cells)
			{
				bool flag = map.fogGrid.IsFogged(c);
				if (flag)
				{
					return false;
				}
			}
			return true;
		}
	}
}
