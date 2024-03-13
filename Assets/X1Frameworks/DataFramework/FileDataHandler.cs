using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using UnityEngine;
using X1Frameworks.LogFramework;
using Zenject;
using Debug = X1Frameworks.LogFramework.Debug;

namespace X1Frameworks.DataFramework
{
    public class FileDataHandler: IInitializable, IDataHandler
    {
        private const string LocalDataFolderPrefix = "LocalData";
        public void Initialize()
        {
            AotHelper.EnsureList<int>();
            AotHelper.EnsureList<string>();    
        }

        public void Load(BaseData data, Action onComplete)
        {
            var dataType = data.GetType();
            var folderPath = GetFolderPath();
            var filePath = GetFilePath(dataType.Name);
            
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"{folderPath} directory not found", LogContext.DataFramework);
                onComplete?.Invoke();
                return;
            }
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning( $"{filePath} file not found", LogContext.DataFramework);
                onComplete?.Invoke();
                return;
            }
            
            var json = File.ReadAllText(filePath);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"{filePath} loaded file is empty", LogContext.DataFramework);
                onComplete?.Invoke();
                return;
            }
            
            try
            {
                JsonConvert.PopulateObject(json, data);
                Debug.Log($"{filePath} loaded from FileSystem", LogContext.DataFramework);
            }
            catch (Exception e)
            {
                Debug.LogError( $"{filePath} json parse error\n {e.Message}", LogContext.DataFramework);
            }
            onComplete?.Invoke();
        }

        public void Save(BaseData data, Action onComplete)
        {
            var dataType = data.GetType();
            var folderPath = GetFolderPath();
            var filePath = GetFilePath(dataType.Name);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            var json = string.Empty;
            try
            {
                json = JsonConvert.SerializeObject(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"{filePath} json serialization error\n{e.Message}", LogContext.DataFramework);
            }
            
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"{filePath} error when writing the file\n{e.Message}", LogContext.DataFramework);
            }
            
            Debug.Log( $"{filePath} saved to FileSystem", LogContext.DataFramework);
            
            onComplete?.Invoke();
        }

        public static string GetFolderPath()
        {
            return Application.persistentDataPath + "/" + LocalDataFolderPrefix;
        }
        public static string GetFilePath(string dataKey)
        {
            return GetFolderPath() + "/" + dataKey + ".data";
        }
    }
}