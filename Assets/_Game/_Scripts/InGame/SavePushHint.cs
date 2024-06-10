using System;
using System.Collections.Generic;
using System.IO;
using _Game._Scripts.Managers;
using _Game.Data;
using _Game.GameGrid;
using _Game.Managers;
using _Game.Utilities;
using UnityEngine;

namespace _Game._Scripts.InGame
{
    [Serializable]
    public record PlayerStep
    {
        public int x;
        public int y;
        public int d; // Direction
        public int i; //  Island ID

        public bool IsSameStep(PlayerStep other)
        {
            return x == other.x && y == other.y && d == other.d && i == other.i;
        }
    }

    public class PushHint
    {
        private readonly HashSet<int> _islandCompleted = new();
        private readonly Dictionary<int, Stack<PlayerStep>> _playerStepsOnIsland;
        private Stack<PlayerStep> _currentPlayerSteps;
        private int _currentIslandHintId;
        private bool _isShowHint;
        private Stack<PlayerStep> _pops;

        public PushHint(IEnumerable<PlayerStep> steps)
        {
            _currentIslandHintId = -1;
            _playerStepsOnIsland = new Dictionary<int, Stack<PlayerStep>>();
            if (steps is null) return;
            foreach (PlayerStep step in steps)
            {
                if (!_playerStepsOnIsland.ContainsKey(step.i))
                    _playerStepsOnIsland.Add(step.i, new Stack<PlayerStep>());
                _playerStepsOnIsland[step.i].Push(step);
            }
        }

        public bool IsPlayerMakeHintWrong { get; private set; }

        public bool IsStartHint { get; private set; }

        public int LastHintIslandId => _currentIslandHintId;
        
        public bool ContainIsland(int islandID)
        {
            return _playerStepsOnIsland.ContainsKey(islandID);
        }

        public void OnStartHint(int islandID, bool isShowHint = true)
        {
            if (_playerStepsOnIsland.Count == 0) return;
            if (_currentPlayerSteps is not null) OnStopHint();
            // make a copy of the stack
            _currentIslandHintId = islandID;
            _isShowHint = isShowHint;
            _currentPlayerSteps = new Stack<PlayerStep>(_playerStepsOnIsland[islandID]);
            _pops = new Stack<PlayerStep>();
            RemoveListener();
            EventGlobalManager.Ins.OnPlayerPushStep.AddListener(OnTrackingPlayerPush);
            IsStartHint = true;
            IsPlayerMakeHintWrong = false;
            OnShowHint(_currentPlayerSteps.Peek(), _isShowHint);
            GameplayManager.Ins.OnShowFocusBooster(false);
        }

        private void OnPauseHint()
        {
            if (GameplayManager.Ins.PushHintObject != null) GameplayManager.Ins.PushHintObject.SetActive(false);
        }

        public void OnContinueHint()
        {
            IsPlayerMakeHintWrong = false;
            PlayerStep playerStep = _currentPlayerSteps.Peek();
            OnShowHint(playerStep, _isShowHint);
        }

        public void OnStopHint(bool isRemoveListener = true)
        {
            // check if the event contains the listener
            if (isRemoveListener) RemoveListener();
            _currentIslandHintId = -1;
            _currentPlayerSteps = null;
            _pops = null;
            IsStartHint = false;
            IsPlayerMakeHintWrong = false;
            if (GameplayManager.Ins.PushHintObject != null) GameplayManager.Ins.PushHintObject.SetActive(false);
        }

        public void OnRevertHint()
        {
            // Revert the last step
            if (_pops.Count == 0) return;
            PlayerStep playerStep = _pops.Pop();
            // If the player Island now is not the same with the last step, StopHint
            if (playerStep.i != LevelManager.Ins.player.islandID)
            {
                OnStopHint();
                return;
            }

            _currentPlayerSteps.Push(playerStep);
            OnShowHint(playerStep, _isShowHint);
        }

