using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

  public class JobDriver_PlantsHarvestSecondary : JobDriver {

    private const TargetIndex PlantInd = TargetIndex.A;
    private const TargetIndex ThingToHaulInd = TargetIndex.B;
    private const TargetIndex StorageCellInd = TargetIndex.C;
    private const int Duration = 1000;

    protected PlantWithSecondary Plant {
      get {
        return (PlantWithSecondary)CurJob.GetTarget(TargetIndex.A).Thing;
      }
    }

    protected Thing Resource {
      get {
        return CurJob.GetTarget(TargetIndex.B).Thing;
      }
    }


    protected override IEnumerable<Toil> MakeNewToils() {

      // Verify plant validity
      this.FailOn(() => !Plant.Sec_HarvestableNow);
      this.FailOnDestroyedNullOrForbidden(PlantInd);
      this.FailOnBurningImmobile(PlantInd);

      // Reserve plant
      yield return Toils_Reserve.Reserve(PlantInd);

      // Go to the plant
      yield return Toils_Goto.GotoThing(PlantInd, PathEndMode.ClosestTouch);

      // Add delay for collecting resource from plant, if it is ready
      yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(PlantInd).WithProgressBarToilDelay(PlantInd).PlaySustainerOrSound(SoundDef.Named("Harvest_Standard"));

      // Collect resource
      Toil collect = new Toil();
      collect.initAction = () => {
        Thing resource = Plant.CollectSecondaryThing();

        // Remove the designation on the plant
        pawn.Map.designationManager.RemoveAllDesignationsOn(Plant);

        GenPlace.TryPlaceThing(resource, pawn.Position, Map, ThingPlaceMode.Near);
        StoragePriority storagePriority = HaulAIUtility.StoragePriorityAtFor(resource.Position, resource);

        // Gain experience from gathering this resource (done after completion to prevent drafting exploit)
        if (pawn.skills != null && !pawn.skills.GetSkill(SkillDefOf.Growing).LearningSaturatedToday) {
          pawn.skills.GetSkill(SkillDefOf.Growing).Learn(100f, true); 
        }

        // Ref the cell for the following out parameter
        IntVec3 c;

        // Try to find a suitable storage spot for the resource
        if (StoreUtility.TryFindBestBetterStoreCellFor(resource, pawn, Map, storagePriority, pawn.Faction, out c)) {
          CurJob.SetTarget(TargetIndex.B, resource);
          CurJob.count = resource.stackCount;
          CurJob.SetTarget(TargetIndex.C, c);
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Incompletable);
        }
      };
      collect.defaultCompleteMode = ToilCompleteMode.Instant;
      collect.PlaySoundAtEnd(SoundDef.Named("Harvest_Standard_Finish"));
      yield return collect;

      // Reserve the resource
      yield return Toils_Reserve.Reserve(ThingToHaulInd);

      // Reserve the storage cell
      yield return Toils_Reserve.Reserve(StorageCellInd);

      // Go to the resource
      yield return Toils_Goto.GotoThing(ThingToHaulInd, PathEndMode.ClosestTouch);

      // Pick up the resource
      yield return Toils_Haul.StartCarryThing(ThingToHaulInd);

      // Carry the resource to the storage cell, then place it down
      Toil carry = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
      yield return carry;
      yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carry, true);

      // End the current job
      yield break;
    }
  }
}
