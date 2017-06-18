using RimWorld;
using Verse;

namespace ZenGarden {
  [StaticConstructorOnStartup]
  public class Building_Fountain : Building {

    private TickManager tickMan;
    private CompPowerTrader powerComp;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);
      tickMan = Find.TickManager;
      powerComp = GetComp<CompPowerTrader>();
      Rotation = Rot4.North;
    }


    public override void Tick() {
      base.Tick();
      if (tickMan.TicksGame % 50 == 0) {
        if (powerComp.PowerOn) {
          if (Rotation == Rot4.North) {
            Rotation = Rot4.East;
          }
          else if (Rotation == Rot4.East) {
            Rotation = Rot4.South;
          }
          else if (Rotation == Rot4.South) {
            Rotation = Rot4.East;
          }
        }
        else {
          Rotation = Rot4.North;
        }
        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
      }
    }
  }
}
