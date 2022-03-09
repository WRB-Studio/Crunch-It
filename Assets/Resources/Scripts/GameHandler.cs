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
    public AudioClip countSound;

    [Header("Score")]
    public float counterAnimationSpeed = 0.2f;
    public static Int32 curScore = 0;
    public static int curDestroyed = 0;

    [Header("Life")]
    public static int curLifes = 3;

    [Header("Combo")]
    public int addLifeAtComboCount = 5;
    public static int addLifeCounter = 0;
    public float comboDelay = 0.75f;
    private static float comboDelayCountDown = 0;
    public static int comboCounter = 0;
    private static int curBestCombo = 0;

    public static bool isPaused = false;
    public static bool isGameOver = false;

    [Header("UI elements")]
    public GameObject UIContent;
    public GameObject pauseMenue;
    public GameObject comboPanel;
    public GameObject comboScorePopupPrefab;

    public Text txtHealth;
    public Text txtScore;
    public Text txtPauseMenuTitle;
    public Text txtCombo;

    public Button btPause;
    public Button btContinue;
    public Button btReplay;
    public Button btExit;

    public Text txtYOUScore;
    public Text txtYOUCombo;
    public Text txtBestScore;
    public Text txtBestCombo;
    public GameObject newBestScoreArrow;
    public GameObject newBestComboArrow;
    public RectTransform scorePanel;

    private static EventSystem myEventSystem;

    public static GameHandler instance;



    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (removeSaveGame)
            SaveLoadData.removeAll();

        init();
    }

    private void Update()
    {
        myEventSystem.SetSelectedGameObject(null);//deselect ui-elements after clicking on it.

        comboHandlerUpdateCall();
        UltimateMode.updateCall();
        CrunchieSpawner.updateCall();
    }

    public static void init()
    {
        recalculateBackgroundScale();
        recalculateSideCollidersPosition();

        curScore = 0;
        curDestroyed = 0;

        curLifes = 3;

        addLifeCounter = 0;
        comboDelayCountDown = 0;
        comboCounter = 0;
        curBestCombo = 0;

        isPaused = false;
        isGameOver = false;

        instance.txtHealth.text = curLifes.ToString();
        instance.txtScore.text = curScore.ToString();
        instance.txtPauseMenuTitle.text = "Pause";
        instance.txtCombo.text = comboCounter.ToString();

        instance.newBestScoreArrow.GetComponent<Image>().enabled = false;
        instance.newBestComboArrow.GetComponent<Image>().enabled = false;

        myEventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        instance.playingMusic = StaticAudioHandler.playMusic(instance.mainMusic, -0.5f);

        instance.comboPanel.gameObject.SetActive(false);

        //if (GameObject.Find("AdMob") != null)
        //    adMobScrp = GameObject.Find("AdMob").GetComponent<AdMob>();

        UltimateMode.init();
        CrunchieSpawner.init();

        showScore(false);

        pauseMenueShowHide();
    }


    public static void addScore(Int32 addScore)
    {
        curScore += addScore;
        instance.txtScore.text = curScore.ToString();
    }

    public static void addDestroyed(int addDestroyed)
    {
        curDestroyed += addDestroyed;

        UltimateMode.instance.addUltimateKill(addDestroyed);
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
            instance.txtHealth.text = curLifes.ToString();
            if (curLifes <= 0)
                setGameOver();
        }
    }

    private static void comboHandlerUpdateCall()
    {
        if (getIsPause())
            return;

        if (comboDelayCountDown > 0)//when combo active
        {
            comboDelayCountDown -= Time.deltaTime;

            if (comboDelayCountDown <= 0)//when combo ends
            {
                if (comboCounter > 1)//calculate score for ended combo
                {
                    if (comboCounter > curBestCombo)//check combo is current highest combo
                        curBestCombo = comboCounter;

                    Int32 tmpComboScore = (Int32)comboCounter * (Int32)comboCounter;//calculate combo score

                    GameObject newComboScorePopup = Instantiate(instance.comboScorePopupPrefab, instance.UIContent.transform);
                    newComboScorePopup.GetComponent<Text>().text = tmpComboScore.ToString();
                    Destroy(newComboScorePopup, 3f);

                    addScore(tmpComboScore);
                }
                comboCounter = 1;
                instance.comboPanel.gameObject.SetActive(false);
            }
        }
    }

    public static void setCombo()
    {
        if (comboDelayCountDown <= 0)//when combo counter ends
        {
            comboCounter = 0;
            addLifeCounter = 0;
            instance.comboPanel.gameObject.SetActive(false);
        }

        comboDelayCountDown = instance.comboDelay;//reset combo countdown

        //activate and display combo
        comboCounter++;
        if (comboCounter > 1)
        {
            instance.comboPanel.gameObject.SetActive(true);
            instance.comboPanel.GetComponent<Animator>().Play("MultiAdd");
            instance.txtCombo.text = comboCounter.ToString() + "X";
        }

        //add life after defined combo clicks
        addLifeCounter++;
        if (addLifeCounter == instance.addLifeAtComboCount)
        {
            addLife(1);
            instance.comboPanel.GetComponent<Animator>().Play("MultiAdd");
            addLifeCounter = 0;
        }
    }


    public static void setGameOver()
    {
        isGameOver = true;
        pauseMenueShowHide();
        instance.txtPauseMenuTitle.text = "GAME OVER";

        instance.playingMusic.pitch = -1.2f;

        instance.btContinue.interactable = false;
        Color tmpColor = instance.btContinue.transform.GetChild(0).GetComponent<Image>().color;
        tmpColor.a = 0.2f;
        instance.btContinue.transform.GetChild(0).GetComponent<Image>().color = tmpColor;
        instance.btPause.GetComponent<Button>().interactable = false;

        showScore();
        saveBestScores();
    }

    public static bool getGameOver()
    {
        return isGameOver;
    }

    public static void replayGame()
    {
        saveBestScores();

        init();
    }

    public static void pauseMenueShowHide()
    {
        if (instance.pauseMenue.activeSelf)
        {
            instance.pauseMenue.SetActive(false);
            isPaused = false;
        }
        else
        {
            instance.pauseMenue.SetActive(true);
            isPaused = true;

            showScore();
        }
    }

    public static void pauseMenueShowHide(bool show)
    {
        if (!show)
        {
            instance.pauseMenue.SetActive(false);
            isPaused = false;
        }
        else
        {
            instance.pauseMenue.SetActive(true);
            isPaused = true;

            showScore();
        }
    }

    private static void showScore(bool playSound = true)
    {
        Int32 loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestCombo = SaveLoadData.loadBestCombo();

        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtYOUScore, 0, curScore, instance.counterAnimationSpeed, "",  playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestScore, 0, loadedBestScore, instance.counterAnimationSpeed, "", playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtYOUCombo, 0, curBestCombo, instance.counterAnimationSpeed, "x", playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestCombo, 0, loadedBestCombo, instance.counterAnimationSpeed, "x", playSound));

        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.scorePanel);
    }

    private static void saveBestScores()
    {
        instance.StartCoroutine(instance.setNewBestCoroutine());
    }

    public static bool getIsPause()
    {
        return isPaused;
    }

    public static void exitGame()
    {
        saveBestScores();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
        txtHealth.text = curLifes.ToString();
    }

    private IEnumerator countAnimationCoroutine(Text textElement, Int32 startValue, Int32 endValue, float countingSpeed, string extension = "", bool playCountSound = true)
    {
        yield return new WaitForSeconds(0.2f);

        int counterIntervall = (Int32)endValue / 10;
        if (counterIntervall <= 10)
            counterIntervall = 1;

        int playSoundByIntervall = 2;
        int playSoundCountdown = 0;

        for (Int32 counter = startValue; counter < endValue; counter += counterIntervall)
        {
            if (Application.platform == RuntimePlatform.Android && Input.touchCount > 0)
                break;
            else if (Input.GetMouseButton(0))
                break;

            textElement.text = counter + extension;

            if (playCountSound)
            {
                playSoundCountdown--;
                if (playSoundCountdown <= 0)
                {
                    StaticAudioHandler.playSound(countSound, "tmpCountSound", 1.3f, 0.05f, -0.5f);
                    playSoundCountdown = playSoundByIntervall;
                }
            }
            
            yield return new WaitForSeconds(countingSpeed);
        }

        textElement.text = endValue + extension;
    }

    private IEnumerator setNewBestCoroutine()
    {
        Int32 loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestCombo = SaveLoadData.loadBestCombo();

        if (curScore > loadedBestScore)
        {
            newBestScoreArrow.GetComponent<Image>().enabled = true;
        }

        if (curBestCombo > loadedBestCombo)
        {
            newBestComboArrow.GetComponent<Image>().enabled = true;
        }

        yield return new WaitForSeconds(1f);

        if (curScore > loadedBestScore)
        {
            SaveLoadData.saveBestScore(curScore);
            instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestScore, 0, curScore, instance.counterAnimationSpeed));
        }

        if (curBestCombo > loadedBestCombo)
        {
            SaveLoadData.saveBestCombo(curBestCombo);
            instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestCombo, 0, curBestCombo, instance.counterAnimationSpeed, "x"));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.scorePanel);
    }
}
