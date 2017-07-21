using System;

using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {
	[HarmonyPatch(typeof(WorkGiver_GrowerHarvest))]
	[HarmonyPatch("HasJobOnCell")]
	[HarmonyPatch(new Type[] {typeof(Pawn), typeof(IntVec3)})]
	public class WorkGiver_GrowerHarvest_HasJobOnCell {

		static bool Prefix (WorkGiver_GrowerHarvest __instance, Pawn pawn, IntVec3 c, ref bool __result) {
			Plant plant = c.GetPlant(pawn.Map);
			// If this plant has a secondary resource and is in an orchard zone, don't chop it down unless it's designated
			if ((plant is PlantWithSecondary) && pawn.Map.zoneManager.ZoneAt(c) is Zone_Orchard) {
				if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.CutPlant) == null) {
					__result = false;
				}
			}
			else {
				__result = (
					plant != null && 
					!plant.IsForbidden(pawn) && 
					plant.def.plant.Harvestable && 
					plant.LifeStage == PlantLifeStage.Mature && 
					pawn.CanReserve(plant, 1, -1, null, false)
			  );
			}
			
			return __result;
		}
	}
}
