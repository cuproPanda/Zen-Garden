using UnityEngine;
using Verse;

namespace ZenGarden {
  [StaticConstructorOnStartup]
  public static class Static {
    public static Texture2D texHarvestSecondary = ContentFinder<Texture2D>.Get("Cupro/UI/Designations/HarvestSecondary");

    public static DesignationDef DesignationHarvestSecondary = DefDatabase<DesignationDef>.GetNamed("ZEN_Designator_PlantsHarvestSecondary");

		public static string LabelOrchardZone = "ZEN_OrchardZone".Translate();
    public static string LabelHarvestSecondary = "ZEN_DesignatorHarvestSecondary".Translate();
    public static string DescriptionHarvestSecondary = "ZEN_DesignatorHarvestSecondaryDesc".Translate();
    public static string ReportDesignatePlantsWithSecondary = "ZEN_MustDesignatePlantsWithSecondary".Translate();
    public static string ReportDesignateHarvestableSecondary = "ZEN_MustDesignateHarvestableSecondary".Translate();
    public static string ReportBadSeason = "ZEN_CannotGrowBadSeason".Translate();
		public static string DisplayStat_LimitedGrowSeasons = "ZEN_DisplayStat_LimitedGrowSeasons".Translate();
		public static string DisplayStat_GrowsInAllSeasons = "ZEN_DisplayStat_GrowsInAllSeasons".Translate();
		public static string DisplayStat_MinPlantGrowth = "ZEN_DisplayStat_MinPlantGrowth".Translate();
		public static string DisplayStat_MinGrowthReport = "ZEN_DisplayStat_MinGrowthReport".Translate();
	}
}
