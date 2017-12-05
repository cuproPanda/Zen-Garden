using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {

	public class Zone_Orchard : Zone, IPlantToGrowSettable {

		private ThingDef plantDefToGrow = ZenDefOf.ZEN_PlantTreeCherry;
		public bool allowSow = true;
		public bool CanAcceptSowNow() => allowSow;

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells => Cells;

		public override bool IsMultiselectable => true;

		protected override Color NextZoneColor {
			get {
				return ZoneColorUtility.NextGrowingZoneColor();
			}
		}


		public Zone_Orchard() {
		}


		public Zone_Orchard(ZoneManager zoneManager) : base(Static.LabelOrchardZone, zoneManager) {
		}


		public override void ExposeData() {
			base.ExposeData();
			Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
			Scribe_Values.Look(ref allowSow, "allowSow", true, false);
		}


		public override IEnumerable<Gizmo> GetGizmos() {
			foreach (Gizmo g in base.GetGizmos()) {
				yield return g;
			}
			yield return SecondaryPlantToGrowSettableUtility.SetPlantCommand(this);
			yield return new Command_Toggle {
				defaultLabel = "CommandAllowSow".Translate(),
				defaultDesc = "CommandAllowSowDesc".Translate(),
				hotKey = KeyBindingDefOf.CommandItemForbid,
				icon = TexCommand.Forbidden,
				isActive = (() => allowSow),
				toggleAction = delegate {
					allowSow = !allowSow;
				}
			};
		}


		public override string GetInspectString() {
			string text = string.Empty;
			if (!Cells.NullOrEmpty()) {
				IntVec3 c = Cells.First();
				if (c.UsesOutdoorTemperature(Map)) {
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"OutdoorGrowingPeriod".Translate(),
						": ",
						Zone_Growing.GrowingQuadrumsDescription(Map.Tile),
						"\n"
					});
				}
				if (GenPlant.GrowthSeasonNow(c, Map)) {
					text += "GrowSeasonHereNow".Translate();
				}
				else {
					text += "CannotGrowBadSeasonTemperature".Translate();
				}
			}
			return text;
		}


		public static string GrowingQuadrumsDescription(int tile) {
			List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, 10f, 42f);
			if (list.NullOrEmpty()) {
				return "NoGrowingPeriod".Translate();
			}
			if (list.Count == 12) {
				return "GrowYearRound".Translate();
			}
			return "PeriodDays".Translate(new object[]
			{
				list.Count * 5
			}) + " (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")";
		}


		public ThingDef GetPlantDefToGrow() {
			return plantDefToGrow;
		}


		public void SetPlantDefToGrow(ThingDef plantDef) {
			if (plantDef.thingClass == typeof(PlantWithSecondary)) {
				plantDefToGrow = plantDef;
			}
		}
	}
}
