using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

  internal class JoyGiver_SitAtScenicBench : JoyGiver {


    public override Job TryGiveJob(Pawn pawn) {
      // If the weather outside is frightful, only look for benches indoors
      bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn, null);

      // If the pawn doesn't have the required needs for some reason, fail
      if ((pawn.needs == null || pawn.needs.joy == null || pawn.needs.mood == null || pawn.needs.mood.thoughts == null)) {
        return null;
      }

      // Find a bench matching the correct criteria
      IEnumerable<Thing> bench = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(delegate (Thing b) {
        return (b.def == ThingDef.Named("ZEN_ScenicBench") && b.Faction == Faction.OfPlayer && !b.IsForbidden(pawn) 
                && (allowedOutside || b.Position.Roofed(b.Map)) && pawn.CanReserveAndReach(b, PathEndMode.Touch, Danger.None, 1, -1, null, false));
      });
      Thing t;
      if (!bench.TryRandomElementByWeight(delegate (Thing x) {
        float lengthHorizontal = (x.Position - pawn.Position).LengthHorizontal;
        return Mathf.Max(150f - lengthHorizontal, 5f);
      }, out t)) {
        return null;
      }
      return new Job(def.jobDef, t);
    }
  }
}
