using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ZenGarden {

  public class JobDriver_PlantsHarvestSecondary : JobDriver {

    private const TargetIndex PlantInd = TargetIndex.A;
    private const int Duration = 250;
		private float workDone;

    protected PlantWithSecondary Plant {
      get {
        return (PlantWithSecondary)CurJob.GetTarget(TargetIndex.A).Thing;
      }
    }


    protected override IEnumerable<Toil> MakeNewToils() {
			foreach (Toil toil in Clipboard()) {
				yield return toil;
			}
			yield return Toils_General.RemoveDesignationsOnThing(TargetIndex.A, ZenDefOf.ZEN_Designator_PlantsHarvestSecondary);
    }


		private IEnumerable<Toil> Clipboard() {
			yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
			yield return Toils_Reserve.ReserveQueue(TargetIndex.A);
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
			Toil checkNextQueuedTarget = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, checkNextQueuedTarget);
			Toil cut = new Toil();
			cut.tickAction = delegate {
				Pawn actor = GetActor();
				if (actor.skills != null) {
					actor.skills.Learn(SkillDefOf.Growing, 0.11f);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				float num = statValue;
				workDone += num;
				if (workDone >= Duration) {
					if (Plant.def.plant.harvestedThingDef != null) {
						if (actor.RaceProps.Humanlike && Plant.def.plant.harvestFailable && Rand.Value > actor.GetStatValue(StatDefOf.PlantHarvestYield, true)) {
							Vector3 loc = (pawn.DrawPos + Plant.DrawPos) / 2f;
							MoteMaker.ThrowText(loc, Map, "TextMote_HarvestFailed".Translate(), 3.65f);
						}
						else {
							Thing thing = Plant.CollectSecondaryThing();
							if (actor.Faction != Faction.OfPlayer) {
								thing.SetForbidden(true, true);
							}
							GenPlace.TryPlaceThing(thing, actor.Position, Map, ThingPlaceMode.Near);
							actor.records.Increment(RecordDefOf.PlantsHarvested);

							// If there is a SeedsPlease seed, try to drop it
							if (Plant.seedDef != null) {
								int stack = Rand.RangeInclusive(-1, 1);
								if (pawn.skills != null) {
									stack += GenMath.RoundRandom(pawn.skills.GetSkill(SkillDefOf.Growing).Level / 8f);
								}
								if (stack > 0) {
									Thing seed = ThingMaker.MakeThing(Plant.seedDef);
									seed.stackCount = stack;
									GenPlace.TryPlaceThing(seed, pawn.Position, pawn.Map, ThingPlaceMode.Near);
								}
							}
						}
					}
					Plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
					workDone = 0f;
					ReadyForNextToil();
					return;
				}
			};
			cut.FailOn(() => !Plant.Sec_HarvestableNow);
			cut.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			cut.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			cut.defaultCompleteMode = ToilCompleteMode.Never;
			cut.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
			cut.WithProgressBar(TargetIndex.A, () => workDone / Duration, true, -0.5f);
			cut.PlaySustainerOrSound(() => Plant.def.plant.soundHarvesting);
			yield return cut;
			yield return checkNextQueuedTarget;
			yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.A, initExtractTargetFromQueue);
		}
  }
}