        private void RemoveListener()
        {
            if (!EventGlobalManager.Ins.OnPlayerPushStep.Contains(OnTrackingPlayerPush)) return;
            EventGlobalManager.Ins.OnPlayerPushStep.RemoveListener(OnTrackingPlayerPush);
        }

        private void OnTrackingPlayerPush(PlayerStep playerPushStep)
        {
            if (IsPlayerMakeHintWrong)
            {
                GameplayManager.Ins.OnShowFocusBooster(false);
                // make the hint button show again
                GameplayManager.Ins.Screen.ActivePushHintIsland(true);
                IsPlayerMakeHintWrong = false;
                OnStopHint();
            }

            if (!IsStartHint) return;
            // Check if same step with the top of the stack
            if (playerPushStep.IsSameStep(_currentPlayerSteps.Peek()))
            {
                _currentPlayerSteps.Pop();
                _pops.Push(playerPushStep);
                if (_currentPlayerSteps.Count == 0)
                {
                    OnStopHint();
                    _islandCompleted.Add(playerPushStep.i);
                }
                else
                {
                    OnShowHint(_currentPlayerSteps.Peek(), _isShowHint);
                }
            }
            else
            {
                OnPauseHint();
                IsPlayerMakeHintWrong = true;
                GameplayManager.Ins.OnShowFocusBooster(true);
            }
        }

        private static void OnShowHint(PlayerStep playerStep, bool isShowHint = true)
        {
            GameplayManager.Ins.PushHintObject.MoveTo(playerStep.x, playerStep.y, playerStep.d, isShowHint);
        }

        public bool IsCompleted(int islandId)
        {
            return _islandCompleted.Contains(islandId);
        }
        
        public bool isHintActiveOnIsland(int islandId)
        {
            return _currentPlayerSteps is not null && _currentPlayerSteps.Peek().i == islandId;
        }
    }

    public class SavePushHint
    {
        private readonly List<PlayerStep> playerSteps = new();

        public static List<PlayerStep> ConvertJsonToPlayerSteps(string json)
        {
            PlayerStepsWrapper wrapper = JsonUtility.FromJson<PlayerStepsWrapper>(json);
            return wrapper.pS;
        }

        public void SaveStep(int x, int y, int direction, int islandID)
        {
            DevLog.Log(DevId.Hoang,
                $"Save step at cell {x}, {y}, with Direction {(Direction)direction}, on Island {islandID}");
            playerSteps.Add(new PlayerStep
            {
                x = x,
                y = y,
                d = direction,
                i = islandID
            });
        }

        public void Save()
        {
            Level level = LevelManager.Ins.CurrentLevel;
            // Get type
            LevelType levelType = level.LevelType;
            int levelIndex = level.Index;

            // if (levelIndex == 0 && levelType == LevelType.Normal) return; // Don't save Level 0

            // Get the text asset of the current level
            TextAsset data = DataManager.Ins.GetLevelData(levelType, levelIndex);
            RawLevelData rawLevelData = JsonGridDataHandler.CreateLevelData(levelType, levelIndex);

            string dataName = data.name;

            PlayerStepsWrapper wrapper = new() { pS = playerSteps };

            //  List to array the wrapper.pS
            PlayerStep[] playerStepsArray = wrapper.pS.ToArray();
            // Set RawLevelData pS to playerStepsArray
            rawLevelData.pS = playerStepsArray;

            // Convert rawLevelData to json
            string dataText = JsonUtility.ToJson(rawLevelData);

            string levelFolderName = levelType switch
            {
                LevelType.DailyChallenge => "DailyChallenge",
                LevelType.Secret => "Secret",
                _ => "Normal"
            };

            string path = $"Assets/_Game/Resources/Level/{levelFolderName}/{dataName}.json";
            File.WriteAllText(path, dataText);

            DevLog.Log(DevId.Hoang, $"Saved hint for {dataName} at {path}");
        }

        [Serializable]
        public class PlayerStepsWrapper
        {
            public List<PlayerStep> pS;
        }
    }
}
