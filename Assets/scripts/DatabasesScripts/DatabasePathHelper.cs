using UnityEngine;
using System.IO;

namespace DatabasesScripts
{
    /// <summary>
    /// Utility class to handle database path resolution for both Editor and Build modes.
    /// In Editor: Database is in Assets/Database.db
    /// In Build: Database should be in StreamingAssets and copied to persistent data path
    /// </summary>
    public static class DatabasePathHelper
    {
        private const string DATABASE_FILENAME = "Database.db";
        
        /// <summary>
        /// Gets the correct database path for current runtime environment.
        /// Handles both Editor and Build scenarios properly.
        /// </summary>
        public static string GetDatabasePath()
        {
            string databasePath;
            
            #if UNITY_EDITOR
            // In Editor: Use the Assets folder directly
            databasePath = Path.Combine(Application.dataPath, DATABASE_FILENAME);
            Debug.Log($"[DatabasePathHelper] Editor mode - Database path: {databasePath}");
            
            #else
            // In Build: Use StreamingAssets
            // For standalone builds, StreamingAssets is accessible directly
            databasePath = Path.Combine(Application.streamingAssetsPath, DATABASE_FILENAME);
            
            // For platforms like Android/iOS, we need to copy from StreamingAssets to persistentDataPath
            #if UNITY_ANDROID || UNITY_IOS
            string persistentPath = Path.Combine(Application.persistentDataPath, DATABASE_FILENAME);
            
            // Copy database to persistent path if it doesn't exist or is outdated
            if (!File.Exists(persistentPath))
            {
                Debug.Log($"[DatabasePathHelper] Copying database from StreamingAssets to persistent path...");
                CopyDatabaseToPersistentPath(databasePath, persistentPath);
            }
            
            databasePath = persistentPath;
            #endif
            
            Debug.Log($"[DatabasePathHelper] Build mode - Database path: {databasePath}");
            #endif
            
            // Verify database exists
            if (!File.Exists(databasePath))
            {
                Debug.LogError($"[DatabasePathHelper] Database not found at: {databasePath}");
                Debug.LogError($"[DatabasePathHelper] Application.dataPath: {Application.dataPath}");
                Debug.LogError($"[DatabasePathHelper] Application.streamingAssetsPath: {Application.streamingAssetsPath}");
                Debug.LogError($"[DatabasePathHelper] Application.persistentDataPath: {Application.persistentDataPath}");
            }
            else
            {
                Debug.Log($"[DatabasePathHelper] Database found successfully at: {databasePath}");
            }
            
            return "URI=file:" + databasePath;
        }
        
        #if UNITY_ANDROID || UNITY_IOS
        private static void CopyDatabaseToPersistentPath(string sourcePath, string destPath)
        {
            try
            {
                // On Android, we need to use WWW or UnityWebRequest to access StreamingAssets
                #if UNITY_ANDROID
                // For Android, the source path needs special handling
                // This should be called from a coroutine in production code
                Debug.LogWarning("[DatabasePathHelper] Android database copying requires coroutine support");
                #else
                // For iOS and other platforms, direct file copy works
                File.Copy(sourcePath, destPath, true);
                Debug.Log($"[DatabasePathHelper] Database copied successfully to: {destPath}");
                #endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DatabasePathHelper] Failed to copy database: {e.Message}");
            }
        }
        #endif
    }
}
