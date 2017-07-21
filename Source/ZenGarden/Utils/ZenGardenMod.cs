using System.Collections.Generic;

using Verse;

namespace ZenGarden {

  public sealed class ZenGardenMod : Mod {

		private static List<ThingDef> secondaryPlants;
		public static List<ThingDef> SecondaryPlants {
			get {
				if (secondaryPlants.NullOrEmpty()) {
					Log.Error("Zen Garden:: Secondary plants list isn't assigned, and will not allow orchard zones to grow plants.");
					return null;
				}
				return secondaryPlants;
			}
		}


    public ZenGardenMod(ModContentPack content) : base(content) {
			LongEventHandler.ExecuteWhenFinished(AssignPlants);
    }


		private void AssignPlants() {
			secondaryPlants = new List<ThingDef>();
			foreach (ThingDef plant in DefDatabase<ThingDef>.AllDefs) {
				if (plant.HasModExtension<SecondaryResource>()) {
					secondaryPlants.Add(plant);
				}
			}
		}
  }
}
