using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager _instance = null;

    [Header("Enemy quantity per wave")]
    public String enemyWavesProgression = "";   

    public Texture2D aimCursor;
    public Action<GameObject, Vector3, Quaternion> onFireGranted;
    public enum gameState { mainMenu, game, pause, win, lose };
    public Action<gameState> onGameStateChange;
    public Action<int> onLevelChange;
    public Action onLifeCountChange;
    public Action<int> onAmmoCountChange;
    public Action<int> onScoreChange;
    public Action onMaxScoreChange;
    public Action<int> onEnemyDestroyGranted;
    public Action<int> onEnemyDestroyByArrivalGranted;
    public Action onPlayerDestroyGranted;
    public Action<int> onEnemiesSpawnRequest;
    public Action<int> onBossFight;
    public Action<int> onBossReduceLives;
    public Action onBossDeath;
    public Action onBossStartAttack;
    public Action<int> onBossTurretFire;
    public Action<int, float> onTurretReload; 

    //public Text ammoText;
    //public Text scoreText;

    [SerializeField] private float ammoAddRate = 2.5f;
    [SerializeField] private int ammoCount = 30;
    [SerializeField] private int lifeCount = 1;
    [SerializeField] private int score;
    [SerializeField] private int maxScore;
    [SerializeField] private gameState currentGameState;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] private int currentLevel;
    private Dictionary<int,int> enemiesPerLevelDict = new Dictionary<int, int>();
    [SerializeField] private BossTurret[] bossTurrets;
    [SerializeField] int currenTTurretNumber = 1;
    private float bossNextShotTime = 0;
    private float bossShootDelay; //
    private float bossTurretReloadTime;

    private HoverController hoverController;
    private EnemySpawnManager enemySpawnManager;
    private OnMouseOverButton onMouseOverButton;
    private bool isOverPauseButton;    
    private UI_Manager ui_Manager;
    private GameObject player;


    public int AmmoCount { get { return ammoCount; } private set { ammoCount = value; onAmmoCountChange?.Invoke(AmmoCount); } }
    public int LifeCount { get { return lifeCount; } private set { lifeCount = value; onLifeCountChange?.Invoke(); } }
    public int CurrentLevel { get { return currentLevel;} set { currentLevel = value; onLevelChange?.Invoke(currentLevel);} }
    public gameState CurrentGameState { get { return currentGameState; } set { currentGameState = value; onGameStateChange?.Invoke(currentGameState); UnityEngine.Debug.Log("Game state changed. Current state is: " +currentGameState); } }
    public int Score { get { return score; } set { score = value; onScoreChange?.Invoke(score);} }
    public int MaxScore { get { return maxScore; } set { maxScore = value; onMaxScoreChange?.Invoke(); } }
   
    #endregion

   

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
    }

    void Start()
    {
        enemySpawnManager = GameObject.FindObjectOfType<EnemySpawnManager>();
       
        onMouseOverButton = GameObject.FindObjectOfType<OnMouseOverButton>();
        onMouseOverButton.onMouseChangePosition += ChangeCursorEvent;

        ui_Manager = GameObject.FindObjectOfType<UI_Manager>();
        ui_Manager.onPauseButtonPressed += ChangeGameState;
        ui_Manager.onResume += Resume;

        enemySpawnManager.onEnemySpawn += StartListenEnemyEvents; //register spawned enemy
        enemySpawnManager.onBossSpawn += StartListenBossEvents;
        enemySpawnManager.onEnemyWaveSpawned += LevelUp;
       
        Invoke("PrepareGame",0.1f);    
    }

    private void StartListenBossEvents(GameObject boss)
    {
        bossShootDelay = boss.GetComponent<BossAI>().BossData.bossShootDelay;
        bossTurretReloadTime = boss.GetComponent<BossAI>().BossData.bossTurretReloadTime;

        var bossAI = boss.GetComponent<BossAI>();
        bossAI.onBossReadyForAttack += StartBossAttack;
        bossAI.onPlayerCollided += GrantDestroyPlayer;
        bossAI.onSomethingCollided += ReduceBossLive;

        //var bossTurrets = GameObject.FindObjectsOfType<BossTurret>();
        //foreach (var b in bossTurrets)
        //    b.onProjectileSpawn += StartListenHitPlayer;

        UnityEngine.Debug.Log("Start listening boss events");
    }

    //private void StartListenHitPlayer(GameObject projectile)
    //{
    //    projectile.GetComponent<ExplodingProjectile>().onPlayerHit += GrantDestroyPlayer;
    //}

    private void ReduceBossLive(int currentBossLives)
    {
        if (currentBossLives > 0)
        {
            onBossReduceLives?.Invoke(--currentBossLives);
        }
        else
        {
            onBossDeath?.Invoke();
            
            if(CurrentGameState!=gameState.lose)
                ChangeGameState(gameState.win);            
        }
    }

    private void GrantDestroyPlayer()
    {
        onPlayerDestroyGranted?.Invoke();
        ChangeGameState(gameState.lose);
        hoverController.onPlayerDeath -= GrantDestroyPlayer;
        hoverController.onPlayerFire -= Shoot;
    }

    private void StartBossAttack()
    {        
        UnityEngine.Debug.Log("On boss starting attack (game manager)");

        bossTurrets = GameObject.FindObjectsOfType<BossTurret>();
        foreach (var t in bossTurrets)
        {
            t.onBossTurretAimed += FireTurret;
        }

        onBossStartAttack?.Invoke();        
    }

    private void FireTurret(int turretID, int ammoCount, float nextShotTime)
    {
        if (ammoCount == 0)
            onTurretReload?.Invoke(turretID, bossTurretReloadTime);

        //if (currenTTurretNumber > bossTurrets.Length)
        //    currenTTurretNumber = 1;


        if (ammoCount > 0 && Time.time >= nextShotTime && Time.time >= bossNextShotTime && CurrentGameState == gameState.game)
        {
            onBossTurretFire?.Invoke(turretID);
            bossNextShotTime = Time.time + bossShootDelay;

            //if (turretID == currenTTurretNumber)
            //{
            //    onBossTurretFire?.Invoke(turretID);
            //    currenTTurretNumber++;
            //    bossNextShotTime = Time.time + bossShootDelay;

            //    UnityEngine.Debug.Log("Invoked shot boss turret wit ID: " + turretID);
            //}
        }
    }

    private void LevelUp()
    {
        UnityEngine.Debug.Log("Enemy wave ended. Ready up for new wave!");

        if (CurrentLevel < enemiesPerLevelDict.Count)
        {
            UnityEngine.Debug.Log("Next level");

            ChangeLevel(++CurrentLevel);
            Invoke("StartSpawningEnemies", 1f);
        }          
        else
        {
            ChangeLevel(0);
            onBossFight?.Invoke(0);           
        }        
    }

    private void PrepareGame() 
    {
        SaveGameManager.Load();
        
        Score = SaveGameManager.saveFile.score;
        MaxScore = SaveGameManager.saveFile.highScore;
        CurrentLevel = SaveGameManager.saveFile.levelReached;       

        ParseWavesProgression();
        player = Instantiate(playerPrefab, new Vector3(0, 9, 0), Quaternion.identity);

        //var hoverController = player.GetComponent<HoverController>();

        hoverController = GameObject.FindObjectOfType<HoverController>();
        hoverController.onPlayerFire += Shoot;
        hoverController.onPlayerDeath += GrantDestroyPlayer;
        //hoverController.onProjectileSpawn += StartListenProjectileEvents;

        ChangeGameState(gameState.game);
        //onLevelChange?.Invoke(CurrentLevel);
        Invoke("StartSpawningEnemies", 1f);
    }

    //private void StartListenProjectileEvents(GameObject projectile)
    //{
    //    projectile.GetComponent<ExplodingProjectile>().onPlayerHit += GrantDestroyPlayer;
    //    projectile.GetComponent<ExplodingProjectile>().onProjectileDestroy += StopListenProjectileEvents;
    //}

    //private void StopListenProjectileEvents(GameObject projectile)
    //{
    //    projectile.GetComponent<ExplodingProjectile>().onPlayerHit -= GrantDestroyPlayer;
    //    projectile.GetComponent<ExplodingProjectile>().onProjectileDestroy -= StopListenProjectileEvents;
    //}

    private void StartSpawningEnemies() 
    {
        if (CurrentLevel == 0)
            onBossFight?.Invoke(0);
        else
            onEnemiesSpawnRequest?.Invoke(enemiesPerLevelDict[CurrentLevel]);
        //Debug.Log("Need to spawn enemies "+ enemiesPerLevelDict[CurrentLevel]);
    }

    private void Resume()
    {
        isOverPauseButton = false;
        CurrentGameState = gameState.game;

        CheckAmmoIncreasePossibility();
    }

    private void ChangeGameState(gameState state)
    {
        CurrentGameState = state;
        onGameStateChange?.Invoke(state);

        CheckAmmoIncreasePossibility();
    }

    private void ChangeLevel(int levelNumber) 
    {
        CurrentLevel = levelNumber;
        UnityEngine.Debug.Log("Level changed to " + CurrentLevel);        
        
        //SaveGameManager.Save(levelNumber);
    }

    private void ChangeCursorEvent(bool state)
    {
        isOverPauseButton = state;
    }

    private void StopListenEnemyEvents(GameObject go) 
    {
        var enemyController = go.GetComponentInChildren<EnemyController>();
        enemyController.onEnemyTriggerEntered -= CheckEnemyDestroy; //gameManager signed for destroy event      
        enemyController.onEnemyDeletedFromScene -= StopListenEnemyEvents;
    }

    private void StartListenEnemyEvents(GameObject go)
    {
        var enemyController = go.GetComponentInChildren<EnemyController>();
        enemyController.onEnemyTriggerEntered += CheckEnemyDestroy; //gameManager signed for destroy event      
        enemyController.onEnemyDeletedFromScene += StopListenEnemyEvents;
    }

    private void CheckEnemyDestroy(int enemyID, GameObject other)
    {
        if (other)
        {
            if (other.tag == "Enemy")
            {
                onEnemyDestroyGranted?.Invoke(other.GetComponent<EnemyController>().EnemyID);
                onEnemyDestroyGranted?.Invoke(enemyID);
            }
            else if (other.tag == "Player")
            {
                
                GrantDestroyPlayer();
                onEnemyDestroyGranted?.Invoke(enemyID);
                ChangeGameState(gameState.lose);
            }
            else if (other.tag == "PlayerShell")
            {                
                onEnemyDestroyGranted?.Invoke(enemyID);
                AddScore(enemyID);
            }
        }
        else 
        {
            onEnemyDestroyByArrivalGranted?.Invoke(enemyID);
        }        
    }

    private void AddScore(int enemyID)
    {
        EnemyController enemy = GameObject.FindObjectsOfType<EnemyController>().Where(x => x.EnemyID == enemyID).First();
        Score += enemy.Score_SO * currentLevel;     
    }

    void CheckAmmoIncreasePossibility() 
    {      

        if (CurrentGameState == gameState.game)
        {
            InvokeRepeating("AmmoAdd", 0, ammoAddRate);            
        }
        else
        {
            CancelInvoke("AmmoAdd");            
        }
    }

  

    void Update()
    {
        
    }
 

    private void AmmoAdd() 
    {
        if (CurrentGameState == gameState.game)
            AmmoCount++;
    }

    private void Shoot(GameObject shell, Vector3 pos, Quaternion rot) 
    {
        if (ammoCount > 0 && !isOverPauseButton && CurrentGameState == gameState.game)
        {
            onFireGranted?.Invoke(shell, pos, rot);            
            ammoCount--;
        }
           
    }

    private void OnDestroy()
    {

        //enemySpawnManager.onEnemySpawn -= StartListenEnemyEvents;
        //enemySpawnManager.onEnemyWaveSpawned -= LevelUp;
        if (ui_Manager)
        {
            ui_Manager.onResume -= Resume;
            ui_Manager.onPauseButtonPressed -= ChangeGameState;
        }      


        foreach (var t in bossTurrets)
        {
            t.onBossTurretAimed -= FireTurret;
        }
    }

    void ParseWavesProgression()
    {
        string[] wavesStrings = enemyWavesProgression.Split(';');
        int enemyCount;
        
        for (int i = 0; i < wavesStrings.Length; i++)
        {
            if (!int.TryParse(wavesStrings[i], out enemyCount))
            {
                UnityEngine.Debug.LogError("Enemy count can`t be evaluated");
                return; // This is throwing an error anyway, so we'll exit the method. 
            }          
            else 
            {
                enemiesPerLevelDict.Add(i + 1, enemyCount);
            }
        }

        UnityEngine.Debug.Log("Enemy waves parsed successfully");

        foreach (var v in enemiesPerLevelDict.Values)
            UnityEngine.Debug.Log(v);        
    }
}

