using UnityEngine;
using System.Collections;
using System;


[RequireComponent(typeof(Rigidbody))]
public class HoverController : MonoBehaviour
{
	#region Variables

	[SerializeField] public Player_Data playerData;
	[SerializeField] private float speed;
	[SerializeField] private float turnSpeed;
	[SerializeField] private float turretTurnSpeed;
	

	private float powerInput; 
	private float turnInput;
	private Vector3 mousePosition;


	private float fireRate;
	private Rigidbody hoverBody;	
	private float nextShotTime = 0;
	private GameObject shellParent;
	private GameObject vfxPool;

	private bool isCollidedwithBorder;

	public GameObject shootPoint;
	public GameObject tower;
	public GameObject shell;
	public Vector2 bounds;
	public Action<GameObject, Vector3, Quaternion> onPlayerFire;
	public Action onBorderCollision;
	public Action onPlayerDeath;	
	public GameObject playerExplosionEffect;	
    public bool IsCollidedwithBorder { get { return isCollidedwithBorder; } set { isCollidedwithBorder = value; } }


    #endregion


    private void Start()
    {
		// Get the rigidbody
		hoverBody = GetComponent<Rigidbody>();
		shellParent = GameObject.Find("PlayerShellPool");
		vfxPool = GameObject.Find("VFX_Pool");

		// If no cannon is assigned we throw an error. This means using a cannon is mandatory
		if (tower == null)
		{
			throw new MissingReferenceException("No cannon attached!");
		}

		speed = playerData.speed;
		turnSpeed = playerData.rotationSpeed;
		turretTurnSpeed = playerData.turretRotationSpeed;
		fireRate = playerData.fireRate;

		GameManager._instance.onFireGranted += Fire;
		GameManager._instance.onPlayerDestroyGranted += DestroyPlayer;

		float offset = 5f;
		float height = Camera.main.orthographicSize - offset;
		float width = (height + offset) * Camera.main.aspect - offset;
		bounds = new Vector2(width, height);
		Debug.Log(bounds);
	}

    private void DestroyPlayer()
    {
		Destroy(this.gameObject);
		Instantiate(playerExplosionEffect, transform.position, Quaternion.identity, vfxPool.transform);
	}

	void ArenaStayCheck()
	{
		if (transform.position.x > bounds.x)
		{
			transform.position = new Vector3(bounds.x, transform.position.y, transform.position.z);

            if (!isCollidedwithBorder)
            {
                isCollidedwithBorder = true;
                onBorderCollision?.Invoke();
            }

        }
		else if (transform.position.x < -bounds.x)
		{
			transform.position = new Vector3(-bounds.x, transform.position.y, transform.position.z);

            if (!isCollidedwithBorder)
            {
                isCollidedwithBorder = true;
                onBorderCollision?.Invoke();
            }
        }
		else if (transform.position.z > bounds.y)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, bounds.y);

            if (!isCollidedwithBorder)
            {
                isCollidedwithBorder = true;
                onBorderCollision?.Invoke();
            }
        }
		else if (transform.position.z < -bounds.y)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, -bounds.y);

            if (!isCollidedwithBorder)
            {
                isCollidedwithBorder = true;
                onBorderCollision?.Invoke();
            }
        }     
    }

    private void OnTriggerEnter(Collider other)
    {
		onPlayerDeath?.Invoke();
		Debug.Log("Player trigger invoked for death");
	}
   

    public void Update()
	{
		if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
		{
			// The input of W and S key
			powerInput = Input.GetAxis("Vertical");

			// The input of A and D key
			turnInput = Input.GetAxis("Horizontal");

			ArenaStayCheck();
		}
	}

	public void FixedUpdate()
	{
		if (GameManager._instance.CurrentGameState == GameManager.gameState.game)
		{
			MoveHover();
			RotateTower();
			FireButtonPressCheck();
		}	
	}

    private void FireButtonPressCheck()
    {		
		if (Time.time > nextShotTime && Input.GetMouseButton(0))
		{
			onPlayerFire?.Invoke(shell, shootPoint.transform.position, shootPoint.transform.rotation);			
			nextShotTime = Time.time + fireRate;
		}
	}

	private void Fire(GameObject shell, Vector3 pos, Quaternion rot) 
	{
		var projectile = Instantiate(shell, pos, rot, shellParent.transform);		
	}


	private void MoveHover()
	{
		// Add a relative force according to power input and speed settings
		hoverBody.AddRelativeForce(new Vector3(0f, 0f, powerInput * speed));

		Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, turnInput * turnSpeed, 0f) * Time.deltaTime);
		hoverBody.MoveRotation(hoverBody.rotation * deltaRotation);
	}
		
	void RotateTower() 
	{
		Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit hit;

		if (Physics.Raycast(screenRay, out hit)) 
		{
			mousePosition = hit.point;
			//mousePositionNormal = hit.normal;
		}

		if (tower)
		{
			mousePosition.y = tower.transform.position.y;

			Quaternion newRotation = Quaternion.LookRotation(mousePosition - tower.transform.position);

			tower.transform.rotation = Quaternion.RotateTowards(tower.transform.rotation, newRotation, turretTurnSpeed * Time.deltaTime);
		}
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mousePosition, 1f);
    }

    private void OnDestroy()
    {
		GameManager._instance.onFireGranted -= Fire;
		GameManager._instance.onPlayerDestroyGranted -= DestroyPlayer;
	}
}
