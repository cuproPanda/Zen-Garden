using Verse;

namespace ZenGarden {

  internal class PlaceWorker_OnRakeableGravel : PlaceWorker {


    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null) {

      // Don't allow placing outside of the map
      if (!loc.InBounds(Map)) {
        return false;
      }

      // Only alloy placing on unraked gravel
      TerrainDef terrain = loc.GetTerrain(Map);
      if (terrain == null) {
        return false;
      }
      if (terrain.tags == null) {
        return "ZEN_MustPlaceHeadOnUnrakedGravel".Translate();
      }
      if (!terrain.tags.Contains("UnrakedGravel")) {
        return "ZEN_MustPlaceHeadOnUnrakedGravel".Translate();
      }
      return true;
    }
  }
}
