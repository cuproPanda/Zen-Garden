using System.Collections.Generic;
using System.Text;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {
  [StaticConstructorOnStartup]
  public class PlantWithSecondary : Plant {

    // If there is currently a seed for SeedsPlease, and the mod is installed, list it.
    public ThingDef seedDef = null;

    // Def reference for the secondary thing
    private SecondaryResource secondaryDef;

    // Reference for the harvest designation
    private DesignationDef harvestDesignation;

    // Label for secondary thing. Used in case a special string is needed
    private string thingLabel;

    // The graphic while this plant is producing secondary things
    private Graphic bloomingGraphic;

    // Growth of the secondary thing
    private float sec_GrowthInt = -1f;
    public float Sec_Growth {
      get {
        return sec_GrowthInt;
      }
      set {
        sec_GrowthInt = value;
      }
    }

    private float Sec_GrowthPerTick {
      get {
        if (LifeStage == PlantLifeStage.Sowing || Resting || Sec_HarvestableNow) {
          return 0f;
        }
        float num = 1f / (60000f * secondaryDef.growDays);
        return num * GrowthRate;
      }
    }

    // This is harvestable when the secondary thing is fully grown and the parent plant is sufficiently grown
    // This is useful for times when the secondary thing grows much quicker than the parent plant
    // This also checks if there are limited growth seasons and limits harvesting to those seasons
    public bool Sec_HarvestableNow {
      get {
        // If DevMode is enabled, disregard other checks since there is a button for insta-growing
        if (Prefs.DevMode && sec_GrowthInt >= 0.9999f) {
          return true;
        }
        return sec_GrowthInt >= 0.9999f && growthInt >= secondaryDef.parentMinGrowth && GrowsThisSeason;
      }
    }

    // If the growth is limited to specific seasons, return whether the current season is acceptable
    public bool GrowsThisSeason {
      get {
        if (!Spawned || secondaryDef == null) {
          return false;
        }
        if (secondaryDef.limitedGrowSeasons == null || secondaryDef.limitedGrowSeasons.Count < 1) {
          return true;
        }
        if (secondaryDef.limitedGrowSeasons.Contains(GenLocalDate.Season(Map))) {
          return true;
        }
        return false;
      }
    }

    private string Sec_GrowthPercentString {
      get {
        return (sec_GrowthInt + 0.0001f).ToStringPercent();
      }
    }

    // Adjusted growth based on parent's growth, preventing the secondary item from being harvestable before the parent has matured enough
    private float AdjustedGrowth {
      get {
        if (secondaryDef == null) {
          return 0f;
        }
        return Mathf.Lerp(0f, 1f, growthInt / secondaryDef.parentMinGrowth);
      }
    }

    // Override the graphic, allowing this plant to potentially bloom
    public override Graphic Graphic {
      get {
        if (Sec_HarvestableNow && !LeaflessNow && bloomingGraphic != null) {
          return bloomingGraphic;
        }
        return base.Graphic;
      }
    }

		// Display additional info on the inspect window that isn't shown normally
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats {
			get {
				foreach (StatDrawEntry entry in base.SpecialDisplayStats) {
					yield return entry;
				}
				string seasons = string.Empty;
				bool conjugate = false;
				
				if (seasons.NullOrEmpty() || secondaryDef.limitedGrowSeasons.Count >= 4) {
					seasons = Static.DisplayStat_GrowsInAllSeasons;
				}
				else {
					for (int i = 0; i < secondaryDef.limitedGrowSeasons.Count; i++) {
						if (conjugate) {
							seasons += ", ";
						}
						conjugate = true;
						seasons += secondaryDef.limitedGrowSeasons[i].LabelCap();
					}
				}

				yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, thingLabel + " " + "GrowingTime".Translate().ToLower(), secondaryDef.growDays.ToString("0.##") + " " + "Days".Translate());
				yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, $"{thingLabel} {Static.DisplayStat_MinPlantGrowth}", secondaryDef.parentMinGrowth.ToStringPercent()) {
					overrideReportText = Static.DisplayStat_MinGrowthReport
				};
				yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, $"{thingLabel} {Static.DisplayStat_LimitedGrowSeasons}", seasons);
			}
		}


		public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref sec_GrowthInt, "secondaryGrowth", 0f);
    }


    public override string LabelMouseover {
      get {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(base.LabelMouseover);
        stringBuilder.Append(" - " + thingLabel + " (" + "PercentGrowth".Translate(new object[]
        {
            Sec_GrowthPercentString
        }));
        stringBuilder.Append(")");
        return stringBuilder.ToString().TrimEndNewlines();
      }
    }


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      if (def.GetModExtension<SecondaryResource>() == null) {
        Log.ErrorOnce($"Zen Garden:: Missing SecondaryResource DefModExtension for {def.defName}.", 316265143);
        return;
      }

      secondaryDef = def.GetModExtension<SecondaryResource>();

      // If a SeedsPlease seed exists, and SeedsPlease is installed, assign it
      string seed = secondaryDef.seedsPleaseSeedDef;
      if (!seed.NullOrEmpty() && DefDatabase<ThingDef>.GetNamed(seed, false) != null && ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "SeedsPlease")) {
        seedDef = DefDatabase<ThingDef>.GetNamed(seed);
      }

      harvestDesignation = Static.DesignationHarvestSecondary;

      // Allow the secondary thing to start grown similar to the parent plant
      if (sec_GrowthInt == -1f) {
        sec_GrowthInt = AdjustedGrowth;
      }

      if (secondaryDef.specialThingDefLabel.NullOrEmpty()) {
        thingLabel = secondaryDef.harvestedThingDef.LabelCap;
      }
      else {
        thingLabel = secondaryDef.specialThingDefLabel.CapitalizeFirst();
      }

      // Create the blooming graphic if there is one
      if (!secondaryDef.bloomingGraphicPath.NullOrEmpty()) {
        LongEventHandler.ExecuteWhenFinished(delegate {
          bloomingGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, secondaryDef.bloomingGraphicPath, def.graphic.Shader, def.graphicData.drawSize, def.graphicData.color, def.graphicData.colorTwo);
        });
      }
    }


		public override void TickLong() {
      base.TickLong();

      if (Destroyed) {
        return;
      }

      // If the parent is able to grow, grow the secondary thing as well
      if (GrowsThisSeason) {
        if (GenPlant.GrowthSeasonNow(Position, Map)) {
          if (HasEnoughLightToGrow) {
						bool gfxUpdate = Sec_HarvestableNow;
            sec_GrowthInt = Mathf.Clamp01(sec_GrowthInt + ((Sec_GrowthPerTick * 2000f) * AdjustedGrowth));
						// If there is a blooming graphic and the plant is now harvestable, dirty the map mesh here to reset the graphic
						if ((gfxUpdate != Sec_HarvestableNow) && bloomingGraphic != null && Sec_HarvestableNow && !LeaflessNow) {
							Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
						}
          }
        }
      }
      // If this isn't the right season, restart growth
      // Disabled in DevMode to allow insta-grow button
      else if (!Prefs.DevMode) {
        sec_GrowthInt = 0;
      }
    }


    // Give the secondary thing and start the regrowth process
    public Thing CollectSecondaryThing() {

      // If this isn't currently harvestable, throw a warning
      // Converts this defName to a thingDef for labelling correctly
      if (!Sec_HarvestableNow) {
        Log.Warning($"Zen Garden:: Tried to harvest {thingLabel} from {ThingDef.Named(def.defName).LabelCap} at {Position.ToString()}, but it's not harvestable.");
        return null;
      }
      Thing secondary = ThingMaker.MakeThing(secondaryDef.harvestedThingDef, null);
      // By default, minToHarvest == int.MaxValue so that exact stackcounts may be used
      if (secondaryDef.minToHarvest == int.MaxValue) {
        if (secondaryDef.maxToHarvest == 0) {
          Log.Error($"Zen Garden:: {secondaryDef.harvestedThingDef.LabelCap} doesn't have a set maxToHarvest. Unable to yield resources.");
          return null;
        }
        secondary.stackCount = secondaryDef.maxToHarvest;
      }
      // If minToHarvest has a different value, a random number is given
      else {
        secondary.stackCount = Rand.RangeInclusive(secondaryDef.minToHarvest, secondaryDef.maxToHarvest);
      }
      // Reset the growth and return the secondary thing
      sec_GrowthInt = 0;

      // If there is a blooming graphic, dirty the map mesh here to reset the graphic
      if (bloomingGraphic != null) {
        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
      }

      return secondary;
    }


    // Check wheter this plant already has a designation to harvest the secondary resource
    private bool HasHarvestDesignation() {
      foreach (Designation current in Map.designationManager.AllDesignationsOn(this)) {
        if (current.def == harvestDesignation) {
          return true;
        }
      }
      return false;
    }


    public override IEnumerable<Gizmo> GetGizmos() {

      // Add button for insta-growing the secondary thing
      Command_Action DevGrow = new Command_Action() {
        defaultLabel = "Debug: Grow " + thingLabel,
        activateSound = SoundDefOf.Click,
        action = () => {
					sec_GrowthInt = 1f;
					if (bloomingGraphic != null) {
						Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
					}
				},
      };

      if (Prefs.DevMode && !Sec_HarvestableNow) {
        yield return DevGrow;
      }

      // Add button for manually designating harvesting
      Command_Action DesignateHarvest = new Command_Action() {
        defaultLabel = "DesignatorHarvest".Translate(),
        defaultDesc = Static.DescriptionHarvestSecondary,
        icon = Static.texHarvestSecondary,
        activateSound = SoundDefOf.Click,
        action = () => {
          Map.designationManager.AddDesignation(new Designation(this, harvestDesignation));
        },
      };

      if (Sec_HarvestableNow && !HasHarvestDesignation()) {
        yield return DesignateHarvest;
      }

      foreach (Command c in base.GetGizmos()) {
        yield return c;
      }
    }


		public override string GetInspectString() {
      StringBuilder stringBuilder = new StringBuilder();
      if (LifeStage == PlantLifeStage.Growing) {
        stringBuilder.AppendLine("PercentGrowth".Translate(new object[]
        {
          GrowthPercentString
        }));

        // Append secondary growth info
        if (Sec_HarvestableNow) {
          stringBuilder.AppendLine(thingLabel + " " + "ReadyToHarvest".Translate().ToLower());
        }
        else {
          if (GrowsThisSeason) {
            stringBuilder.AppendLine(thingLabel + " " + "PercentGrowth".Translate(new object[]
            {
            Sec_GrowthPercentString
            }));
          }
          else {
            stringBuilder.AppendLine(thingLabel + " " + Static.ReportBadSeason);
          }
        }

        stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
        if (Resting) {
          stringBuilder.AppendLine("PlantResting".Translate());
        }
        if (!HasEnoughLightToGrow) {
          stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + def.plant.growMinGlow.ToStringPercent());
        }
        float growthRateFactor_Temperature = GrowthRateFactor_Temperature;
        if (growthRateFactor_Temperature < 0.99f) {
          if (growthRateFactor_Temperature < 0.01f) {
            stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
          }
          else {
            stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(new object[]
            {
              Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()
            }));
          }
        }
      }
      else if (LifeStage == PlantLifeStage.Mature) {
        if (def.plant.Harvestable) {
          stringBuilder.AppendLine("ReadyToHarvest".Translate());
        }
        else {
          stringBuilder.AppendLine("Mature".Translate());
        }

        // Append secondary growth info
        if (Sec_HarvestableNow) {
          stringBuilder.AppendLine(thingLabel + " " + "ReadyToHarvest".Translate().ToLower());
        }
        else {
          if (GrowsThisSeason) {
            stringBuilder.AppendLine(thingLabel + " " + "PercentGrowth".Translate(new object[]
            {
            Sec_GrowthPercentString
            }));
          }
          else {
            stringBuilder.AppendLine(thingLabel + " " + Static.ReportBadSeason);
          } 
        }

      }
      return stringBuilder.ToString().TrimEndNewlines();
    }
  }
}
