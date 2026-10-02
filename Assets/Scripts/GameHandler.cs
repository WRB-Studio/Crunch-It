using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private BoxCollider2D leftEdgeCollider;
    private BoxCollider2D rightEdgeCollider;
    private readonly List<GameObject> lifeAnimations = new List<GameObject>();
    private bool initialized;



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
        StopAllCoroutines();
        foreach (GameObject animation in lifeAnimations)
            if (animation != null) Destroy(animation);
        lifeAnimations.Clear();

        Transform explosions = GameObject.Find("ExplosionsParent").transform;
        foreach (Transform explosion in explosions)
        {
            explosion.gameObject.SetActive(false);
            Destroy(explosion.gameObject);
        }
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

        SetPaused(false);
        isGameOver = false;

        playingMusic = StaticAudioHandler.playMusic(instance.mainMusic, -0.5f);

        uIManager.Init();
        ultimateMode.Init();
        crunchieSpawner.Init();
        initialized = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
            uIManager.pauseMenueShowHide();
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
        if (isGameOver)
            return;

        curLifes = Mathf.Max(0, curLifes + newLifeValue);
        uIManager.UpdateLifes();
        if (newLifeValue > 0)
        {
            instance.StartCoroutine(instance.AddLifeAnimation());
        }
        else if (curLifes <= 0)
            SetGameOver();
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
            curBestCombo = Mathf.Max(curBestCombo, comboCounter);
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
        if (isGameOver)
            return;
        instance.uIManager.FinishCombo();
        isGameOver = true;
        SetPaused(true);

        instance.playingMusic.pitch = 0.9f;

        UIManager.instance.btContinue.interactable = false;
        Color tmpColor = UIManager.instance.btContinue.transform.GetChild(0).GetComponent<Image>().color;
        tmpColor.a = 0.2f;
        UIManager.instance.btContinue.transform.GetChild(0).GetComponent<Image>().color = tmpColor;
        UIManager.instance.btPause.GetComponent<Button>().interactable = false;

        instance.SaveBestScores();
        instance.uIManager.pauseMenueShowHide(true);
    }

    public static bool GetGameOver()
    {
        return isGameOver;
    }

    public void ReplayGame()
    {
        uIManager.FinishCombo();
        SaveBestScores();

        Init();
    }


    private void SaveBestScores(bool includePendingCombo = false)
    {
        Int32 score = curScore;
        if (includePendingCombo && comboCounter > 1)
            score += comboCounter * comboCounter;
        if (score > SaveLoadData.loadBestScore())
        {
            SaveLoadData.saveBestScore(score);
            if (uIManager != null) uIManager.newBestScoreArrow.SetActive(true);
        }
        if (curBestCombo > SaveLoadData.loadBestCombo())
        {
            SaveLoadData.saveBestCombo(curBestCombo);
            if (uIManager != null) uIManager.newBestComboArrow.SetActive(true);
        }
    }

    public static bool GetIsPause()
    {
        return isPaused;
    }

    public void ExitGame()
    {
        uIManager.FinishCombo();
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
        if (leftEdgeCollider == null)
        {
            leftEdgeCollider = new GameObject("LeftEdgeCollider").AddComponent<BoxCollider2D>();
            leftEdgeCollider.transform.SetParent(transform);
        }
        BoxCollider2D collider = leftEdgeCollider;
        collider.size = new Vector3(0.1f, Mathf.Abs(topRightScreenPoint.y - bottomLeftScreenPoint.y) * 2, 0f);
        collider.offset = new Vector2(collider.size.x / 2f, collider.size.y / 2f);
        leftEdgeCollider.transform.position = new Vector3(((bottomLeftScreenPoint.x - topRightScreenPoint.x) / 2f) - collider.size.x, bottomLeftScreenPoint.y, 0f);


        //right collider
        if (rightEdgeCollider == null)
        {
            rightEdgeCollider = new GameObject("RightEdgeCollider").AddComponent<BoxCollider2D>();
            rightEdgeCollider.transform.SetParent(transform);
        }
        collider = rightEdgeCollider;
        collider.size = new Vector3(0.1f, Mathf.Abs(topRightScreenPoint.y - bottomLeftScreenPoint.y) * 2, 0f);
        collider.offset = new Vector2(collider.size.x / 2f, collider.size.y / 2f);
        rightEdgeCollider.transform.position = new Vector3(topRightScreenPoint.x, bottomLeftScreenPoint.y, 0f);
    }


    private IEnumerator AddLifeAnimation()
    {
        Vector2 inputPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        RectTransform heart = GameObject.Find("ImgHearth").GetComponent<RectTransform>();
        Canvas canvas = heart.GetComponentInParent<Canvas>().rootCanvas;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, inputPosition, uiCamera, out Vector2 startPosition);

        RectTransform reward = Instantiate(heart, canvasRect, false);
        reward.name = "LifeReward";
        lifeAnimations.Add(reward.gameObject);
        reward.GetComponent<Image>().raycastTarget = false;
        reward.anchorMin = reward.anchorMax = new Vector2(0.5f, 0.5f);
        reward.pivot = new Vector2(0.5f, 0.5f);
        reward.sizeDelta = new Vector2(heart.rect.width * heart.lossyScale.x / canvasRect.lossyScale.x,
            heart.rect.height * heart.lossyScale.y / canvasRect.lossyScale.y);
        reward.SetAsLastSibling();

        Vector2 margin = reward.sizeDelta * 1.25f;
        startPosition.x = Mathf.Clamp(startPosition.x, canvasRect.rect.xMin + margin.x, canvasRect.rect.xMax - margin.x);
        startPosition.y = Mathf.Clamp(startPosition.y, canvasRect.rect.yMin + margin.y, canvasRect.rect.yMax - margin.y - 100f);
        Vector2 raisedPosition = startPosition + Vector2.up * 100f;
        reward.anchoredPosition = startPosition;
        reward.localScale = Vector3.one * 0.5f;
        StaticAudioHandler.playSound(addLifeSound, "tmpAddLife", 1, 0, -0.5f);

        // Pop above the finger before flying to the life counter.
        float elapsed = 0;
        while (elapsed < 0.45f)
        {
            float progress = Mathf.Clamp01(elapsed / 0.45f);
            float scale = elapsed < 0.16f
                ? Mathf.Lerp(0.5f, 2.5f, Mathf.SmoothStep(0, 1, elapsed / 0.16f))
                : Mathf.Lerp(2.5f, 2.2f, Mathf.SmoothStep(0, 1, (elapsed - 0.16f) / 0.29f));
            reward.localScale = Vector3.one * scale;
            reward.anchoredPosition = Vector2.Lerp(startPosition, raisedPosition, Mathf.SmoothStep(0, 1, progress));
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        elapsed = 0;
        while (elapsed < 0.65f)
        {
            float progress = Mathf.SmoothStep(0, 1, elapsed / 0.65f);
            Vector2 targetPosition = canvasRect.InverseTransformPoint(heart.TransformPoint(heart.rect.center));
            Vector2 controlPosition = (raisedPosition + targetPosition) * 0.5f + Vector2.up * 70f;
            reward.anchoredPosition = Vector2.Lerp(Vector2.Lerp(raisedPosition, controlPosition, progress),
                Vector2.Lerp(controlPosition, targetPosition, progress), progress);
            reward.localScale = Vector3.one * Mathf.Lerp(2.2f, 1f, progress);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        lifeAnimations.Remove(reward.gameObject);
        Destroy(reward.gameObject);
    }

    public static void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused || !initialized) return;
        SaveBestScores(true);
        if (!isPaused && !isGameOver) uIManager.pauseMenueShowHide(true);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) OnApplicationPause(true);
    }

    private void OnApplicationQuit()
    {
        if (!initialized) return;
        uIManager.FinishCombo(false);
        SaveBestScores();
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        Time.timeScale = 1;
        instance = null;
    }
}
