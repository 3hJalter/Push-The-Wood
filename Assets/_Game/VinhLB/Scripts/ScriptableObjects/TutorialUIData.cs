using System.Collections.Generic;
using GameGridEnum;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VinhLB
{
    [CreateAssetMenu(fileName = "TutorialUIData", menuName = "ScriptableObjects/TutorialUIData", order = 4)]
    public class TutorialUIData : SerializedScriptableObject
    {
        public Dictionary<TutorialType, TutorialUI[]> TutorialUIDict = new();
    }

    [System.Serializable]
    public class TutorialUI
    {
        public Sprite TutorialSprite;
        public string Description;
    }
}