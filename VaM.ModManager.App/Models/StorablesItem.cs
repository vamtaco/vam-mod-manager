using System.Collections.Generic;
using System.Numerics;

namespace VaM.ModManager.App.Models
{
    public class StorablesItem
    {
        public string Id { get; set; }
        public string Character { get; set; }
        public string Hair { get; set; }
        public string Face { get; set; }
        public Vector3? Position { get; set; }
        public Vector3? Rotation { get; set; }
        public string PositionState { get; set; }
        public string RotationState { get; set; }
        public Vector3? RootPosition { get; set; }
        public Vector3? RootRotation { get; set; }
        public bool? InteractableInPlayMode { get; set; }
        public bool? XPositionLock { get; set; }
        public bool? YPositionLock { get; set; }
        public bool? ZPositionLock { get; set; }
        public bool? XRotationLock { get; set; }
        public bool? YRotationLock { get; set; }
        public bool? ZRotationLock { get; set; }
        public List<ClothingItem> Clothing { get; set; }
        public List<MorphItem> Morphs { get; set; }
        //trigger
    }
}
