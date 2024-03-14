using System;
using UnityEngine;
using X1Frameworks.DataFramework;
using X1Frameworks.LogFramework;
using Debug = X1Frameworks.LogFramework.Debug;

public class DataTest : MonoBehaviour
{
    private DataManager data;
    private void Start()
    {
        data = new DataManager();
    }

    public void AddLevel()
    {
        var playerData =  data.Get<PlayerData>();
        playerData.Level++;
        Debug.Log(playerData.Level.ToString(), LogContext.DataManager);
    }

    public async void SaveLevel()
    {
        await data.SaveAsync<PlayerData>();
    }
}