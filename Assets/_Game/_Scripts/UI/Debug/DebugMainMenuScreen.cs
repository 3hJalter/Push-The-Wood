using System;
using _Game.Managers;
using System.Collections;
using System.Collections.Generic;
using _Game._Scripts.Utilities;
using _Game.Data;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;
using _Game.GameGrid;
using _Game.UIs.Popup;
using TMPro;

namespace _Game.UIs.Screen
{
    public class DebugMainMenuScreen : UICanvas
    {
        [Header("General")]
        [SerializeField]
        private Toggle _debugMenusToggle;
        [SerializeField]
        private GameObject _menusGO;
        [SerializeField]
        private Button _backButton;
        
        [Header("Debug Menu")]
        [SerializeField]
        private GameObject _debugMenuGO;
        [SerializeField] 
        private Button _settingsButton;
        [SerializeField] 
        private Button _resourceButton;
        [SerializeField]
        private Button _dailyRewardButton;
        [SerializeField]
        private Button _dailyChallengeButton;
        [SerializeField]
        private Button _changeLevel;
        [SerializeField]
        private TMP_InputField _levelInputField;

        [Header("Settings Menu")]
        [SerializeField]
        private GameObject _settingsMenuGO;
        [SerializeField] 
        private Button _toggleWaterButton;
        
        [Header("Resource Menu")]
        [SerializeField]
        private GameObject _resourceMenuGO;
        [SerializeField] 
        private Button _addGoldButton;
        [SerializeField]
        private Button _addAdTicketsButton;
        [SerializeField]
        private Button _addRewardKeysButton;
        [SerializeField]
        private Button _addLevelStarsButton;
        [SerializeField]
        private Button _addSecretMapPieceButton;
        [SerializeField]
        private Button _resetResourceButton;
        [SerializeField]
        private Button _changePlayerSkinButton;
        [SerializeField]
        private TMP_InputField _playerSkinInputField;
        
        [Header("Daily Reward Menu")]
        [SerializeField]
        private GameObject _dailyRewardMenuGO;
        [SerializeField] 
        private Button _setCanCollectTodayButton;
        [SerializeField]
        private Button _increase1DailyDay;
        [SerializeField]
        private Button _decrease1DailyDayButton;
        [SerializeField]
        private Button _resetDailyRewardButton;

        [Header("Daily Challenge menu")]
        [SerializeField]
        private GameObject _dailyChallengeMenuGO;

        [SerializeField]
        private Button _increase1ChallengeDay;
        
        private void Awake()
        {
            _debugMenusToggle.onValueChanged.AddListener(ToggleDebugMenu);
            _backButton.onClick.AddListener(BackToDebugMenu);
            
            _settingsButton.onClick.AddListener(SettingsButton);
            _resourceButton.onClick.AddListener(ResourceButton);
            _dailyRewardButton.onClick.AddListener(DailyRewardButton);
            _dailyChallengeButton.onClick.AddListener(DailyChallengeButton);
            
            _toggleWaterButton.onClick.AddListener(ToggleWater);
            
            _addAdTicketsButton.onClick.AddListener(AddHeart);
            _addGoldButton.onClick.AddListener(AddGold);
            _addRewardKeysButton.onClick.AddListener(AddRewardKeys);
            _addLevelStarsButton.onClick.AddListener(AddLevelStars);
            _addSecretMapPieceButton.onClick.AddListener(AddSecretMapPiece);
            _resetResourceButton.onClick.AddListener(ResetUserData);
            _changePlayerSkinButton.onClick.AddListener(ChangePlayerSkin);
            
            _setCanCollectTodayButton.onClick.AddListener(DailyRewardManager.SetCanCollectToday);
            _increase1DailyDay.onClick.AddListener(DailyRewardManager.Increase1DailyDay);
            _decrease1DailyDayButton.onClick.AddListener(DailyRewardManager.Decrease1DailyDay);
            _resetDailyRewardButton.onClick.AddListener(DailyRewardManager.ResetAll);
            
            _increase1ChallengeDay.onClick.AddListener(AddOneDailyChallengeDay);

            _playerSkinInputField.text = DataManager.Ins.GameData.user.currentPlayerSkinIndex.ToString();
            _levelInputField.text = DataManager.Ins.GameData.user.normalLevelIndex.ToString();
            _changeLevel.onClick.AddListener(ChangeLevel);
        }

