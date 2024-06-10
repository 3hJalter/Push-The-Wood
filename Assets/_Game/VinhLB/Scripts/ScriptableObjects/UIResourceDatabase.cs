using System.Collections.Generic;
using _Game._Scripts.InGame;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VinhLB
{
    using _Game.Resource;
    [CreateAssetMenu(fileName = "UIResourceDatabase", menuName = "ScriptableObjects/UIResourceDatabase")]
    public class UIResourceDatabase : SerializedScriptableObject
    {
        [Title("Booster")]
        [SerializeField]
        private List<Sprite> _boosterIconList;
        public Dictionary<BoosterType, UIResourceConfig> BoosterResourceConfigDict = new();
        
        [Title("Currency")]
        [SerializeField]
        private List<Sprite> _currencyIconList;
        public Dictionary<CurrencyType, UIResourceConfig> CurrencyResourceConfigDict = new();

        [Title("Character")]
        [SerializeField]
        private List<Sprite> _characterIconList;
        public Dictionary<CharacterType, UIResourceConfig> CharacterResourceConfigDict = new();

        [Title("Win Lose Screen")] 
        public Dictionary<LevelWinCondition, UIResourceConfig> WinScreenResourceConfigDict = new();
        public  Dictionary<LevelLoseCondition, UIResourceConfig> LoseScreenResourceConfigDict = new();
        
        [Title("Objective Icon")]
        public Dictionary<LevelWinCondition, UIResourceConfig> objectiveIconDict = new();
        
        [ContextMenu("Convert From List To Dict")]
        public void ConvertListToDict()
        {
            for (int i = 0; i < _currencyIconList.Count; i++)
            {
                CurrencyType type = (CurrencyType)i;
                if (CurrencyResourceConfigDict.ContainsKey(type))
                    CurrencyResourceConfigDict[type] = new UIResourceConfig(type.ToString(), _currencyIconList[i]);
                else
                    CurrencyResourceConfigDict.Add(type, new UIResourceConfig(type.ToString(), _currencyIconList[i]));
            }

            for (int i = 0; i < _boosterIconList.Count; i++)
            {
                BoosterType type = (BoosterType)i;
                if (BoosterResourceConfigDict.ContainsKey(type))
                    BoosterResourceConfigDict[type] = new UIResourceConfig(type.ToString(), _boosterIconList[i]);
                else
                    BoosterResourceConfigDict.Add(type, new UIResourceConfig(type.ToString(), _boosterIconList[i]));
            }
            
            for (int i = 0; i < _characterIconList.Count; i++)
            {
                CharacterType type = (CharacterType)i;
                if (CharacterResourceConfigDict.ContainsKey(type))
                    CharacterResourceConfigDict[type] = new UIResourceConfig(type.ToString(), _characterIconList[i]);
                else
                    CharacterResourceConfigDict.Add(type, new UIResourceConfig(type.ToString(), _characterIconList[i]));
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }
    }

    [System.Serializable]
    public struct UIResourceConfig
    {
        public string Name;
        public Sprite MainIconSprite;
        public Sprite SubIconSprite;

        public UIResourceConfig(string name, Sprite mainIconSprite, Sprite subIconSprite = null)
        {
            Name = name;
            MainIconSprite = mainIconSprite;
            SubIconSprite = subIconSprite;
        }
    }   
}