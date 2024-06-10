using System;
using System.Collections.Generic;
using _Game.DesignPattern;
using _Game.GameGrid.GridSurface;
using _Game.GameGrid.Unit;
using _Game.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VinhLB;

namespace _Game.Data
{
    [CreateAssetMenu(fileName = "GridData", menuName = "ScriptableObjects/GridData", order = 1)]
    public class GridData : SerializedScriptableObject
    {
        [Title("Level Text Data")]
        [SerializeField]
        private List<TextAsset> normalLevel = new();
        [SerializeField]
        private List<TextAsset> dailyChallengerLevel = new();
        [SerializeField]
        private List<TextAsset> secretLevel = new();

        [Title("Dynamic Unit")]
        [SerializeField]
        private readonly Dictionary<PoolType, GridUnitDynamic> _dynamicUnitDict = new();

        [Title("Static Unit")]
        [SerializeField]
        private readonly Dictionary<PoolType, GridUnitStatic> _staticUnitDict = new();

        [Title("Building Unit")]
        [SerializeField]
        private readonly Dictionary<PoolType, BuildingUnit> _buildingUnitDict = new();

        [Title("Surface")]
        [SerializeField]
        private readonly Dictionary<PoolType, GridSurface> _surfaceDict = new();

        [Title("Environment Unit")]
        [SerializeField]
        private readonly Dictionary<PoolType, EnvironmentUnit[]> _environmentUnitDict = new();

        [Title("UI Unit")]
        [SerializeField]
        private readonly Dictionary<PoolType, UIUnit> _uiUnitDict = new();
        [SerializeField]
        private readonly Dictionary<PoolType, UIUnit> _worldUIUnitDict = new();

        public int CountNormalLevel => normalLevel.Count;
        public int CountSecretLevel => secretLevel.Count;

        public TextAsset GetLevelData(LevelType type, int index)
        {
            return type switch
            {
                LevelType.Normal => normalLevel[index],
                LevelType.DailyChallenge => dailyChallengerLevel[index],
                LevelType.Secret => secretLevel[index],
                _ => null
            };
        }

        public GridSurface GetGridSurface(PoolType poolType)
        {
            return _surfaceDict.GetValueOrDefault(poolType);
        }

        public GridUnit GetGridUnit(PoolType poolType)
        {
            // Get from dictionary static unit first
            if (_staticUnitDict.TryGetValue(poolType, out GridUnitStatic staticUnit)) return staticUnit;
            // else get from dictionary dynamic unit
            if (_dynamicUnitDict.TryGetValue(poolType, out GridUnitDynamic dynamicUnit)) return dynamicUnit;
            // else get from dictionary building unit or return null (default)
            return _buildingUnitDict.GetValueOrDefault(poolType);
        }

        public EnvironmentUnit GetRandomEnvironmentObject(PoolType poolType)
        {
            EnvironmentUnit[] environmentObjects = _environmentUnitDict.GetValueOrDefault(poolType);
            if (environmentObjects == null || environmentObjects.Length == 0)
            {
                return null;
            }
            int randomIndex = UnityEngine.Random.Range(0, environmentObjects.Length);

            return _environmentUnitDict[poolType][randomIndex];
        }

        public UIUnit GetUIUnit(PoolType poolType)
        {
            return _uiUnitDict.GetValueOrDefault(poolType);
        }
            
        public UIUnit GetWorldUIUnit(PoolType poolType)
        {
            return _worldUIUnitDict.GetValueOrDefault(poolType);
        }

        public void AddGridTextData(LevelType type, TextAsset textAsset)
        {
            switch (type)
            {
                case LevelType.Normal:
                    normalLevel.Add(textAsset);
                    break;
                case LevelType.DailyChallenge:
                    dailyChallengerLevel.Add(textAsset);
                    break;
                case LevelType.Secret:
                    secretLevel.Add(textAsset);
                    break;
                default:
                    DevLog.Log(DevId.Hoang, "AddGridTextData: LevelType not found");
                    break;
            }
        }

        public bool HasGridTextData(LevelType type, TextAsset textAsset)
        {
            return type switch
            {
                LevelType.Normal => normalLevel.Contains(textAsset),
                LevelType.DailyChallenge => dailyChallengerLevel.Contains(textAsset),
                LevelType.Secret => secretLevel.Contains(textAsset),
                _ => false
            };
        }

        public int GetGridTextDataIndex(LevelType type, TextAsset load)
        {
            // return -1 if not found
            switch (type)
            {
                case LevelType.Normal:
                    if (!normalLevel.Contains(load)) return -1;
                    return normalLevel.IndexOf(load);
                case LevelType.DailyChallenge:
                    if (!dailyChallengerLevel.Contains(load)) return -1;
                    return dailyChallengerLevel.IndexOf(load);
                case LevelType.Secret:
                    if (!secretLevel.Contains(load)) return -1;
                    return secretLevel.IndexOf(load);
                default:
                    DevLog.Log(DevId.Hoang, "GetGridTextDataIndex: LevelType not found");
                    return -1;
            }
        }
    }

    public enum LevelNormalType
    {
        None = -1,
        Easy = 0,
        Medium = 1,
        Hard = 2,
    }
    
    public enum LevelType
    {
        None = -1,
        Normal = 0,
        DailyChallenge = 1,
        Secret = 2,
    }
}