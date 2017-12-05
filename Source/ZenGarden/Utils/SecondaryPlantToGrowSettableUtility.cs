using RimWorld;
using Verse;

namespace ZenGarden {

	public static class SecondaryPlantToGrowSettableUtility {

		public static Command SetPlantCommand(IPlantToGrowSettable newSettable) {
			return new Command_SetPlantWithSecondaryToGrow {
				defaultDesc = "CommandSelectPlantToGrowDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc1,
				settable = newSettable
			};
		}
	}
}
