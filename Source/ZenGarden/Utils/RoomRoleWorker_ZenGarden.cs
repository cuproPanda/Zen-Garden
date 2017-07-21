using System.Collections.Generic;

using Verse;

namespace ZenGarden {

  public class RoomRoleWorker_ZenGarden : RoomRoleWorker {

    public override float GetScore(Room room) {
      int num = 0;
      List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;

      for (int i = 0; i < containedAndAdjacentThings.Count; i++) {
        Thing thing = containedAndAdjacentThings[i];
        if (thing.def.category == ThingCategory.Building) {
          if (thing.def.defName == "ZEN_BorderPath" || thing.def.defName == "ZEN_BorderPond" 
            || thing.def.defName == "ZEN_GravelCurve" || thing.def.defName == "ZEN_GravelHoriz" 
            || thing.def.defName == "ZEN_GravelVert" || thing.def.defName == "ZEN_Hedge") {
            num++;
          }
        }
      }
      return num;
    }
  }
}
