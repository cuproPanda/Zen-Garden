using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {

  public class Designator_ZoneAdd_Orchard : Designator_ZoneAdd {

    protected override string NewZoneLabel {
      get {
        return "OrchardZone".Translate();
      }
    }


    public Designator_ZoneAdd_Orchard() {
      zoneTypeToPlace = typeof(Zone_Orchard);
      defaultLabel = "ZEN_OrchardZone".Translate();
      defaultDesc = "ZEN_DesignatorOrchardZoneDesc".Translate();
      icon = ContentFinder<Texture2D>.Get("Cupro/UI/Designations/ZoneCreate_Orchard", true);
    }


    protected override Zone MakeNewZone() {
      return new Zone_Orchard(Find.VisibleMap.zoneManager);
    }


    public override AcceptanceReport CanDesignateCell(IntVec3 c) {
      if (!base.CanDesignateCell(c).Accepted) {
        return false;
      }
      if (Map.fertilityGrid.FertilityAt(c) < ThingDefOf.PlantPotato.plant.fertilityMin) {
        return false;
      }
      return true;
    }
  }
}
