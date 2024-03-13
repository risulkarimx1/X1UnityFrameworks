using System.Collections.Generic;
using UnityEngine;

namespace X1Frameworks.LogFramework
{
    public enum LogContext : byte
    {
        UIFrame = 0,
        DataFramework = 1,
    }
    public static partial class Debug
    {
        private static readonly Dictionary<LogContext, Color> colorCache = new();
        private static Color GenerateColorForEnum(LogContext context)
        {
            // Check if the color is already in the cache
            if (colorCache.TryGetValue(context, out var cachedColor))
            {
                return cachedColor;
            }

            // Hash the context to get a unique, consistent integer
            var hash = context.GetHashCode();

            // Use the hash as a seed to ensure the same context always gives the same color
            var random = new System.Random(hash);

            // Generate bright, vibrant colors
            // Favoring high brightness and light tones
            var hue = (float)random.NextDouble(); // Hue between 0 and 1
            var saturation = 0.2f + (float)random.NextDouble() * 0.5f; // Saturation between 0.5 and 1.0
            var brightness = 0.9f + (float)random.NextDouble() * 0.1f; // Brightness between 0.8 and 1.0

            var color = Color.HSVToRGB(hue, saturation, brightness);

            // Cache the color for future use
            colorCache[context] = color;

            return color;
        }
        
        public static void Log(string message, LogContext context)
        {
            var color = GenerateColorForEnum(context);

            // Convert color to hex format
            var hexColor = ColorUtility.ToHtmlStringRGB(color);

            UnityEngine.Debug.Log($"<color=#{hexColor}>[{context}]: {message}</color>");
        }

        public static void LogError(string message, LogContext context)
        {
            var color = GenerateColorForEnum(context);

            // Convert color to hex format
            var hexColor = ColorUtility.ToHtmlStringRGB(color);

            UnityEngine.Debug.LogError($"<color=#{hexColor}>[{context}]: {message}</color>");
        }

        public static void LogWarning(string message, LogContext context)
        {
            var color = GenerateColorForEnum(context);

            // Convert color to hex format
            var hexColor = ColorUtility.ToHtmlStringRGB(color);

            UnityEngine.Debug.LogWarning($"<color=#{hexColor}>[{context}]: {message}</color>");
        }
    }
}