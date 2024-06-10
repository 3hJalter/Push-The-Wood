using _Game._Scripts.InGame;
using _Game.Data;
using _Game.Managers;
using UnityEngine;

namespace _Game.GameGrid
{
    public static class JsonGridDataHandler
    {
        // Convert json file to LevelData
        public static RawLevelData CreateLevelData(LevelType type, int mapIndex) {
            return JsonUtility.FromJson<RawLevelData>(DataManager.Ins.GetLevelData(type, mapIndex).text);
        }
        
    }
}
