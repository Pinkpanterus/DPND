using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager _instance = null;

    public GameObject enemyPrefab;
    public GameObject BossPrefab;

    public Transform[] spawnPoints;

    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;
    public Transform[] topSpawnPoints;
    public Transform[] bottomSpawnPoints;

    public float spawnRate = 2f;
    public Action<GameObject> onEnemySpawn;
    public Action<GameObject> onBossSpawn;
    public Action onEnemyWaveSpawned;

    private GameObject enemiesParent;
    private GameManager gameManager;
    [SerializeField] private int enemyToSpawnedCount;
    [SerializeField] private int enemySpawnedCount;    
    private GameManager.gameState gameState;



    private void Awake()
    {
        
       
        
    }
    void Start()
    {
        enemiesParent = GameObject.Find("EnemiesPool");

        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance == this)
        {
            Destroy(gameObject);
        }

        leftSpawnPoints = spawnPoints.Where(x => x.position.x < -89).ToArray();
        rightSpawnPoints = spawnPoints.Where(x => x.position.x > 89).ToArray();
        topSpawnPoints = spawnPoints.Where(x => x.position.z > 59).ToArray();
        bottomSpawnPoints = spawnPoints.Where(x => x.position.z < -59).ToArray();

        foreach (var p in leftSpawnPoints)
        {
            p.transform.position = new Vector3(p.transform.position.x - 20,p.transform.position.y,p.transform.position.z);
        }

        foreach (var p in rightSpawnPoints)
        {
            p.transform.position = new Vector3(p.transform.position.x + 20, p.transform.position.y, p.transform.position.z);
        }

        foreach (var p in topSpawnPoints)
        {
            p.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z +20);
        }

        foreach (var p in bottomSpawnPoints)
        {
            p.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z - 20);
        }

        gameManager = GameManager._instance;
        gameManager.onGameStateChange += CheckGameState;
        gameManager.onEnemiesSpawnRequest += PrepareEnemyWave;
        gameManager.onBossFight += SpawnBoss;      

       
    }

    private void SpawnBoss(int obj)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Transform bossSpawnPoint = spawnPoints.OrderByDescending(s => Vector3.Distance(player.transform.position, s.transform.position)).First();       

        var boss = Instantiate(BossPrefab, new Vector3(bossSpawnPoint.position.x, player.transform.position.y, bossSpawnPoint.position.z), Quaternion.identity);
        UnityEngine.Debug.Log("Boss spawned");
        onBossSpawn?.Invoke(boss);
    }

    private IEnumerator SpawnEnemies(float waitTime)
    {       

        while (gameState != GameManager.gameState.game) 
        {
            yield return null;           
        }
       
        while (enemySpawnedCount < enemyToSpawnedCount)
        {
            yield return new WaitForSeconds(waitTime);
            enemySpawnedCount++;

            var trans = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length - 1)];
            var enemy = Instantiate(enemyPrefab, trans.position, Quaternion.identity, enemiesParent.transform);
            onEnemySpawn?.Invoke(enemy);
        }

        yield return new WaitForSeconds(4);
        onEnemyWaveSpawned?.Invoke();
        yield break;
    }

    private void PrepareEnemyWave(int enemyCountToSpawn)
    {
        enemySpawnedCount = 0;
        enemyToSpawnedCount = enemyCountToSpawn;
        UnityEngine.Debug.Log("Starting spawning eneies: " + enemyToSpawnedCount);

        StartCoroutine(SpawnEnemies(spawnRate));
    }

    private void CheckGameState(GameManager.gameState newGamestate)
    {
        gameState = newGamestate;

    }

    void Update()
    {
        
    }

   

    private void OnDestroy()
    {
        if (gameManager)
        {
            gameManager.onGameStateChange -= CheckGameState;
            gameManager.onEnemiesSpawnRequest -= PrepareEnemyWave;
            gameManager.onBossFight -= SpawnBoss;
        }
        
    }
}
    
