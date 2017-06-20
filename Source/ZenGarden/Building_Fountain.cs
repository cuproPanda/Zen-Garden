using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {
  [StaticConstructorOnStartup]
  public class Building_Fountain : Building {

    public static Graphic[] AnimFrames;
    public CompPowerTrader powerComp;
    public Graphic animOff;

    private TickManager tickMan;
    private Graphic currFrame;
    private int frameLerp = 0;
    private int FrameCount = 4;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);
      tickMan = Find.TickManager;
      powerComp = GetComp<CompPowerTrader>();
      LongEventHandler.ExecuteWhenFinished(GetGraphicArray);
    }


    public void GetGraphicArray() {
      //Creating frame array
      AnimFrames = new Graphic_Single[FrameCount];
      for (int i = 0; i < FrameCount; i++) {
        AnimFrames[i] = GraphicDatabase.Get<Graphic_Single>("Cupro/Anim/FountainWater" + (i + 1), ShaderDatabase.Transparent, def.graphicData.drawSize, new Color(1f, 1f, 1f, 0.65f));
      }
      if (AnimFrames.Count() > 0) {
        currFrame = AnimFrames.FirstOrDefault();
      }
      animOff = GraphicDatabase.Get<Graphic_Single>("Cupro/Anim/FountainWater0", ShaderDatabase.Transparent, def.graphicData.drawSize, new Color(1f, 1f, 1f, 0.65f));
    }


    public override void Tick() {
      base.Tick();

      if (tickMan.TicksGame % 35 == 0 && powerComp.PowerOn) {
        frameLerp++;
        if (frameLerp >= FrameCount) {
          frameLerp = 0;
        }
        currFrame = AnimFrames[frameLerp];
      }
    }


    public override void Draw() {
      base.Draw();
      if (currFrame != null && animOff != null) {
        Matrix4x4 matrix = default(Matrix4x4);
        Vector3 s = new Vector3(def.graphicData.drawSize.x, 9f, def.graphicData.drawSize.y); //Size and altitude
        Vector3 x = new Vector3(0f, 0f, 0f); // This is a offset for drawing position
        matrix.SetTRS(DrawPos + x + Altitudes.AltIncVect, Rotation.AsQuat, s);
        if (powerComp.PowerOn && AnimFrames.Count() > 0) {
          Graphics.DrawMesh(MeshPool.plane10, matrix, currFrame.MatAt(Rotation), 0);
        }
        else {
          Graphics.DrawMesh(MeshPool.plane10, matrix, animOff.MatAt(Rotation), 0);
        }
      }
    }
  }
}
