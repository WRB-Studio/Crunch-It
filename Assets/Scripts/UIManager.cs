using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI elements")]
    public GameObject UIContent;
    public GameObject pauseMenu;
    public GameObject comboPanel;
    public GameObject comboScorePopupPrefab;

    public GameObject pauseMenuTitle;
    public GameObject gameOverMenuTitle;

    public Text txtHealth;
    public Text txtScore;
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

    [Header("Animation & Audio Settings")]
    public float counterAnimationSpeed = 0.2f;
    public AudioClip countSound;

    private GameHandler gameHandler;


    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        gameHandler = GameHandler.instance;

        instance.txtHealth.text = GameHandler.curLifes.ToString();
        instance.txtScore.text = GameHandler.curScore.ToString();
        instance.txtCombo.text = GameHandler.comboCounter.ToString();

        instance.newBestScoreArrow.SetActive(false);
        instance.newBestComboArrow.SetActive(false);

        instance.comboPanel.gameObject.SetActive(false);

        btPause.onClick.RemoveAllListeners();
        btContinue.onClick.RemoveAllListeners();
        btReplay.onClick.RemoveAllListeners();
        btExit.onClick.RemoveAllListeners();

        btPause.onClick.AddListener(delegate { pauseMenueShowHide(); });
        btContinue.onClick.AddListener(delegate { pauseMenueShowHide(false); });
        btReplay.onClick.AddListener(delegate { gameHandler.ReplayGame(); });
        btExit.onClick.AddListener(delegate { gameHandler.ExitGame(); });

        btPause.GetComponent<Button>().interactable = true;

        showScore(false);

        pauseMenueShowHide();
    }

    public void UpdateCall()
    {
        ComboHandlerUpdateCall();
    }

    public void pauseMenueShowHide()
    {
        instance.pauseMenuTitle.SetActive(true);
        instance.gameOverMenuTitle.SetActive(false);

        if (instance.pauseMenu.activeSelf)
        {
            instance.pauseMenu.SetActive(false);
            GameHandler.isPaused = false;
        }
        else
        {
            instance.pauseMenu.SetActive(true);
            GameHandler.isPaused = true;

            showScore();
        }
    }

    public void pauseMenueShowHide(bool show)
    {
        if (!show)
        {
            instance.pauseMenu.SetActive(false);
            GameHandler.isPaused = false;
        }
        else
        {
            instance.pauseMenu.SetActive(true);
            GameHandler.isPaused = true;

            showScore();
        }
    }

    public void showScore(bool playSound = true)
    {
        Int32 loadedBestScore = SaveLoadData.loadBestScore();
        int loadedBestCombo = SaveLoadData.loadBestCombo();

        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtYOUScore, 0, GameHandler.curScore, instance.counterAnimationSpeed, "", playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestScore, 0, loadedBestScore, instance.counterAnimationSpeed, "", playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtYOUCombo, 0, GameHandler.curBestCombo, instance.counterAnimationSpeed, "x", playSound));
        instance.StartCoroutine(instance.countAnimationCoroutine(instance.txtBestCombo, 0, loadedBestCombo, instance.counterAnimationSpeed, "x", playSound));

        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.scorePanel);
    }

    public IEnumerator countAnimationCoroutine(Text textElement, Int32 startValue, Int32 endValue, float countingSpeed, string extension = "", bool playCountSound = true)
    {
        // Sicherheitswartezeit einmal anlegen
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.0001f, countingSpeed));

        // Schrittweite: max(1, ~10 Schritte gesamt)
        int range = Mathf.Max(0, endValue - startValue);
        int step = Mathf.Max(1, Mathf.CeilToInt(range / 10f));

        // Sound-Intervall (alle 2 Updates) nur wenn aktiv
        int soundEvery = 2;
        int soundCountdown = soundEvery;

        // kleine Startverzögerung wie im Original
        yield return new WaitForSeconds(0.2f);

        for (int v = startValue; v < endValue; v += step)
        {
            // Abbruch bei Benutzereingabe
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) break;

            textElement.text = v.ToString() + extension;

            if (playCountSound)
            {
                if (--soundCountdown <= 0)
                {
                    StaticAudioHandler.playSound(instance.countSound, "tmpCountSound", 1.3f, 0.05f, -0.5f);
                    soundCountdown = soundEvery;
                }
            }

            yield return wait;
        }

        // Finale Zielanzeige
        textElement.text = endValue.ToString() + extension;
    }

    public void UpdateScore()
    {
        instance.txtScore.text = GameHandler.curScore.ToString();
    }

    public void UpdateLifes()
    {
        instance.txtHealth.text = GameHandler.curLifes.ToString();
    }

    private void ComboHandlerUpdateCall()
    {
        if (GameHandler.GetIsPause())
            return;

        if (GameHandler.comboDelayCountDown > 0)//when combo active
        {
            GameHandler.comboDelayCountDown -= Time.deltaTime;

            if (GameHandler.comboDelayCountDown <= 0)//when combo ends
            {
                if (GameHandler.comboCounter > 1)//calculate score for ended combo
                {
                    if (GameHandler.comboCounter > GameHandler.curBestCombo)//check combo is current highest combo
                        GameHandler.curBestCombo = GameHandler.comboCounter;

                    Int32 tmpComboScore = (Int32)GameHandler.comboCounter * (Int32)GameHandler.comboCounter;//calculate combo score

                    GameObject newComboScorePopup = Instantiate(instance.comboScorePopupPrefab, instance.UIContent.transform);
                    newComboScorePopup.GetComponent<Text>().text = tmpComboScore.ToString();
                    Destroy(newComboScorePopup, 3f);

                    gameHandler.AddScore(tmpComboScore);
                }
                GameHandler.comboCounter = 1;
                instance.comboPanel.gameObject.SetActive(false);
            }
        }
    }


}
