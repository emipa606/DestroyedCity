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
            Scribe_Values.Look(ref destroyIfUnfogged, "destroyIfUnfogged", true);
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00005854 File Offset: 0x00003A54
        public override void Tick()
        {
            if (!this.IsHashIntervalTick(60))
            {
                return;
            }

            for (var i = Rect.minZ; i <= Rect.maxZ; i++)
            {
                for (var j = Rect.minX; j <= Rect.maxX; j++)
                {
                    var intVec = new IntVec3(j, 0, i);
                    var thingList = intVec.GetThingList(Map);
                    foreach (var thing in thingList)
                    {
                        if (thing.def.category == ThingCategory.Pawn &&
                            thing.def.race.intelligence == Intelligence.Humanlike &&
                            thing.Faction == Faction.OfPlayer)
                        {
                            ActivatedBy((Pawn) thing);
                        }
                    }
                }

                if (!destroyIfUnfogged || !IsRectUnfogged(Map))
                {
                    continue;
                }

                Destroy();
                break;
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00005990 File Offset: 0x00003B90
        private new void ActivatedBy(Pawn p)
        {
            Find.SignalManager.SendSignal(new Signal(signalTag, p.Named("SUBJECT")));
            FloodFillerFog.FloodUnfog(p.Position, p.Map);
            p.Position.GetRoom(p.Map).Notify_RoomShapeOrContainedBedsChanged();
            if (!destroyIfUnfogged && !Destroyed)
            {
                Destroy();
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00005A28 File Offset: 0x00003C28
        private void UnfogRect(Map map)
        {
            var hashSet = new HashSet<Room>();
            for (var i = Rect.minZ; i <= Rect.maxZ; i++)
            {
                for (var j = Rect.minX; j <= Rect.maxX; j++)
                {
                    var intVec = new IntVec3(j, 0, i);
                    map.fogGrid.Unfog(intVec);
                    hashSet.Add(intVec.GetRoom(map));
                }
            }

            foreach (var room in hashSet)
            {
                room.Notify_RoomShapeOrContainedBedsChanged();
            }
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00005B00 File Offset: 0x00003D00
        private bool IsRectUnfogged(Map map)
        {
            foreach (var c in Rect.Cells)
            {
                if (map.fogGrid.IsFogged(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}