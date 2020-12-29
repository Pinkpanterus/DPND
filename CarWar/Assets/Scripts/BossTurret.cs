using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.PlayerLoop;

public class BossTurret : MonoBehaviour
{
    public Turret_Data turretData;

    private float turretTurnSpeed;
    private float fireRate;
    private int ammoCount;
    private float maxDeviation;
    private GameObject shell;

    [SerializeField] private static int ID;
    [SerializeField] private int turretID;
        
    private float nextShotTime = 0;
    private GameObject shellParent;
    private GameObject vfxPool;
    public GameObject shootPoint;
    public Action<int> onBossTurretFire;
    public Action<int,int,float> onBossTurretAimed;
    //public Action<GameObject> onProjectileSpawn;
    private Vector3 aimingPosition;

    private List<Vector3> aimingPositionList;
    private int CurrentNumberInPositionsList;
    private int aimedCurrentNumber;

    Quaternion aiming_lookAtRotation = Quaternion.identity;
    [SerializeField] private GameObject target;
    bool isRotating;
    
    public int AmmoCount { get { return ammoCount; } private set { ammoCount = value; } }
    public int TurretID { get { return turretID;} private set { turretID = value;} }

    // Start is called before the first frame update

    private void Awake()
    {
        ID++;
        TurretID = ID;

        GameManager._instance.onBossStartAttack += StartAttack;
        GameManager._instance.onBossTurretFire += Fire;
        GameManager._instance.onTurretReload += EnableReloading;

        turretTurnSpeed = turretData.turretTurnSpeed;
        fireRate = turretData.fireRate;
        ammoCount = turretData.ammoCount;
        maxDeviation = turretData.maxDeviation;
        shell = turretData.shell;

        //UnityEngine.Debug.Log("Turret ID: " + bossTurretID);      
    }


    private void EnableReloading(int reloadingTurretID, float reloadingDelay)
    {
        if (TurretID == reloadingTurretID)
            Invoke("TurretReload", reloadingDelay);
    }

    private void TurretReload() 
    {
        AmmoCount = turretData.ammoCount;
    }

    void Start()
    {
        shellParent = GameObject.Find("PlayerShellPool");
        vfxPool = GameObject.Find("VFX_Pool");
        target = GameObject.FindGameObjectWithTag("Player");

        aimingPositionList = new List<Vector3>();
        aimingPositionList.Add(Vector3.zero);     
    }

    private void StartAttack()
    {        
        InvokeRepeating("SetAimingPosition", 0, 0.2f);        
        UnityEngine.Debug.Log("Started function AimTurret (boss turret)");
    }


    //private IEnumerator CheckAimTurret(int turretID)
    //{
    //    yield return new WaitForSeconds(0.1f);  

    //    if (TurretID == turretID)
    //    {            
    //        var lookingAtRotation = Quaternion.LookRotation(aimingPositionList[aimedCurrentNumber] - transform.position);

    //        if (transform.rotation == lookingAtRotation)
    //        {
    //            UnityEngine.Debug.Log("On boss aimed turret with ID: " + TurretID);
    //            onBossTurretAimed?.Invoke(TurretID, AmmoCount, nextShotTime);
    //            aimedCurrentNumber++;
    //            yield break;
    //        }
    //    }
    //}

    void RotateTower() 
    {
        if (aimingPositionList.Count > 0) 
        {
            aiming_lookAtRotation = Quaternion.LookRotation(aimingPositionList[aimingPositionList.Count - 1] - transform.position);

            if (transform.rotation != aiming_lookAtRotation /*&& !isRotating*/)
            {
                //isRotating = true;   

                do
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, aiming_lookAtRotation, turretTurnSpeed * Time.deltaTime);
                while (transform.rotation != aiming_lookAtRotation);
            }
            else
            {
                UnityEngine.Debug.Log("On boss aimed turret with ID: " + TurretID);
                onBossTurretAimed?.Invoke(TurretID, AmmoCount, nextShotTime);

                aimingPositionList.RemoveAt(0);
                //aimedCurrentNumber++;

                //isRotating = false;
            }
        }
    }


    void SetAimingPosition() 
    {
        if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
        {
            var chance = GetComponentInParent<BossAI>().LuckyChance;

            if (aimingPosition != target.transform.position)
            {
                if (UnityEngine.Random.Range(0, 100) <= chance / 2f)
                {
                    aimingPosition = predictedPosition(target.transform.position, transform.position, target.GetComponent<Rigidbody>().velocity, shell.GetComponent<MGE_InitialForce>().z_str);
                    //Debug.Log("Boss lucky shot!");
                }
                else
                {
                    aimingPosition = target.transform.position;
                }
            }
            else
            {            
                aimingPosition = new Vector3(target.transform.position.x + UnityEngine.Random.Range(0, maxDeviation), target.transform.position.y, target.transform.position.z + UnityEngine.Random.Range(0, maxDeviation));
            }

            aimingPositionList.Add(aimingPosition);
        }         
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimingPosition, 1f);
    }

    private void Fire(int turretId)
    {
        if (TurretID == turretId)
        {
            var projectile = Instantiate(shell, shootPoint.transform.position, shootPoint.transform.rotation, shellParent.transform);
            //onProjectileSpawn?.Invoke(projectile);
            nextShotTime = Time.time + fireRate;
            AmmoCount--;         

            UnityEngine.Debug.Log("Shooted turret with ID" + TurretID);
        }        
    }
   
    void Update()
    {        
        RotateTower();
    }

    private Vector3 predictedPosition(Vector3 targetPosition, Vector3 shooterPosition, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 displacement = targetPosition - shooterPosition;
        float targetMoveAngle = Vector3.Angle(-displacement, targetVelocity) * Mathf.Deg2Rad;
        //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
        if (targetVelocity.magnitude == 0 || targetVelocity.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / targetVelocity.magnitude)
        {
            Debug.Log("Position prediction is not feasible.");
            return targetPosition;
        }
        //also Sine Formula
        float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * targetVelocity.magnitude / projectileSpeed);
        return targetPosition + targetVelocity * displacement.magnitude / Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / targetVelocity.magnitude;
    }

    private void OnDestroy()
    {
        GameManager._instance.onBossStartAttack -= StartAttack;
        GameManager._instance.onBossTurretFire -= Fire;
        GameManager._instance.onTurretReload -= EnableReloading;
    }
}
