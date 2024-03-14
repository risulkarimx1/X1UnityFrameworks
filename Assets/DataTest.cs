using System;
using UnityEngine;
using X1Frameworks.DataFramework;

public class DataTest : MonoBehaviour
{
    private DataManager data;
    private void Start()
    {
        data = new DataManager();
    }

    public void AddLevel()
    {
        data.AddPlayerLevel();
    }

    public void SaveLevel()
    {
        data.SavePlayerData();
    }
}