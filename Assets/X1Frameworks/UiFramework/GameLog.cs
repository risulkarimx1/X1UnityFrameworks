using UnityEngine;
using X1Frameworks.UiFramework;

namespace Vengadores.UIFramework
{
    internal static class GameLog
    {
        public static void LogError(string uiframe, string message)
        {
            Debug.LogError($"{uiframe} - {message}");
        }
    }
}