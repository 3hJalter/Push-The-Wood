using System.Linq;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid.GridSurface;
using _Game.GameGrid.Unit;
using _Game.Resource;
using UnityEngine;
using UnityEngine.Serialization;
using VinhLB;
using Random = System.Random;

namespace _Game.Managers
{
    [DefaultExecutionOrder(-100)]
    public class DataManager : Singleton<DataManager>
    {

        #region Game Saving Data
        private GameData _gameData;

        public GameData GameData => _gameData ?? Load();

        private GameData Load()
        {
            _gameData = Database.LoadData();
            return _gameData;
        }
        public void Save()
        {
            Database.SaveData(_gameData);
        }
        #endregion

        #region In-Game Data
        [SerializeField]
        private ConfigData configData;
        [SerializeField]
        private AudioData audioData;
        [SerializeField]
        private GridData gridData;
        [SerializeField]
        private MaterialData materialData;
        [SerializeField]
        private VFXData vfxData;
        [SerializeField]
        private UIResourceDatabase _uiResourceDatabase;

        public ConfigData ConfigData => configData;
        public AudioData AudioData => audioData;
        public VFXData VFXData => vfxData;
        public UIResourceDatabase UIResourceDatabase => _uiResourceDatabase;

        #endregion

        #region In-Game Function

        public int CountNormalLevel => gridData.CountNormalLevel;
        public int CountSecretLevel => gridData.CountSecretLevel;
        public int CountSurfaceMaterial => materialData.CountSurfaceMaterial;
        public int CurrentPlayerSkinIndex
        {
            get
            {
                if (!(IsCharacterSkinUnlock(GameData.user.currentPlayerSkinIndex) || IsCharacterSkinRent(GameData.user.currentPlayerSkinIndex)))
                {
                    GameData.user.currentPlayerSkinIndex = GameData.user.currentUnlockPlayerSkinIndex;
                }
                return GameData.user.currentPlayerSkinIndex;
            }
        }
        public int CurrentUIPlayerSkinIndex
        {
            get
            {
                if (!(IsCharacterSkinUnlock(GameData.user.currentPlayerSkinIndex) || 
                      GameData.user.playerRentSkinState[GameData.user.currentPlayerSkinIndex] > 0))
                {
                    return GameData.user.currentUnlockPlayerSkinIndex;
                }
                return GameData.user.currentPlayerSkinIndex;
            }
        }
        public int HintAdsCount => GameData.user.hintAdsCount;
        public int GoldCount => GameData.user.gold;
        public int InterAdsStepCount => GameData.user.interAdsStepCount;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public int CurrentDay => GameData.user.currentDay;
        public int DaysInMonth => GameData.user.daysInMonth;
        
        public int GetDailyChallengeDay(int index)
        {
            return _gameData.user.dailyLevelIndex[index];
        }

        public bool IsOpenInGameDailyChallengeTut()
        {
            return _gameData.user.isOpenInGameDailyChallengeTut;
        }

        public void ChangeOpenDailyChallengeTut(bool value)
        {
            _gameData.user.isOpenInGameDailyChallengeTut = value;
        }

        public bool IsDailyChallengeFreePlay()
        {
            return _gameData.user.isFreeDailyChallengeFirstTime;
        }

        public void ChangeDailyChallengeFreePlay(bool value)
        {
            _gameData.user.isFreeDailyChallengeFirstTime = value;
        }

        public bool IsCollectedAllDailyChallengeReward()
        {
            int collected = _gameData.user.dailyChallengeRewardCollected.Count;
            int cleared = _gameData.user.dailyLevelIndexComplete.Count;
            // TEMPORARY: Need change if total daily challenge reward is changed
            return cleared switch
            {
                7 => collected >= 4,
                4 or 5 or 6 => collected >= 3,
                2 or 3 => collected >= 2,
                1 => collected >= 1,
                _ => false
            };
        }

        public bool IsClearAllDailyChallenge()
        {
            // get current day in month from UTC time
            int currentDay = System.DateTime.UtcNow.Day;
            
            return _gameData.user.dailyLevelIndexComplete.Count >= currentDay;
        }
        
        public bool IsClearDailyChallenge(int index)
        {
            int levelIndex = _gameData.user.dailyLevelIndex[index];
            return _gameData.user.dailyLevelIndexComplete.Contains(levelIndex);
        }

        public bool IsClearAllSecretLevel()
        {
            return _gameData.user.secretLevelIndexComplete.Count >= CountSecretLevel;
        }

        public bool IsSecretLevelComplete(int index)
        {
            return _gameData.user.secretLevelIndexComplete.Contains(index);
        }

        public bool IsAdsHintEnough()
        {
            return GameData.user.hintAdsCount >= ConfigData.GetBoosterConfig(BoosterType.PushHint).GoldPerBuyRatio.goldNeed;
        }

