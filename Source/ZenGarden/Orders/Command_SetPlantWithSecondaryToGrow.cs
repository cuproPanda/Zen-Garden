using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;

namespace ZenGarden {

	[StaticConstructorOnStartup]
	public class Command_SetPlantWithSecondaryToGrow : Command {
		public IPlantToGrowSettable settable;

		private List<IPlantToGrowSettable> settables;

		private static readonly Texture2D SetPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow", true);


		public Command_SetPlantWithSecondaryToGrow() {
			ThingDef thingDef = null;
			bool flag = false;
			foreach (object current in Find.Selector.SelectedObjects) {
				IPlantToGrowSettable plantToGrowSettable = current as IPlantToGrowSettable;
				if (plantToGrowSettable != null) {
					if (thingDef != null && thingDef != plantToGrowSettable.GetPlantDefToGrow()) {
						flag = true;
						break;
					}
					thingDef = plantToGrowSettable.GetPlantDefToGrow();
				}
			}
			if (flag) {
				icon = SetPlantToGrowTex;
				defaultLabel = "CommandSelectPlantToGrowMulti".Translate();
			}
			else {
				icon = thingDef.uiIcon;
				defaultLabel = "CommandSelectPlantToGrow".Translate(new object[]
				{
					thingDef.label
				});
			}
		}


		public override void ProcessInput(Event ev) {
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (settables == null) {
				settables = new List<IPlantToGrowSettable>();
			}
			if (!settables.Contains(settable)) {
				settables.Add(settable);
			}
			List<ThingDef> plants = ZenGardenMod.SecondaryPlants;
			foreach (ThingDef current in plants) {
				ThingDef localPlantDef = current;
				string text = current.LabelCap;
				if (current.plant.sowMinSkill > 0) {
					string text2 = text;
					text = string.Concat(new object[]
					{
							text2,
							" (",
							"MinSkill".Translate(),
							": ",
							current.plant.sowMinSkill,
							")"
					});
				}
				List<FloatMenuOption> menu = list;
				Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localPlantDef);
				menu.Add(new FloatMenuOption(text, delegate
				{
					string s = localPlantDef.defName;
					for (int i = 0; i < settables.Count; i++) {
						settables[i].SetPlantDefToGrow(localPlantDef);
					}
					WarnAsAppropriate(localPlantDef);
				}, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override bool InheritInteractionsFrom(Gizmo other) {
			if (settables == null) {
				settables = new List<IPlantToGrowSettable>();
			}
			settables.Add(((Command_SetPlantWithSecondaryToGrow)other).settable);
			return false;
		}

		private void WarnAsAppropriate(ThingDef plantDef) {
			if (plantDef.plant.sowMinSkill > 0) {
				foreach (Pawn current in this.settable.Map.mapPawns.FreeColonistsSpawned) {
					if (current.skills.GetSkill(SkillDefOf.Growing).Level >= plantDef.plant.sowMinSkill && !current.Downed && current.workSettings.WorkIsActive(WorkTypeDefOf.Growing)) {
						return;
					}
				}
				Find.WindowStack.Add(new Dialog_MessageBox("NoGrowerCanPlant".Translate(new object[]
				{
					plantDef.label,
					plantDef.plant.sowMinSkill
				}).CapitalizeFirst(), null, null, null, null, null, false));
			}
		}

		private bool IsPlantAvailable(ThingDef plantDef) {
			List<ResearchProjectDef> sowResearchPrerequisites = plantDef.plant.sowResearchPrerequisites;
			if (sowResearchPrerequisites == null) {
				return true;
			}
			for (int i = 0; i < sowResearchPrerequisites.Count; i++) {
				if (!sowResearchPrerequisites[i].IsFinished) {
					return false;
				}
			}
			return true;
		}
	}
}
