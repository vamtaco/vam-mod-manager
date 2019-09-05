using System.Collections.Generic;
using System.Numerics;

namespace VaM.ModManager.App.Models
{
    public class AtomItem
    {
        public string Id { get; set; }
        public bool? On { get; set; }
        public string Type { get; set; }
        public List<StorablesItem> Storables { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 ContainerPosition { get; set; }
        public Vector3 ContainerRotation { get; set; }
    }
}