        public void CheckingRentPlayerSkinCount()
        {
            if (IsCharacterSkinRent(GameData.user.currentPlayerSkinIndex))
            {
                AddRentCharacterSkinCount(GameData.user.currentPlayerSkinIndex, -1);
            }
        }
        public bool IsCharacterSkinUnlock(int index)
        {
            return GameData.user.playerSkinState[index] != 0;
        }

        public void SetUnlockCharacterSkin(int index, bool value)
        {
            GameData.user.playerSkinState[index] = value ? 1 : 0;
            if (value)
            {
                GameData.user.playerRentSkinState[index] = 0;
            }
        }

        public void SetCharacterSkinIndex(int index)
        {
            GameData.user.currentPlayerSkinIndex = index;
            if (IsCharacterSkinUnlock(index))
                GameData.user.currentUnlockPlayerSkinIndex = index;
        }

        public bool IsCharacterSkinRent(int index)
        {
            return GameData.user.playerRentSkinState[index] >= 0;
        }
        
        public void SetRentCharacterSkinCount(int index, int value)
        {
            GameData.user.playerRentSkinState[index] = value;
        }

        public int GetRentCharacterSkinCount(int index)
        {
            return GameData.user.playerRentSkinState[index];
        }
        public void AddRentCharacterSkinCount(int index, int value)
        {
            GameData.user.playerRentSkinState[index] += value;
        }
        public void SetInterAdsStepCount(int value)
        {
            if(value < 0)
            {
                GameData.user.interAdsStepCount = 0;
            }
            else
            {
                GameData.user.interAdsStepCount = value;
            }
        }
        public void AddInterAdsStepCount(int value)
        {
            int newValue = InterAdsStepCount + value;
            if (newValue < 0)
            {
                GameData.user.interAdsStepCount = 0;
            }
            else
            {
                GameData.user.interAdsStepCount = newValue;
            }
        }
        public int GetLevelTime(LevelType type, LevelNormalType normalType = LevelNormalType.None)
        {
            return type switch
            {
                LevelType.Normal => normalType is LevelNormalType.None
                    ? configData.timePerNormalLevel[(int)LevelNormalType.Medium].time
                    : configData.timePerNormalLevel[(int)normalType].time,
                LevelType.Secret => configData.timePerSecretLevel,
                LevelType.DailyChallenge => configData.timePerDailyChallengeLevel,
                _ => configData.timePerNormalLevel[(int)LevelNormalType.Medium].time
            };
        }

        public Material GetTransparentMaterial()
        {
            return materialData.GetTransparentMaterial();
        }

        public Material GetSurfaceMaterial(MaterialEnum materialEnum)
        {
            return materialData.GetSurfaceMaterial(materialEnum);
        }

        public Material GetGrassMaterial(MaterialEnum materialEnum)
        {
            return materialData.GetGrassMaterial(materialEnum);
        }

        public void OnChangeTheme(ThemeEnum theme)
        {
            materialData.CurrentTheme = theme;
        }
        
        public TextAsset GetLevelData(LevelType type, int index)
        {
            return gridData.GetLevelData(type, index);
        }

        public void AddGridTextData(LevelType type, TextAsset textAsset)
        {
            gridData.AddGridTextData(type, textAsset);
        }

        public bool HasGridTextData(LevelType type, TextAsset textAsset)
        {
            return gridData.HasGridTextData(type, textAsset);
        }

        public GridSurface GetGridSurface(PoolType poolType)
        {
            return gridData.GetGridSurface(poolType);
        }

        public GridUnit GetGridUnit(PoolType poolType)
        {
            return gridData.GetGridUnit(poolType);
        }

        public int GetGridTextDataIndex(LevelType type, TextAsset load)
        {
            return gridData.GetGridTextDataIndex(type, load);
        }

        public EnvironmentUnit GetRandomEnvironmentObject(PoolType poolType)
        {
            return gridData.GetRandomEnvironmentObject(poolType);
        }

        public UIUnit GetUIUnit(PoolType poolType)
        {
            return gridData.GetUIUnit(poolType);
        }

        public UIUnit GetWorldUIUnit(PoolType poolType)
        {
            return gridData.GetWorldUIUnit(poolType);
        }

        public UIResourceConfig GetBoosterUIResourceConfig(BoosterType type)
        {
            return _uiResourceDatabase.BoosterResourceConfigDict[type];
        }

        public UIResourceConfig GetCurrencyUIResourceConfig(CurrencyType type)
        {
            if (type is CurrencyType.RandomBooster)
            {
                return _uiResourceDatabase.BoosterResourceConfigDict.ElementAt(new Random().Next(0, _uiResourceDatabase.BoosterResourceConfigDict.Count)).Value;
            }
            return _uiResourceDatabase.CurrencyResourceConfigDict[type];
        }

        public UIResourceConfig GetCharacterUIResourceConfig(CharacterType type)
        {
            return _uiResourceDatabase.CharacterResourceConfigDict[type];
        }

        #endregion
    }
}
