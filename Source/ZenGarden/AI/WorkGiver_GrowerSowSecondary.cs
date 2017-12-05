using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

	public class WorkGiver_GrowerSowSecondary : WorkGiver_Scanner {

		protected static ThingDef wantedPlantDef;

		public override PathEndMode PathEndMode {
			get {
				return PathEndMode.ClosestTouch;
			}
		}


		private ThingDef CalculateWantedPlantDef(IntVec3 c, Map map) {
			IPlantToGrowSettable plantToGrowSettable = c.GetPlantToGrowSettable(map);
			if (plantToGrowSettable == null) {
				return null;
			}
			return plantToGrowSettable.GetPlantDefToGrow();
		}


		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) {
			Danger maxDanger = pawn.NormalMaxDanger();
			wantedPlantDef = null;
			List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
			for (int z = 0; z < zonesList.Count; z++) {
				if (zonesList[z] is Zone_Orchard orchardZone) {
					if (orchardZone.cells.Count == 0) {
						Log.ErrorOnce("Orchard zone has 0 cells: " + orchardZone, -563487);
					}
					else if (ExtraRequirements(orchardZone, pawn)) {
						if (!orchardZone.ContainsStaticFire) {
							if (pawn.CanReach(orchardZone.Cells[0], PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn)) {
								for (int k = 0; k < orchardZone.cells.Count; k++) {
									yield return orchardZone.cells[k];
								}
								wantedPlantDef = null;
							}
						} 
					}
				}
			}
			wantedPlantDef = null;
		}


		private bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn) {
			if (!settable.CanAcceptSowNow()) {
				return false;
			}
			Zone_Orchard orchardZone = settable as Zone_Orchard;
			IntVec3 c;
			if (orchardZone != null) {
				if (!orchardZone.allowSow) {
					return false;
				}
				c = orchardZone.Cells[0];
			}
			else {
				c = ((Thing)settable).Position;
			}
			wantedPlantDef = CalculateWantedPlantDef(c, pawn.Map);
			return wantedPlantDef != null;
		}

		// Not synced with CanGiveJob
		public override Job JobOnCell(Pawn pawn, IntVec3 c) {
			if (c.IsForbidden(pawn)) {
				return null;
			}
			if (!GenPlant.GrowthSeasonNow(c, pawn.Map)) {
				return null;
			}
			if (wantedPlantDef == null) {
				wantedPlantDef = CalculateWantedPlantDef(c, pawn.Map);
				if (wantedPlantDef == null) {
					return null;
				}
			}
			List<Thing> thingList = c.GetThingList(pawn.Map);
			for (int i = 0; i < thingList.Count; i++) {
				Thing thing = thingList[i];
				if (thing.def == wantedPlantDef) {
					return null;
				}
				if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction) {
					return null;
				}
			}
			Plant plant = c.GetPlant(pawn.Map);
			if (plant != null && plant.def.plant.blockAdjacentSow) {
				if (!pawn.CanReserve(plant) || plant.IsForbidden(pawn)) {
					return null;
				}
				return new Job(JobDefOf.CutPlant, plant);
			}
			else {
				Thing thing2 = GenPlant.AdjacentSowBlocker(wantedPlantDef, c, pawn.Map);
				if (thing2 != null) {
					if (thing2 is Plant plant2 && pawn.CanReserve(plant2, 1, -1, null, false) && !plant2.IsForbidden(pawn)) {
						IPlantToGrowSettable plantToGrowSettable = plant2.Position.GetPlantToGrowSettable(plant2.Map);
						if (plantToGrowSettable == null || plantToGrowSettable.GetPlantDefToGrow() != plant2.def) {
							return new Job(JobDefOf.CutPlant, plant2);
						}
					}
					return null;
				}
				if (wantedPlantDef.plant.sowMinSkill > 0 && pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Growing).Level < wantedPlantDef.plant.sowMinSkill) {
					return null;
				}
				int j = 0;
				while (j < thingList.Count) {
					Thing thing3 = thingList[j];
					if (thing3.def.BlockPlanting) {
						if (!pawn.CanReserve(thing3, 1, -1, null, false)) {
							return null;
						}
						if (thing3.def.category == ThingCategory.Plant) {
							if (!thing3.IsForbidden(pawn)) {
								return new Job(JobDefOf.CutPlant, thing3);
							}
							return null;
						}
						else {
							if (thing3.def.EverHaulable) {
								return HaulAIUtility.HaulAsideJobFor(pawn, thing3);
							}
							return null;
						}
					}
					else {
						j++;
					}
				}
				if (!wantedPlantDef.CanEverPlantAt(c, pawn.Map) || !GenPlant.GrowthSeasonNow(c, pawn.Map) || !pawn.CanReserve(c, 1, -1, null, false)) {
					return null;
				}
				return new Job(ZenDefOf.ZEN_PlantsSowSecondary, c) {
					plantDefToSow = wantedPlantDef
				};
			}
		}
	}
}
