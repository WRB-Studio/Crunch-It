using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public bool removeSaveGame = false;

    [Header("Musics and sounds")]
    public AudioClip mainMusic;
    private AudioSource playingMusic;
    public AudioClip addLifeSound;

    [Header("Score")]
    public static Int32 curScore = 0;
    public static int curDestroyed = 0;

    [Header("Life")]
    public static int curLifes = 3;

    [Header("Combo")]
    public int addLifeAtComboCount = 5;
    public static int addLifeCounter = 0;
    public float comboDelay = 0.75f;
    public static float comboDelayCountDown = 0;
    public static int comboCounter = 0;
    public static int curBestCombo = 0;

    public static bool isPaused = false;
    public static bool isGameOver = false;

    public static GameHandler instance;
    private UIManager uIManager;
    private UltimateMode ultimateMode;
    private CrunchieSpawner crunchieSpawner;



    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (removeSaveGame)
            SaveLoadData.removeAll();

        Init();
    }

    public void Init()
    {
        uIManager = UIManager.instance;
        ultimateMode = UltimateMode.instance;
        crunchieSpawner = CrunchieSpawner.instance;

        RecalculateBackgroundScale();
        RecalculateSideCollidersPosition();

        curScore = 0;
        curDestroyed = 0;

        curLifes = 3;

        addLifeCounter = 0;
        comboDelayCountDown = 0;
        comboCounter = 0;
        curBestCombo = 0;

        isPaused = false;
        isGameOver = false;

        playingMusic = StaticAudioHandler.playMusic(instance.mainMusic, -0.5f);

        uIManager.Init();
        ultimateMode.Init();
        crunchieSpawner.Init();
    }

    private void Update()
    {
        uIManager.UpdateCall();
        ultimateMode.updateCall();
        crunchieSpawner.UpdateCall();
    }



    public void AddScore(Int32 addScore)
    {
        curScore += addScore;
        uIManager.UpdateScore();
    }

    public void AddDestroyed(int addDestroyed)
    {
        curDestroyed += addDestroyed;

        UltimateMode.instance.addUltimateKill(addDestroyed);
    }

    public void AddLife(int newLifeValue)
    {
        if (newLifeValue > 0)
        {
            instance.StartCoroutine(instance.AddLifeAnimation(newLifeValue));
        }
        else
        {
            curLifes += newLifeValue;
            if (curLifes < 0)
                curLifes = 0;

            uIManager.UpdateLifes();

            if (curLifes <= 0)
                SetGameOver();
        }
    }

    

    public void SetCombo()
    {
        if (comboDelayCountDown <= 0)//when combo counter ends
        {
            comboCounter = 0;
            addLifeCounter = 0;
            UIManager.instance.comboPanel.gameObject.SetActive(false);
        }

        comboDelayCountDown = instance.comboDelay;//reset combo countdown

        //activate and display combo
        comboCounter++;
        if (comboCounter > 1)
        {
            UIManager.instance.comboPanel.gameObject.SetActive(true);
            UIManager.instance.comboPanel.GetComponent<Animator>().Play("MultiAdd");
            UIManager.instance.txtCombo.text = comboCounter.ToString() + "X";
        }

        //add life after defined combo clicks
        addLifeCounter++;
        if (addLifeCounter == instance.addLifeAtComboCount)
        {
            AddLife(1);
            UIManager.instance.comboPanel.GetComponent<Animator>().Play("MultiAdd");
            addLifeCounter = 0;
        }
    }


    public static void SetGameOver()
    {
        isGameOver = true;
        instance.uIManager.pauseMenueShowHide();
        instance.uIManager.pauseMenuTitle.SetActive(false);
        instance.uIManager.gameOverMenuTitle.SetActive(true);

        instance.playingMusic.pitch = 0.9f;

        UIManager.instance.btContinue.interactable = false;
        Color tmpColor = UIManager.instance.btContinue.transform.GetChild(0).GetComponent<Image>().color;
        tmpColor.a = 0.2f;
        UIManager.instance.btContinue.transform.GetChild(0).GetComponent<Image>().color = tmpColor;
        UIManager.instance.btPause.GetComponent<Button>().interactable = false;

        instance.uIManager.showScore();
        instance.SaveBestScores();
    }

    public static bool GetGameOver()
    {
        return isGameOver;
    }

    public void ReplayGame()
    {
        SaveBestScores();

        Init();
    }



    private void SaveBestScores()
    {
        instance.StartCoroutine(instance.SetNewBestCoroutine());
    }

    public static bool GetIsPause()
    {
        return isPaused;
    }

    public void ExitGame()
    {
        SaveBestScores();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    private void RecalculateBackgroundScale()
    {
        GameObject background = GameObject.Find("Background");
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();

        float worldScreenHeight = Camera.main.orthographicSize * 2;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        background.transform.localScale = new Vector3(
            worldScreenHeight / sr.sprite.bounds.size.x,
            worldScreenWidth / sr.sprite.bounds.size.y, 1);
    }

    private void RecalculateSideCollidersPosition()
    {
        Vector3 bottomLeftScreenPoint = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRightScreenPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));

        //finishLine position
        Transform finishLine = GameObject.Find("FinishLine").transform;
        finishLine.transform.position = new Vector3(((bottomLeftScreenPoint.x - topRightScreenPoint.x) / 2f) + finishLine.transform.localScale.x / 4, bottomLeftScreenPoint.y + 1, 0f);

        //left collider
        GameObject leftColliderGO = new GameObject("LeftEdgeCollider");
        BoxCollider2D collider = leftColliderGO.AddComponent<BoxCollider2D>();
        collider.size = new Vector3(0.1f, Mathf.Abs(topRightScreenPoint.y - bottomLeftScreenPoint.y) * 2, 0f);
        collider.offset = new Vector2(collider.size.x / 2f, collider.size.y / 2f);
        leftColliderGO.transform.position = new Vector3(((bottomLeftScreenPoint.x - topRightScreenPoint.x) / 2f) - collider.size.x, bottomLeftScreenPoint.y, 0f);


        //right collider
        GameObject rightColliderGO = new GameObject("RightEdgeCollider");
        collider = rightColliderGO.AddComponent<BoxCollider2D>();
        collider.size = new Vector3(0.1f, Mathf.Abs(topRightScreenPoint.y - bottomLeftScreenPoint.y) * 2, 0f);
        collider.offset = new Vector2(collider.size.x / 2f, collider.size.y / 2f);
        rightColliderGO.transform.position = new Vector3(topRightScreenPoint.x, bottomLeftScreenPoint.y, 0f);
    }


    private IEnumerator AddLifeAnimation(int newLifeValue)
    {
        Vector2 inputPosition;
        if (Application.platform == RuntimePlatform.Android && Input.touchCount > 0)
            inputPosition = Input.GetTouch(0).position;
        else
            inputPosition = Input.mousePosition;

        GameObject lifeReference = GameObject.Find("ImgHearth");
        GameObject newLife = Instantiate(lifeReference, GameObject.Find("ImgHearth").transform);
        newLife.transform.position = inputPosition;
        newLife.transform.localScale = new Vector3(newLife.transform.localScale.x * 2, newLife.transform.localScale.y * 2, newLife.transform.localScale.z * 2);

        float animationSpeed = 0.4f;
        float curTime = 0;

        while (Vector2.Distance(newLife.transform.position, lifeReference.transform.position) > 0.1f)
        {
            yield return null;

            curTime += Time.deltaTime / animationSpeed;
            newLife.transform.position = Vector3.Lerp(inputPosition, lifeReference.transform.position, curTime);
            newLife.transform.localScale = Vector3.Lerp(newLife.transform.localScale, Vector3.one, curTime);
        }

        Destroy(newLife);
        StaticAudioHandler.playSound(addLifeSound, "tmpAddLife", 1, 0, -0.5f);

        curLifes += newLifeValue;
        uIManager.UpdateLifes();
    }


    private IEnumerator SetNewBestCoroutine()
    {
        Int32 loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestCombo = SaveLoadData.loadBestCombo();

        if (curScore > loadedBestScore)
        {
            uIManager.newBestScoreArrow.SetActive(true);
        }

        if (curBestCombo > loadedBestCombo)
        {
            uIManager.newBestComboArrow.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        if (curScore > loadedBestScore)
        {
            SaveLoadData.saveBestScore(curScore);
            instance.StartCoroutine(UIManager.instance.countAnimationCoroutine(UIManager.instance.txtBestScore, 0, curScore, UIManager.instance.counterAnimationSpeed));
        }

        if (curBestCombo > loadedBestCombo)
        {
            SaveLoadData.saveBestCombo(curBestCombo);
            instance.StartCoroutine(UIManager.instance.countAnimationCoroutine(UIManager.instance.txtBestCombo, 0, curBestCombo, UIManager.instance.counterAnimationSpeed, "x"));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(UIManager.instance.scorePanel);
    }
}
