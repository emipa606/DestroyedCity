using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MapGenerator;

public class RectTrigger_UnfogArea : RectTrigger
{
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref destroyIfUnfogged, "destroyIfUnfogged", true);
    }

    protected override void Tick()
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
                        ActivatedBy((Pawn)thing);
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

    private new void ActivatedBy(Pawn p)
    {
        Find.SignalManager.SendSignal(new Signal(signalTag, p.Named("SUBJECT")));
        FloodFillerFog.FloodUnfog(p.Position, p.Map);
        p.Position.GetRoom(p.Map).Notify_RoomShapeChanged();
        if (!destroyIfUnfogged && !Destroyed)
        {
            Destroy();
        }
    }

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
            room.Notify_RoomShapeChanged();
        }
    }

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