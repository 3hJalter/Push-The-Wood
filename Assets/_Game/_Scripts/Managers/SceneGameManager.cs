using _Game.DesignPattern;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class SceneGameManager : Singleton<SceneGameManager>
{
    public event Action<int ,float> OnLoadingScene;
    public event Action<int> OnSceneLoaded;
    
    private int _id = 0;

    public int Id => _id;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadingSceneAsync(int id)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(id));
    }
    
    private IEnumerator LoadSceneAsyncCoroutine(int id)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
        _id = id;
        while (!asyncLoad.isDone)
        {
            float loadingPercentage = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            OnLoadingScene?.Invoke(_id, loadingPercentage);
            // Update your loading UI with the percentage
            yield return null;
        }
        // Scene is fully loaded; hide loading UI
        OnSceneLoaded?.Invoke(_id);
    }
}
