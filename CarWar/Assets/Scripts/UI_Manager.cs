using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class UI_Manager : MonoBehaviour
{
    
    public Texture2D aimCursor;

    public Action<GameManager.gameState> onPauseButtonPressed;
    public Action onResume;

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    //[SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;

    [SerializeField] private Text ammoText;
    [SerializeField] private Text scoreText;

    private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private GameManager gameManager;
    private OnMouseOverButton pauseButton;
    private bool isOverPauseButton;

    public GameObject levelGameobj;
    public Vector3 startPosition, offscreenPosition;
    public float offscreenYAdj;
    public float moveSpeed = 10f;
    public float movePauseTime = 2f;

    void Start()
    {
        gameManager = GameManager._instance;
        gameManager.onPlayerDestroyGranted += GameOverPanel;
        gameManager.onBossDeath += WinPanel;
        gameManager.onGameStateChange += CheckCursorState;
        gameManager.onLevelChange += StartShowWaveNumber;
        //gameManager.onAmmoCountChange += ShowNewAmmoCount;
        gameManager.onScoreChange += ShowNewScore;

        pauseButton = GameObject.FindObjectOfType<OnMouseOverButton>();
        pauseButton.onMouseChangePosition += ChangeIsOverMouseButton;

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.clip = clickSound;

        startPosition = levelGameobj.transform.position;
        offscreenPosition = startPosition;
        offscreenPosition.y += offscreenYAdj;
        levelGameobj.transform.position = offscreenPosition;

        CheckCursorState(gameManager.CurrentGameState);
    }

    private void ShowNewScore(int score)
    {
        scoreText.text = "Score: " + score.ToString("D4");
    }

    private void ShowNewAmmoCount(int ammoCount)
    {
        ammoText.text = "Ammo: " + ammoCount.ToString();
    }


    private void StartShowWaveNumber(int levelNum)
    {
        IEnumerator coroutine = ShowWaveNumber(levelNum);
        StartCoroutine(coroutine);
    }

    private void ChangeIsOverMouseButton(bool state)
    {
        isOverPauseButton = state;
        CheckCursorState(gameManager.CurrentGameState);
    }

    void CheckCursorState(GameManager.gameState gameState)
    {
#if UNITY_WEBGL
        if (gameState == GameManager.gameState.game && !isOverPauseButton)
        {
            UnityEngine.Cursor.SetCursor(aimCursor, new Vector2(aimCursor.width / 2, aimCursor.height / 2), CursorMode.ForceSoftware);
        }
        else
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }
#else
        if (gameState == GameManager.gameState.game && !isOverPauseButton)
        {
            UnityEngine.Cursor.SetCursor(aimCursor, new Vector2(aimCursor.width / 2, aimCursor.height / 2), CursorMode.Auto);
        }
        else
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
#endif
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        ShowNewAmmoCount(GameManager._instance.AmmoCount);
    }

    private void GameOverPanel() 
    {
        StartCoroutine("GameOver");
    }

    private IEnumerator GameOver() 
    {
        yield return new WaitForSeconds(2f);        
        SceneManager.LoadScene(2);
    }

    private void WinPanel()
    {
        StartCoroutine("Win");
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(2f);

        //gamePanel.SetActive(false);
        //winPanel.SetActive(true);
        SceneManager.LoadScene(3);
    }


    IEnumerator ShowWaveNumber(int levelNumber)
    {
        if(levelNumber > 0)
            levelGameobj.GetComponentInChildren<Text>().text = "Enemy wave " + levelNumber;
        else
            levelGameobj.GetComponentInChildren<Text>().text = "Boss fight";

        float step = (moveSpeed / (offscreenPosition - startPosition).magnitude * Time.fixedDeltaTime);
        float t = 0;
        float u;
        while (t <= 1.0f)
        {
            t += step;
            u = 1 - (1 - t) * (1 - t); // This does some fancy easing on the Lerp
            levelGameobj.transform.position = Vector3.LerpUnclamped(offscreenPosition, startPosition, u);
            yield return new WaitForFixedUpdate();
        }
        levelGameobj.transform.position = startPosition;

        yield return new WaitForSeconds(movePauseTime);

        t = 0;
        while (t <= 1.0f)
        {
            t += step;
            u = t * t; // This does some fancy easing on the Lerp
            levelGameobj.transform.position = Vector3.Lerp(startPosition, offscreenPosition, u);
            yield return new WaitForFixedUpdate();
        }
        levelGameobj.transform.position = offscreenPosition;        
    }

 
    public void Pause() 
    {
        audioSource.Play();
        onPauseButtonPressed?.Invoke(GameManager.gameState.pause);
        
        gamePanel.SetActive(false);
        pausePanel.SetActive(true);

    }

    public void Resume() 
    {
        audioSource.Play();
        isOverPauseButton = false;

        onResume?.Invoke();
       
        //CheckCursorState();

        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void Save()
    {
        audioSource.Play();
        SaveGameManager.Save(GameManager._instance.CurrentLevel);
    }

    public void Load()
    {
        audioSource.Play();
        SaveGameManager.Load();
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        audioSource.Play();
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        audioSource.Play();
        Application.Quit();
    }

    private void OnDestroy()
    {
        //gameManager.onGameStateChange -= CheckCursorState;
        if (gameManager)
        {
            gameManager.onPlayerDestroyGranted -= GameOverPanel;
            gameManager.onLevelChange -= StartShowWaveNumber;
            gameManager.onBossDeath -= WinPanel;
            gameManager.onAmmoCountChange -= ShowNewAmmoCount;
            gameManager.onScoreChange -= ShowNewScore;
        }  
    }
}
