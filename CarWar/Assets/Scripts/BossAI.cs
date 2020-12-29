using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [SerializeField] private Boss_Data boss_Data;
    [SerializeField] private GameObject bossExplosionVFX;

    private int shotsBeforeDeath;
    private float speed;
    private float luckyChance; // Chance for Boss lucky strike
    private float bossTurnSpeed;

    private Vector3 aimingPosition;                     // aiming
    private List<Vector3> aimingPositionList;            // aiming

    private GameObject player;
    
    private GameObject vfxPool;
    private Vector2 bounds;
   
    private Vector3 normalizeDirection;
    private Vector3 fixedNormalizedDirection;
    public Action onBossReadyForAttack;
    public Action onPlayerCollided;
    public Action<int> onSomethingCollided;

    private Vector3 randomPosition;
    private bool isReadyForManeuer;
    Vector3 m_lastKnownPosition = Vector3.zero;
    private Quaternion m_lookAtRotation = Quaternion.identity;

    public Boss_Data BossData { get { return boss_Data;} }
    public float LuckyChance { get { return luckyChance; } private set { luckyChance = value;}}
    public int ShotsBeforeDeath { get { return shotsBeforeDeath; } private set { shotsBeforeDeath = value; } }

    private void Awake()
    {
        GameManager._instance.onBossStartAttack += StartAttack;
        GameManager._instance.onBossReduceLives += ReduceShotsBeforeDeath;
        GameManager._instance.onBossDeath += Destroy;
        vfxPool = GameObject.Find("VFX_Pool");
    }
    void Start()
    {
        shotsBeforeDeath = boss_Data.shotsBeforeDeath;
        speed = boss_Data.speed;
        luckyChance = boss_Data.luckyChance;
        bossTurnSpeed = boss_Data.bossTurnSpeed;

        aimingPositionList = new List<Vector3>(); // aiming

        player = GameObject.FindGameObjectWithTag("Player");
        bounds = player.GetComponent<HoverController>().bounds;
        
        normalizeDirection = (player.transform.position - transform.position).normalized;
        Debug.Log(normalizeDirection);        

        StartCoroutine("MoveToArena");      
    }

    private void StartAttack()
    {
        InvokeRepeating("SetPositionForManeuer", 1f, 3f);        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
        {
            MoveBoss();          
        }    
    }

    private void MoveBoss()
    {
        if (m_lastKnownPosition != randomPosition)
        {
            m_lastKnownPosition = randomPosition;
            m_lookAtRotation = Quaternion.LookRotation(m_lastKnownPosition - transform.position);
        }

        if (transform.rotation != m_lookAtRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_lookAtRotation, bossTurnSpeed * Time.deltaTime);
        }

        // Move our position a step closer to the target.
        float step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, randomPosition, step);     
    }

    IEnumerator MoveToArena()
    {
        if (GameManager._instance.CurrentGameState == GameManager.gameState.game) 
        {
            while (!isAtArena())
            {
                transform.LookAt(player.transform);

                transform.position += normalizeDirection * speed * Time.deltaTime;

                if (transform.position.y != player.transform.position.y)
                    transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);

                yield return new WaitForFixedUpdate();
            }

            onBossReadyForAttack?.Invoke();
            UnityEngine.Debug.Log("On Boss ready for attack");

            yield break;
        }
         
    }

    void SetPositionForManeuer() 
    {
        if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
        {
            if (UnityEngine.Random.Range(0, 100) <= luckyChance)
                randomPosition = player.transform.position;
            else
                randomPosition = new Vector3(UnityEngine.Random.Range(-bounds.x, bounds.x), player.transform.position.y, UnityEngine.Random.Range(-bounds.y, bounds.y));
        }                
    }

    bool isAtArena()
    {
        float offset = GetComponent<Collider>().bounds.size.x / 2 + 10;

        if (transform.position.x > bounds.x - offset || transform.position.x < -bounds.x + offset || transform.position.z > bounds.y - offset || transform.position.z < -bounds.y + offset)
            return false;
        else
            return true;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimingPosition, 1f);
    }

    private void Destroy()
    {
        //Destroy(this.gameObject);
        Instantiate(bossExplosionVFX, transform.position, Quaternion.identity, vfxPool.transform);
        GetComponent<Rigidbody>().useGravity = true;
    }

    private void ReduceShotsBeforeDeath(int currentValue)
    {
        ShotsBeforeDeath = currentValue;
    }

    private void OnTriggerEnter(Collider other)
    {
        onSomethingCollided?.Invoke(ShotsBeforeDeath);

        if (other.tag == "Player")
        {
            onPlayerCollided?.Invoke();
        }
    }

    private void OnDestroy()
    {
        GameManager._instance.onBossStartAttack -= StartAttack;
        GameManager._instance.onBossReduceLives -= ReduceShotsBeforeDeath;
        GameManager._instance.onBossDeath -= Destroy;
    }


    //TODO change boss turret logic to avoid multiple aiming point calculation (move it to bossAI script)
    
    //void SetAimingPosition()
    //{
    //    if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
    //    {
    //        //var chance = GetComponentInParent<BossAI>().LuckyChance;

    //        if (aimingPosition != player.transform.position)
    //        {
    //            if (UnityEngine.Random.Range(0, 100) <= luckyChance / 2f)
    //            {
    //                aimingPosition = predictedPosition(player.transform.position, transform.position, player.GetComponent<Rigidbody>().velocity, 100 /*shell.GetComponent<MGE_InitialForce>().z_str*/); //TODO Set speed of shell depending the actial speed of shell of corresponding turret 
    //                //Debug.Log("Boss lucky shot!");
    //            }
    //            else
    //            {
    //                aimingPosition = player.transform.position;
    //            }
    //        }
    //        //else
    //        //{
    //        //    //aimingPosition = target.transform.position;
    //        //    aimingPosition = new Vector3(player.transform.position.x + UnityEngine.Random.Range(0, maxDeviation), target.transform.position.y, target.transform.position.z + UnityEngine.Random.Range(0, maxDeviation));
    //        //}

    //        aimingPositionList.Add(aimingPosition);

    //        if (aimingPositionList.Count > 5)
    //            aimingPositionList.RemoveAt(0);
    //    }
    //}

    //private Vector3 predictedPosition(Vector3 targetPosition, Vector3 shooterPosition, Vector3 targetVelocity, float projectileSpeed)
    //{
    //    Vector3 displacement = targetPosition - shooterPosition;
    //    float targetMoveAngle = Vector3.Angle(-displacement, targetVelocity) * Mathf.Deg2Rad;
    //    //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
    //    if (targetVelocity.magnitude == 0 || targetVelocity.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / targetVelocity.magnitude)
    //    {
    //        Debug.Log("Position prediction is not feasible.");
    //        return targetPosition;
    //    }
    //    //also Sine Formula
    //    float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * targetVelocity.magnitude / projectileSpeed);
    //    return targetPosition + targetVelocity * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / targetVelocity.magnitude;
    //}

}
