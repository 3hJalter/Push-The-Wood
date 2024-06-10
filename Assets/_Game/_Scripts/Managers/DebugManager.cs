using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Utilities.Grid;
using System.Collections;
using System.Collections.Generic;
using _Game.Data;
using UnityEngine;
using _Game.Managers;
using _Game.UIs.Screen;

public class DebugManager : SimpleSingleton<DebugManager>
{
    private Grid<GameGridCell, GameGridCellData>.DebugGrid debugGrid;
    [SerializeField]
    GameObject FpsDebug;
    [SerializeField]
    GameObject LogDebug;
    [SerializeField]
    bool isShowAds = false;
    List<UICanvas> DebugCanvass = new List<UICanvas>();

    int level = -1;
    public bool IsDebugGridLogic => debugGrid != null ? true : false;
    public int Level => level;
    public bool IsShowAds => isShowAds;
    public void DebugGridData(Grid<GameGridCell, GameGridCellData> grid)
    {
        debugGrid?.DrawGrid(grid, true);
    }
    public void OpenDebugCanvas(UI_POSITION position)
    {
        if(position == UI_POSITION.NONE) return;
        for (int i = 0; i < DebugCanvass.Count; i++)
        {
            DebugCanvass[i].Close();
        }
        UICanvas canvas;       
        switch (position)
        {
            case UI_POSITION.MAIN_MENU:
                canvas = UIManager.Ins.OpenUI<DebugMainMenuScreen>();
                if(!DebugCanvass.Contains(canvas))
                    DebugCanvass.Add(canvas);
                break;
            case UI_POSITION.IN_GAME:
                canvas = UIManager.Ins.OpenUI<DebugInGameScreen>();
                if (!DebugCanvass.Contains(canvas))
                    DebugCanvass.Add(canvas);
                break;
        }
        
    }
    public void OnInit(bool isDebugGridLogic, bool isDebugFps, bool isDebugLog, bool isShowAds, int level = -1)
    {
        _instance = this;
        if(isDebugGridLogic)
        {
            debugGrid = new Grid<GameGridCell, GameGridCellData>.DebugGrid();
        }
        FpsDebug.SetActive(isDebugFps);
        LogDebug.SetActive(isDebugLog);
        this.isShowAds = isShowAds;
        this.level = level;
        // Save the level to database
        GameData gameData = Database.LoadData();
        gameData.user.normalLevelIndex = level;
        Database.SaveData(gameData);
    }
}
