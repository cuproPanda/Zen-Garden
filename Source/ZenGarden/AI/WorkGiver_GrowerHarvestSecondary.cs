using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

  public class WorkGiver_GrowerHarvestSecondary : WorkGiver_Scanner {

    public override PathEndMode PathEndMode {
      get {
        return PathEndMode.ClosestTouch;
      }
    }


    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
      return pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Plant)).Where(p => p is PlantWithSecondary);
    }


    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
      if (t.def.category != ThingCategory.Plant) {
        return null;
      }

      if (!(t is PlantWithSecondary)) {
        return null;
      }

      PlantWithSecondary plant = (PlantWithSecondary)t;

      if (!pawn.CanReserve(plant)) {
        return null;
      }

      if (plant.IsForbidden(pawn)) {
        return null;
      }

      if (plant.IsBurning()) {
        return null;
      }

      foreach (Designation current in pawn.Map.designationManager.AllDesignationsOn(plant)) {
        if (current.def == DefDatabase<DesignationDef>.GetNamed("ZEN_Designator_PlantsHarvestSecondary")) {
          Job result;
          if (!plant.Sec_HarvestableNow) {
            result = null;
            return result;
          }
          result = new Job(DefDatabase<JobDef>.GetNamed("ZEN_PlantsHarvestSecondary"), plant);
          return result;
        }
      }
      return null;
    }
  }
}
