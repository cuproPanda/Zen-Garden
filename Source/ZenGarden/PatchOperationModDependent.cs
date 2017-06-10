using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Verse;

namespace ZenGarden {

  internal class PatchOperationModDependent : PatchOperation {

    private List<string> modList;
    private string modName;

    protected override bool ApplyWorker(XmlDocument xml) {

      if (!modList.NullOrEmpty()) {
        return ApplyWorker_Multiple();
      }

      if (modName.NullOrEmpty()) {
        return false;
      }

      return ApplyWorker_Single();
    }


    private bool ApplyWorker_Multiple() {
      for (int m = 0; m < modList.Count; m++) {
        if (!ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modList[m])) {
          return false;
        }
      }
      return true;
    }


    private bool ApplyWorker_Single() {
      return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName);
    }
  }
}
