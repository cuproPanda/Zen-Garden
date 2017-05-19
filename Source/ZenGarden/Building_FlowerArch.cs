using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {
  [StaticConstructorOnStartup]
  internal class Building_FlowerArch : Building {

    private static Graphic GraphicPlain = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/FlowerArch/FlowerArch_Plain",       ShaderDatabase.DefaultShader, new Vector2(2, 2), Color.white);
    private static Graphic GraphicVines = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/FlowerArch/FlowerArch_Vines",       ShaderDatabase.DefaultShader, new Vector2(2, 2), Color.white);
    private static Graphic GraphicBlooming = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/FlowerArch/FlowerArch_Blooming", ShaderDatabase.DefaultShader, new Vector2(2, 2), Color.white);
    private static Graphic GraphicFrozen = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/FlowerArch/FlowerArch_Frozen",     ShaderDatabase.DefaultShader, new Vector2(2, 2), Color.white);

    public override Graphic Graphic {
      get {
        if (Map.weatherManager.SnowRate > 0) {
          return GraphicFrozen;
        }
        if (Temperature > 5f) {
          if (Season == Season.Spring || Season == Season.Summer) {
            return GraphicBlooming;
          }
          return GraphicVines;
        }
        return GraphicPlain;
      }
    }

    public float Temperature {
      get {
        float num;
        if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out num)) {
          return 1f;
        }
        return num;
      }
    }

    private Season Season {
      get {
        return GenLocalDate.Season(Map);
      }
    }
  }
}
