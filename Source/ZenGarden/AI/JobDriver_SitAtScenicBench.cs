using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

  internal class JobDriver_SitAtScenicBench : JobDriver {

    private const TargetIndex BenchInd = TargetIndex.A;
    private float surroundingBeauty;


		public override bool TryMakePreToilReservations() {
			return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0);
		}


		protected override IEnumerable<Toil> MakeNewToils() {
      // Set fail conditions
      this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);

      // Reference the tick manager so Find isn't constantly called
      TickManager tickMan = Find.TickManager;

      // Go to the bench
      yield return Toils_Goto.GotoCell(BenchInd, PathEndMode.OnCell);

      // Set up the toil
      JobDef joyJob = DefDatabase<JobDef>.GetNamed("ZEN_SitAtScenicBench");
			Toil sitAtBench = new Toil() {
				socialMode = RandomSocialMode.Normal,
				initAction = () => { surroundingBeauty = BeautyUtility.AverageBeautyPerceptible(pawn.Position, Map); },
				tickAction = () => {
					pawn.needs.joy.GainJoy(joyJob.joyGainRate * 0.000144f, joyJob.joyKind);
					pawn.needs.joy.GainJoy(Mathf.Min(Mathf.Max(surroundingBeauty / 2f, 0.3f), 2.5f) * 0.000144f, joyJob.joyKind);
					// Gain comfort from sitting on the bench
					pawn.GainComfortFromCellIfPossible();

					// Occasionally look in a different direction, observing the surroundings
					if (tickMan.TicksGame % 250 == 0) {
						pawn.rotationTracker.FaceCell(GenAdj.RandomAdjacentCellCardinal(pawn.Position));
					}

					// End this job once the pawn has maxed out their joy
					if (pawn.needs.joy.CurLevel > 0.9999f) {
						pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = job.def.joyDuration
			};
			sitAtBench.AddFinishAction(() => {
        Thought_Memory memory;
        // If the scenery was very beautiful, give a better memory,
        if (surroundingBeauty > 5f) {
          memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ZEN_EnjoyedBeautifulScenery")); 
        }
        // or if the scenery was okay, give a normal memory,
        else if (surroundingBeauty > 0) {
          memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ZEN_EnjoyedScenery"));
        }
        // otherwise, give a bad memory
        else {
          memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ZEN_DidNotEnjoyScenery"));
        }
        pawn.needs.mood.thoughts.memories.TryGainMemory(memory);
      });

      // Sit at the bench, enjoying the scenery
      yield return sitAtBench;
    }


    private float AverageBeautyPerceptible(IntVec3 root, Map map) {
      List<Thing> tempCountedThings = new List<Thing>();
      float beauty = 0f;
      int cells = 0;
      BeautyUtility.FillBeautyRelevantCells(root, map);
      List<IntVec3> beautyCells = BeautyUtility.beautyRelevantCells;
      for (int i = 0; i < beautyCells.Count; i++) {
        // Get the beauty for this cell
        beauty += BeautyUtility.CellBeauty(beautyCells[i], map, tempCountedThings);
        // Add +1 beauty for any plant that doesn't already have beauty
        Plant plant = beautyCells[i].GetPlant(Map);
        if (plant != null && plant.GetStatValue(StatDefOf.Beauty) <= 0) {
          beauty += 1f;
        }
        // Add +1 beauty for any water cells
        if (beautyCells[i].GetTerrain(map).tags.Contains("Water")) {
          beauty += 1f;
        }
        // Add +2 beauty for moving water (rivers)
        if (beautyCells[i].GetTerrain(map).tags.Contains("River")) {
          beauty += 2f;
        }
        cells++;
      }
      // Divide beauty by the number of cells scanned to get the average
      beauty /= cells;
      return beauty;
    }
  }
}
