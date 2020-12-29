using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region Variables
    private static int id;
    [SerializeField] private Enemy_Data enemyData;
    public Transform targetPos;
    public GameObject explosionEffect;
    //public Action<GameObject> onInstantiate;
    public Action<int, GameObject> onEnemyTriggerEntered;
    public Action<GameObject> onEnemyDeletedFromScene;
    //public Action onPlayerCollision;

    private float startTime;
    private float journeyLength;
    private bool isDestroyByArribalInvoked; //to invoke only once 
    private GameObject vfxPool;
    private int score_SO;
    private AudioSource audiosource;

    public int EnemyID { get; private set;}
    public int Score_SO { get { return score_SO; } }

    [SerializeField] private float speed;    
    
    #endregion

 
    void Start()
    {
        id++;
        EnemyID = id;

        audiosource = GetComponent<AudioSource>();

        score_SO = enemyData.scoreForDestroyEnemy;

        vfxPool = GameObject.Find("VFX_Pool");

        GameManager._instance.onEnemyDestroyGranted += Destroy;
        GameManager._instance.onEnemyDestroyByArrivalGranted += DestroyByArrival;

        speed = enemyData.speed * GameManager._instance.CurrentLevel;
        // onInstantiate?.Invoke(this.gameObject);

        if (transform.position.x > 89)
           targetPos = EnemySpawnManager._instance.leftSpawnPoints[UnityEngine.Random.Range(0, EnemySpawnManager._instance.leftSpawnPoints.Length - 1)];

        if (transform.position.x < -89)
            targetPos = EnemySpawnManager._instance.rightSpawnPoints[UnityEngine.Random.Range(0, EnemySpawnManager._instance.rightSpawnPoints.Length - 1)];

        if (transform.position.z > 59)
            targetPos = EnemySpawnManager._instance.bottomSpawnPoints[UnityEngine.Random.Range(0, EnemySpawnManager._instance.bottomSpawnPoints.Length - 1)];

        if (transform.position.z < -59)
            targetPos = EnemySpawnManager._instance.topSpawnPoints[UnityEngine.Random.Range(0, EnemySpawnManager._instance.topSpawnPoints.Length - 1)];
                      

        startTime = Time.time;
        journeyLength = Vector3.Distance(transform.position, targetPos.position);

        transform.LookAt(targetPos);
    }


    void Update()
    {
        if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
        {
            
            MoveHover();
            DestroyByArrivalCheck();
            RotateTowards();
            audiosource.mute = false;
        }
        else 
        {            
            audiosource.mute = true;
        }
    }

   
    private void DestroyByArrivalCheck()
    {
        if (Vector3.Distance(targetPos.position, transform.position) < 1f)
        {
            if (!isDestroyByArribalInvoked)
            {
                Destroy(this.gameObject/*transform.parent.gameObject*/);
                onEnemyTriggerEntered?.Invoke(EnemyID, null);
            }

            isDestroyByArribalInvoked = true;
        }
    }

    private void MoveHover()
    {       
        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;
        //transform.LookAt(targetPos);
        transform.position = Vector3.Lerp(transform.position, targetPos.position, fracJourney);
    }

    void RotateTowards()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = targetPos.position - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }   

    private void OnTriggerEnter(Collider other)
    {
        onEnemyTriggerEntered?.Invoke(EnemyID, other.gameObject);
    }

    private void Destroy(int enemyDestoyedID) 
    {
        //Debug.Log("Destroing enemy");

        if (EnemyID == enemyDestoyedID)
        {
            Destroy(this.gameObject/*.transform.parent.gameObject*/);
            Instantiate(explosionEffect, transform.position, Quaternion.identity, vfxPool.transform);
        }
      
    }

    private void DestroyByArrival(int enemyDestoyedID) 
    {
        if (EnemyID == enemyDestoyedID)
        {
            Destroy(this.gameObject);            
        }
    }


    private void OnDestroy()
    {
        onEnemyDeletedFromScene?.Invoke(gameObject);

        GameManager._instance.onEnemyDestroyGranted -= Destroy;
        GameManager._instance.onEnemyDestroyByArrivalGranted -= DestroyByArrival;
    }


    
}
