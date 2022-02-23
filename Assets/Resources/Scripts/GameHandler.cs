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

    [Header("Score, lifes and multiplier")]
    public static ulong curScore = 0;
    public static int curDestroyed = 0;
    public static int curLifes = 0;
    public static int addLifeAtMultiCount = 10;
    public static int addLifeCounter = 0;
    public static float multiDelay = 0.5f;
    private static float multiDelayCountDown = 0;
    public static int multiCounter = 0;
    private static int curBestMultiplier = 0;
    private static ulong curBestMultiplierScore = 0;

    public static bool isPaused = false;
    public static bool isGameOver = false;
    public static bool adContinueUsed = false;

    private static GameObject pauseMenue;
    private static GameObject multiPanel;
    private static GameObject adContinue;

    private static Text txtHealth;
    private static Text txtScore;
    private static Text txtStopGame;
    private static Text txtMulti;

    private static Button btPause;
    private static Button btContinue;
    private static Button btReplay;
    private static Button btExit;

    private static Text txtBestScore;
    private static Text txtBestMultiplier;
    private static Text txtBestMultiplierScore;

    private static EventSystem myEventSystem;
    private static AdMob adMobScrp;

    public static GameHandler instance;



    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        recalculateBackgroundScale();
        recalculateSideCollidersPosition();

        if (removeSaveGame)
            SaveLoadData.removeAll();

        myEventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        playingMusic = StaticAudioHandler.playMusic(mainMusic, -0.5f);

        multiPanel = GameObject.Find("MultiPanel");
        adContinue = GameObject.Find("AdContinue");
        adContinue.transform.Find("BtContinueAd").GetComponent<Button>().onClick.AddListener(() => showRewardAdForContinuePlay());
        adContinue.SetActive(false);

        txtHealth = GameObject.Find("txtHealth").GetComponent<Text>();
        txtHealth.text = curLifes.ToString();
        txtScore = GameObject.Find("txtScore").GetComponent<Text>();
        txtStopGame = GameObject.Find("txtStopGame").GetComponent<Text>();
        txtMulti = GameObject.Find("txtMulti").GetComponent<Text>();

        btPause = GameObject.Find("BtPause").GetComponent<Button>();
        btContinue = GameObject.Find("BtContinue").GetComponent<Button>();
        btReplay = GameObject.Find("BtReplay").GetComponent<Button>();
        btExit = GameObject.Find("BtExit").GetComponent<Button>();

        txtBestScore = GameObject.Find("txtBestScore").GetComponent<Text>();
        txtBestMultiplier = GameObject.Find("txtBestMultiplier").GetComponent<Text>();
        txtBestMultiplierScore = GameObject.Find("txtBestMultiplierScore").GetComponent<Text>();

        multiPanel.gameObject.SetActive(false);

        adMobScrp = GameObject.Find("AdMob").GetComponent<AdMob>();

        pauseMenue = GameObject.Find("PauseMenue");
        pauseMenueShowHide();
    }

    private void Update()
    {
        myEventSystem.SetSelectedGameObject(null);//deselect ui-elements after clicking on it.

        multiHandlerUpdate();
    }


    public static void addScore(ulong addScore)
    {
        curScore += addScore;
        txtScore.text = curScore.ToString();
    }

    public static void addDestroyed(int addDestroyed)
    {
        curDestroyed += addDestroyed;
    }

    public static void addLife(int newLifeValue)
    {
        if (newLifeValue > 0)
        {
            instance.StartCoroutine(instance.addLifeAnimation(newLifeValue));
        }
        else
        {
            curLifes += newLifeValue;
            if (curLifes < 0)
                curLifes = 0;
            txtHealth.text = curLifes.ToString();
            if (curLifes <= 0)
                setGameOver();
        }
    }

    private static void multiHandlerUpdate()
    {
        if (multiDelayCountDown > 0)
        {
            multiDelayCountDown -= Time.deltaTime;

            if (multiDelayCountDown <= 0)
            {
                if (multiCounter > 1)
                {
                    if (multiCounter > curBestMultiplier)
                        curBestMultiplier = multiCounter;

                    ulong tmpMultiScore = (ulong)multiCounter * (ulong)multiCounter;
                    if (tmpMultiScore > curBestMultiplierScore)
                        curBestMultiplierScore = tmpMultiScore;

                    addScore(tmpMultiScore);
                }
                multiCounter = 1;
                multiPanel.gameObject.SetActive(false);
            }
        }
    }

    public static void setMulti()
    {
        if (multiDelayCountDown <= 0)
        {
            multiCounter = 0;
            addLifeCounter = 0;
            multiPanel.gameObject.SetActive(false);
        }

        multiDelayCountDown = multiDelay;

        //display multiplier
        multiCounter++;
        if (multiCounter > 1)
        {
            multiPanel.gameObject.SetActive(true);
            txtMulti.text = multiCounter.ToString() + "X";
        }

        //add life after n succesfull multi clicks
        addLifeCounter++;
        if (addLifeCounter == addLifeAtMultiCount)
        {
            addLife(1);
            addLifeCounter = 0;
        }
    }


    public static void setGameOver()
    {
        isGameOver = true;
        pauseMenueShowHide();
        txtStopGame.text = "GAME OVER";

        instance.playingMusic.pitch = -1.2f;

        btContinue.interactable = false;
        Color tmpColor = btContinue.transform.GetChild(0).GetComponent<Image>().color;
        tmpColor.a = 0.2f;
        btContinue.transform.GetChild(0).GetComponent<Image>().color = tmpColor;
        btPause.GetComponent<Button>().interactable = false;

        loadSaveShowBestScores();

        if (!adContinueUsed && Application.isEditor)
            adContinue.SetActive(true);
        else if (!adContinueUsed && adMobScrp.getRewardAdIsLoaded())
            adContinue.SetActive(true);
    }

    public static bool getGameOver()
    {
        return isGameOver;
    }

    public static void replayGame()
    {
        loadSaveShowBestScores();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public static void pauseMenueShowHide()
    {
        if (pauseMenue.activeSelf)
        {
            pauseMenue.SetActive(false);
            isPaused = false;
        }
        else
        {
            pauseMenue.SetActive(true);
            isPaused = true;

            loadShowBestScores();
        }
    }

    public static void pauseMenueShowHide(bool show)
    {
        if (!show)
        {
            pauseMenue.SetActive(false);
            isPaused = false;
        }
        else
        {
            pauseMenue.SetActive(true);
            isPaused = true;

            loadShowBestScores();
        }
    }

    private static void loadShowBestScores()
    {
        ulong loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestMultiplier = SaveLoadData.loadBestMultiplier();
        ulong loadedBestMultiplierScore = SaveLoadData.loadBestMultiplierScore();
        txtBestScore.text = "Score: " + loadedBestScore;
        txtBestMultiplier.text = "Multiplier: " + loadedBestMultiplier + "X";
        txtBestMultiplierScore.text = "Multiplier score: " + loadedBestMultiplierScore;
    }

    private static void loadSaveShowBestScores()
    {
        ulong loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestMultiplier = SaveLoadData.loadBestMultiplier();
        ulong loadedBestMultiplierScore = SaveLoadData.loadBestMultiplierScore();

        if (loadedBestScore < curScore)
        {
            txtBestScore.text = "Score: " + curScore;
            SaveLoadData.saveBestScore(curScore);
        }
        else
        {
            txtBestScore.text = "Score: " + loadedBestScore;
        }

        if (loadedBestMultiplier < curBestMultiplier)
        {
            txtBestMultiplier.text = "Multiplier: " + curBestMultiplier + "X";
            SaveLoadData.saveBestMultiplier(curBestMultiplier);
        }
        else
        {
            txtBestMultiplier.text = "Multiplier: " + loadedBestMultiplier;
        }

        if (loadedBestMultiplierScore < curBestMultiplierScore)
        {
            txtBestMultiplierScore.text = "Multiplier score: " + curBestMultiplierScore;
            SaveLoadData.saveBestMultiplierScore(curBestMultiplierScore);
        }
        else
        {
            txtBestMultiplierScore.text = "Multiplier score: " + loadedBestMultiplierScore;
        }
    }

    public static bool getIsPause()
    {
        return isPaused;
    }

    public static void exitGame()
    {
        loadSaveShowBestScores();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private static void showRewardAdForContinuePlay()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            GameObject.Find("AdMob").GetComponent<AdMob>().userChoseToWatchAd();
        }
        else if (Application.isEditor)
        {
            continueAfterRewardAd(true);
        }
    }

    public static void continueAfterRewardAd(bool rewardEarned)
    {
        if (rewardEarned)
        {
            if (adContinueUsed)
                return;

            adContinueUsed = true;
            adContinue.SetActive(false);

            instance.StartCoroutine(instance.lifeRewardContinue());
        }
        else
        {
            pauseMenueShowHide(true);
        }
    }


    private static void recalculateBackgroundScale()
    {
        GameObject background = GameObject.Find("Background");
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();

        float worldScreenHeight = Camera.main.orthographicSize * 2;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        background.transform.localScale = new Vector3(
            worldScreenHeight / sr.sprite.bounds.size.x,
            worldScreenWidth / sr.sprite.bounds.size.y, 1);
    }

    private static void recalculateSideCollidersPosition()
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


    private IEnumerator addLifeAnimation(int newLifeValue)
    {
        Vector2 inputPosition;
        if (Application.platform == RuntimePlatform.Android)
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
        txtHealth.text = curLifes.ToString();
    }

    private IEnumerator lifeRewardContinue()
    {
        isPaused = true;
        isGameOver = true;

        playingMusic.pitch = 1;

        //add lifes (animated)
        for (int i = 0; i < 20; i++)
        {
            yield return null;

            Vector2 startPosition = adContinue.transform.position;

            GameObject lifeReference = GameObject.Find("ImgHearth");
            GameObject newLife = Instantiate(lifeReference, GameObject.Find("ImgHearth").transform);
            newLife.transform.position = startPosition;
            newLife.transform.localScale = new Vector3(newLife.transform.localScale.x * 2, newLife.transform.localScale.y * 2, newLife.transform.localScale.z * 2);

            float animationSpeed = 0.08f;
            float curTime = 0;

            while (Vector2.Distance(newLife.transform.position, lifeReference.transform.position) > 0.1f)
            {
                yield return null;

                curTime += Time.deltaTime / animationSpeed;
                newLife.transform.position = Vector3.Lerp(startPosition, lifeReference.transform.position, curTime);
                newLife.transform.localScale = Vector3.Lerp(newLife.transform.localScale, Vector3.one, curTime);
            }

            Destroy(newLife);
            StaticAudioHandler.playSound(addLifeSound, "tmpAddLife", 1, 0, -0.5f);

            curLifes += 1;
            txtHealth.text = curLifes.ToString();
        }

        List<Crunchie> crunchyList = GameObject.Find("CrunchieSpawner").GetComponent<CrunchieSpawner>().getCrunchyList();

        //Remove as many as there are 10 left.
        while (crunchyList.Count > 10)
        {
            yield return new WaitForSeconds(0.03f);

            crunchyList[Random.Range(0, crunchyList.Count - 1)].death();
        }

        //remove all crunchies below position.y zero
        foreach (Crunchie crunchy in crunchyList.ToArray())
        {
            yield return new WaitForSeconds(0.03f);

            if (crunchy.transform.position.y < 0)
                crunchy.death();
        }

        adContinueUsed = true;

        isGameOver = false;
        isPaused = false;
        txtStopGame.text = "Pause";

        playingMusic.pitch = 1f;

        btContinue.interactable = true;
        Color tmpColor = btContinue.transform.GetChild(0).GetComponent<Image>().color;
        tmpColor.a = 1f;
        btContinue.transform.GetChild(0).GetComponent<Image>().color = tmpColor;
        btPause.GetComponent<Button>().interactable = true;
        adContinue.SetActive(false);
    }

}
