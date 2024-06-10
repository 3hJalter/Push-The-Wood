using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VinhLB
{
    [CreateAssetMenu(fileName = "MaskDatabase", menuName = "ScriptableObjects/MaskDatabase")]
    public class MaskDatabase : SerializedScriptableObject
    {
        [Title("Mask Sprite Dict")]
        public Dictionary<MaskType, Sprite> MaskSpriteDict = new();
    }
}