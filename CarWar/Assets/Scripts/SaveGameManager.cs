//#define DEBUG_VerboseConsoleLogging

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveGameManager: MonoBehaviour
{
    // ———————————————— Statics ———————————————— //
    // I've chosen not to use SNAKE_CASE for statics in this class because everything is static.
    static public SaveFile saveFile;
    static private string filePath;
    // LOCK, if true, prevents the game from saving. This avoids issues that can
    //  happen while loading files.
    static public bool LOCK
    {
        get;
        private set;
    }

    static public SaveGameManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance == this)
        {
            Destroy(gameObject);
        }


        int saveGameManager = FindObjectsOfType<SaveGameManager>().Length;
        if (saveGameManager != 1)
        {
            Destroy(this.gameObject);
        }        
        else
        {
            DontDestroyOnLoad(gameObject);
        }


        saveFile = new SaveFile();
        filePath = Application.persistentDataPath + "/CarWar.save";
    }


    private void OnLevelWasLoaded(int level)
    {
        if (level == 1)
            GameManager._instance.onLevelChange += Save;
    }

    

    private void Update()
    {
        
    }


    static public void Save(int levelNum)
    {
        CheckHighScore(GameManager._instance.Score);
        saveFile.levelReached = GameManager._instance.CurrentLevel;
        saveFile.score = GameManager._instance.Score;

        // If this is LOCKed, don't save
        if (LOCK) return;

        string jsonSaveFile = JsonUtility.ToJson(saveFile, true);

        File.WriteAllText(filePath, jsonSaveFile);

        Debug.Log("Progress is saved. Current level:" +levelNum);

    }


    static public void Load()
    {
        ResumeSavedDate();
    }

    static public void ResumeSavedDate() 
    {
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);

            try
            {
                saveFile = JsonUtility.FromJson<SaveFile>(dataAsJson);
                //Debug.Log("SaveGameManager:Load() – File text is:\n" + dataAsJson);

                //SceneManager.LoadScene(1);
                //GameManager._instance.CurrentGameState = GameManager.gameState.pause;
                //GameManager._instance.CurrentLevel = saveFile.levelReached;
                //GameManager._instance.Score = saveFile.score;
                //GameManager._instance.MaxScore = saveFile.highScore;
                //GameManager._instance.CurrentGameState = GameManager.gameState.game;

                Debug.Log("Progress is loaded. Current level:" + GameManager._instance.CurrentLevel);
            }
            catch
            {
                Debug.LogWarning("SaveGameManager:Load() – SaveFile was malformed.\n" + dataAsJson);
                return;
            }

#if DEBUG_VerboseConsoleLogging
                        Debug.Log("SaveGameManager:Load() – Successfully loaded save file.");
#endif
            // SceneManager.LoadScene(1);
        }
    }


    static public void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            saveFile = new SaveFile();
            Debug.Log("SaveGameManager:DeleteSave() – Successfully deleted save file.");
        }
        else
        {
            Debug.LogWarning("SaveGameManager:DeleteSave() – Unable to find and delete save file!"
                + " This is absolutely fine if you've never saved or have just deleted the file.");
        }        
    }


    static internal bool CheckHighScore(int score)
    {
        if (score > saveFile.highScore)
        {
            saveFile.highScore = score;
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if(GameManager._instance)
            GameManager._instance.onLevelChange -= Save;      
    }
}



// A class must be serializable to be converted to and from JSON by JsonUtility.
[System.Serializable]
public class SaveFile
{
    public int levelReached = 1;
    public int score;
    public int highScore;  
}