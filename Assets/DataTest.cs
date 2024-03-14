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
        var currencyData = data.Get<CurrencyData>();
        currencyData.Gold += 10;
        currencyData.Silver += 15;
        Debug.Log($"{currencyData.Gold}", LogContext.DataManager);
        Debug.Log($"{currencyData.Silver}", LogContext.DataManager);
    }

    public async void SaveLevel()
    {
        await data.SaveAllAsync();
    }
}