using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace ZenGarden {

  internal class JobDriver_SitAtScenicBench : JobDriver {

    private const TargetIndex BenchInd = TargetIndex.A;
    private float surroundingBeauty;


    protected override IEnumerable<Toil> MakeNewToils() {
      // Set fail conditions
      this.FailOnBurningImmobile(BenchInd);
      this.FailOnDespawnedNullOrForbidden(BenchInd);

      // Reference the tick manager so Find isn't constantly called
      TickManager tickMan = Find.TickManager;

      // Create a new cellRect to get random look spots
      CellRect cellRect = new CellRect() {
        minX = (TargetA.Cell.x - 5),
        maxX = (TargetA.Cell.x + 5),
        minZ = (TargetA.Cell.z - 5),
        maxZ = (TargetA.Cell.z + 5),
      };

      // Reserve the bench, go to it, then sit down
      yield return Toils_Reserve.Reserve(BenchInd);
      yield return Toils_Goto.GotoCell(BenchInd, PathEndMode.OnCell);

      // Get surrounding beauty
      surroundingBeauty = JoyFromSurroundings();

      // Set up the toil
      Toil sitAtBench = new Toil();
      JoyKindDef joyKind = DefDatabase<JobDef>.GetNamed("ZEN_SitAtScenicBench").joyKind;
      sitAtBench.socialMode = RandomSocialMode.Normal;
      sitAtBench.tickAction = () => {
        // TODO: Test this value, max joy per hour is 30
        pawn.needs.joy.GainJoy(Mathf.Min(surroundingBeauty * 0.0004f, 0.012f), joyKind);

        // Occasionally look in a different direction, observing the surroundings
        if (tickMan.TicksGame % 250 == 0) {
          pawn.Drawer.rotator.FaceCell(cellRect.RandomCell);
        }
      };
      sitAtBench.defaultCompleteMode = ToilCompleteMode.Delay;
      sitAtBench.defaultDuration = CurJob.def.joyDuration;
      sitAtBench.AddFinishAction(() => {
        Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ZEN_EnjoyedScenery"));
        pawn.needs.mood.thoughts.memories.TryGainMemory(memory);
      });

      // Sit at the bench, enjoying the scenery
      yield return sitAtBench;
    }


    private float JoyFromSurroundings() {
      Thing bench = TargetA.Thing;
      float beauty = 0;

      RegionTraverser.BreadthFirstTraverse(bench.GetRegion(), (Region from, Region r) => r.portal == null, delegate (Region r) {
        foreach (IntVec3 current in r.Cells) {
          if (current.InHorDistOf(bench.Position, 15f)) {
            beauty += BeautyUtility.CellBeauty(current, Map);
          }
        }
        return false;
      });
      return beauty;
    }
  }
}