        private void OnEnable()
        {
            BackToDebugMenu();
        }
        
        private void ToggleDebugMenu(bool value)
        {
            _menusGO.SetActive(value);
        }
        
        private void BackToDebugMenu()
        {
            _backButton.gameObject.SetActive(false);
            _debugMenuGO.SetActive(true);
            _settingsMenuGO.SetActive(false);
            _resourceMenuGO.SetActive(false);
            _dailyRewardMenuGO.SetActive(false);
            _dailyChallengeMenuGO.SetActive(false);
        }
        
        private void SettingsButton()
        {
            _backButton.gameObject.SetActive(true);
            _debugMenuGO.SetActive(false);
            _settingsMenuGO.SetActive(true);
        }

        private void ResourceButton()
        {
            _backButton.gameObject.SetActive(true);
            _debugMenuGO.SetActive(false);
            _resourceMenuGO.SetActive(true);
        }

        private void DailyRewardButton()
        {
            _backButton.gameObject.SetActive(true);
            _debugMenuGO.SetActive(false);
            _dailyRewardMenuGO.SetActive(true);
        }
        
        private void DailyChallengeButton()
        {
            _backButton.gameObject.SetActive(true);
            _debugMenuGO.SetActive(false);
            _dailyChallengeMenuGO.SetActive(true);
        }
        
        private void ToggleWater()
        {
            FXManager.Ins.ToggleWater();
        }

        private void AddHeart()
        {
            GameManager.Ins.GainHeart(1, _addAdTicketsButton.transform.position);
            //UIManager.Ins.UpdateUIs();
        }
        
        private void AddGold()
        {
            GameManager.Ins.GainGold(1000, _addGoldButton.transform.position);
            //UIManager.Ins.UpdateUIs();
        }

        private void AddRewardKeys()
        {
            GameManager.Ins.GainRewardKeys(1, _addRewardKeysButton.transform.position);
        }
        
        private void AddLevelStars()
        {
            GameManager.Ins.GainLevelProgress(1, _addLevelStarsButton.transform.position);
        }
        
        private void ChangePlayerSkin()
        {
            int index = int.Parse(_playerSkinInputField.text);
            DataManager.Ins.GameData.user.currentPlayerSkinIndex = index;
            LevelManager.Ins.player?.ChangeSkin(index);
        }
        private void AddSecretMapPiece()
        {
            GameManager.Ins.GainSecretMapPiece(1);
        }

        private void ResetUserData()
        {
            GameManager.Ins.ResetUserData();
            //UIManager.Ins.UpdateUIs();
        }       
        
        private void ChangeLevel()
        {
            int index = int.Parse(_levelInputField.text);
            DataManager.Ins.GameData.user.normalLevelIndex = index;
            LevelManager.Ins.OnGoLevel(LevelType.Normal, DataManager.Ins.GameData.user.normalLevelIndex, true);
        }
        private void AddOneDailyChallengeDay()
        {
            DataManager.Ins.GameData.user.currentDay++;
            DataManager.Ins.GameData.user.isFreeDailyChallengeFirstTime = true;
            if (DataManager.Ins.GameData.user.currentDay > DataManager.Ins.GameData.user.daysInMonth)
            {
                DataManager.Ins.GameData.user.currentDay = 1;
                DataManager.Ins.GameData.user.dailyLevelIndexComplete.Clear();
                DataManager.Ins.GameData.user.dailyChallengeRewardCollected.Clear();
                if (DataManager.Ins.GameData.user.completedOneTimeTutorial.Contains(DataManager.Ins.ConfigData
                        .unlockDailyChallengeAtLevelIndex)) // If clear the tutorial && unlock daily challenge
                {
                    // Shuffle daily level index
                    DataManager.Ins.GameData.user.dailyLevelIndex.Shuffle();
                }
            }
            if (!UIManager.Ins.IsOpened<DailyChallengePopup>()) return;
            DailyChallengePopup popup = UIManager.Ins.GetUI<DailyChallengePopup>();
            popup.Setup();
        }
    }
}