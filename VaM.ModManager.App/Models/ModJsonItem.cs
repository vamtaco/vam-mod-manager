using System.Collections.Generic;

namespace VaM.ModManager.App.Models
{
    public class ModJsonItem
    {
        public bool? UseSceneLoadPosition { get; set; }
        public List<AtomItem> Atoms { get; set; } = new List<AtomItem>();
    }
}
