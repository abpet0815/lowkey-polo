using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class GameSaveData
{
    public int score;
    public int comboLevel;
    public float comboTimer;
    public int matchesFound;
    public int totalMatches;
    public float gameTime;
    public Vector2Int boardSize;
    public int[] cardIDs;
    public bool[] matchedCards;
    public bool[] flippedCards;
    public string saveDateTime;
    public int gameVersion = 1;
}

public static class SaveLoadManager
{
    private static string SaveFileName = "memoryGameSave.json";
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
    
    public static void SaveGame(GameSaveData data)
    {
        try
        {
            data.saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            
            Debug.Log($"Game saved successfully to: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    
    public static GameSaveData LoadGame()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                
                Debug.Log($"Game loaded successfully from: {SaveFilePath}");
                Debug.Log($"Save date: {data.saveDateTime}");
                
                return data;
            }
            else
            {
                Debug.Log("No save file found");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return null;
        }
    }
    
    public static bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }
    
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("Save file deleted");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }
    
    public static string GetSaveFileInfo()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                FileInfo fileInfo = new FileInfo(SaveFilePath);
                return $"Save file: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
            }
            return "No save file found";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get save file info: {e.Message}");
            return "Error reading save file";
        }
    }
}
