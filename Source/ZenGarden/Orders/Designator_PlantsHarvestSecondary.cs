using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {

  public class Designator_PlantsHarvestSecondary : Designator {

    private DesignationDef designationDef;

		public override int DraggableDimensions => 2;


    public Designator_PlantsHarvestSecondary() {
      defaultLabel = "ZEN_DesignatorHarvestSecondary".Translate();
      defaultDesc = "ZEN_DesignatorHarvestSecondaryDesc".Translate();
      icon = ContentFinder<Texture2D>.Get("Cupro/UI/Designations/HarvestSecondary");
      soundDragSustain = SoundDefOf.DesignateDragStandard;
      soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
      useMouseIcon = true;
      soundSucceeded = SoundDefOf.DesignateHarvest;
      designationDef = DefDatabase<DesignationDef>.GetNamed("ZEN_Designator_PlantsHarvestSecondary");
    }
    

    public override AcceptanceReport CanDesignateThing(Thing t) {
      // If the thing isn't a plant
      if (t.def.plant == null) {
        return false;
      }

      // If the thing doesn't have a matching PlantWithSecondaryDef
      if (!(t is PlantWithSecondary)) {
        return "ZEN_MustDesignatePlantsWithSecondary".Translate();
      }
      // If the thing is already designated
      if (Map.designationManager.DesignationOn(t, designationDef) != null) {
        return false;
      }
      // If the secondary resource isn't harvestable
      PlantWithSecondary plant = t as PlantWithSecondary;
      if (!plant.Sec_HarvestableNow) {
        return "ZEN_MustDesignateHarvestableSecondary".Translate();
      }
      return true;
    }


    public override AcceptanceReport CanDesignateCell(IntVec3 c) {
      if (!c.InBounds(Map) || c.Fogged(Map)) {
        return false;
      }
			PlantWithSecondary plant = c.GetPlant(Map) as PlantWithSecondary;
      if (plant == null) {
        return "ZEN_MustDesignatePlantsWithSecondary".Translate();
      }
      AcceptanceReport result = CanDesignateThing(plant);
      if (!result.Accepted) {
        return result;
      }
      return true;
    }


    public override void DesignateSingleCell(IntVec3 c) {
      DesignateThing(c.GetPlant(Map));
    }


    public override void DesignateThing(Thing t) {
      Map.designationManager.RemoveAllDesignationsOn(t, false);
      Map.designationManager.AddDesignation(new Designation(t, designationDef));
    }


    public override void SelectedUpdate() {
      GenUI.RenderMouseoverBracket();
    }
  }
}
