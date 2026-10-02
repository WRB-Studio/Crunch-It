using UnityEngine;

public class Crunchie : MonoBehaviour
{
    public enum eCrunchieTypes
    {
        None, Normal, Fast, Boss, Splitter, Zigzag
    }

    public eCrunchieTypes crunchieType = eCrunchieTypes.None;
    public Vector3 curMinMaxSpeed; //current speed; minimal speed; maximal speed; //auto calc current by a random value between min and max 
    public Vector2 hitpoints; //current hitpoints; start hitpoints;

    public float spawnChance;
    public float spawnAfterKills;

    public bool passedFinishLine = false;

    public GameObject bossClickAnim;
    public GameObject explosion;
    public AudioClip[] crunchSounds;

    public Color originColor;
    public Sprite originFace;

    private SpriteRenderer bodyRenderer;
    private SpriteRenderer faceRenderer;
    private SpriteRenderer legsRenderer;

    private GameHandler gameHandler;
    private CrunchieSpawner crunchieSpawner;
    private bool isDead;
    private double zigzagTime;
    private float zigzagCenterX;
    private float zigzagAmplitude;
    private float zigzagDirection;


    private void Awake()
    {
        gameHandler = GameHandler.instance;
        crunchieSpawner = CrunchieSpawner.instance;
    }

    public void Init(Sprite body, Sprite face, float scale = 0f)
    {
        bodyRenderer = GetComponent<SpriteRenderer>();
        faceRenderer = transform.Find("Face").GetComponent<SpriteRenderer>();
        legsRenderer = transform.Find("Legs").GetComponent<SpriteRenderer>();

        if (scale > 0f)
            transform.localScale = new Vector3(scale, scale, scale);
        else
            transform.localScale *= CrunchieSpawner.instance.crunchieSizeMultiplier;

        curMinMaxSpeed.x = Random.Range(curMinMaxSpeed.y, curMinMaxSpeed.z);

        bodyRenderer.sprite = body;
        faceRenderer.sprite = face;

        // Randomize color
        if (crunchieType == eCrunchieTypes.Normal)
        {
            Color randColor = Random.ColorHSV(0.1f, 0.1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
            bodyRenderer.color = randColor;
            legsRenderer.color = randColor;
        }

        if (crunchieType == eCrunchieTypes.Boss)
        {
            hitpoints.x = Mathf.RoundToInt(GameHandler.curDestroyed / spawnAfterKills);
            if (hitpoints.x < 2)
                hitpoints.x = 2;
            hitpoints.y = hitpoints.x;

            float tmpScale = Mathf.Clamp(1 + hitpoints.x / 30, transform.localScale.x, 2);
            transform.localScale *= tmpScale;
        }

        if (crunchieType == eCrunchieTypes.Zigzag)
        {
            GetComponent<Animator>().speed = 1.5f;
            float halfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
            float margin = bodyRenderer.bounds.extents.x + 0.1f;
            float minX = Camera.main.transform.position.x - halfWidth + margin;
            float maxX = Camera.main.transform.position.x + halfWidth - margin;
            zigzagAmplitude = Mathf.Min(0.8f, Mathf.Max(0f, (maxX - minX) * 0.5f));
            zigzagCenterX = Mathf.Clamp(transform.position.x, minX + zigzagAmplitude, maxX - zigzagAmplitude);
            zigzagDirection = Random.value < 0.5f ? -1f : 1f;
            zigzagTime = 0;
            transform.position = new Vector3(zigzagCenterX, transform.position.y, transform.position.z);
        }

        originColor = bodyRenderer.color;
        originFace = faceRenderer.sprite;

        setUltimateMode(UltimateMode.instance.CurrentFace);
    }

    private void OnMouseDown()
    {
        if (GameHandler.GetGameOver() || GameHandler.GetIsPause() || passedFinishLine)
            return;

        onCrunchieClick();
    }

    private void onCrunchieClick()
    {
        if (isDead || GameHandler.isPaused || GameHandler.isGameOver || passedFinishLine)
            return;
        hitpoints.x -= 1;

        if (crunchieType == eCrunchieTypes.Boss)//if a boss crunchie
        {
            //if boss killed
            if (hitpoints.x <= 0)
            {
                gameHandler.AddScore(2);
                gameHandler.AddDestroyed(1);

                death();
            }
            else //if boss have hitpoints
            {
                // Animation when boss is clicked and not destroyed
                StaticAudioHandler.playSound(crunchSounds[Random.Range(0, crunchSounds.Length)], "tmpCrunchSound", 0.8f, 0.2f);
                GameObject newBossClickAnimation = Instantiate(bossClickAnim, getClickPosition(), Quaternion.Euler(0, 0, Random.Range(0, 359)), transform.parent);
                newBossClickAnimation.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
            }
        }
        else//if not a boss
        {
            if (hitpoints.x <= 0)
            {
                gameHandler.AddScore(1);
                gameHandler.AddDestroyed(1);

                gameHandler.SetCombo();
                death();
            }
        }
    }

    public void death()
    {
        if (isDead || GameHandler.isPaused || GameHandler.isGameOver)
            return;
        isDead = true;
        if (crunchieType == eCrunchieTypes.Splitter)
            CrunchieSpawner.instance.SpawnAfterSplitterDeath(transform.position);

        StaticAudioHandler.playSound(crunchSounds[Random.Range(0, crunchSounds.Length)], "tmpCrunchSound", 1, 0.2f);
        GameObject newExplosion = Instantiate(explosion, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 359)), GameObject.Find("ExplosionsParent").transform);
        newExplosion.GetComponent<SpriteRenderer>().color = bodyRenderer.color;
        crunchieSpawner.removeCrunchie(this);
    }

    private Vector3 getClickPosition()
    {
        Vector3 position = transform.position;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.Android:
                if (Application.isMobilePlatform && Input.touchCount > 0)
                {
                    position = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 10.0f);
                }
                else
                {
                    position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
                }
                return Camera.main.ScreenToWorldPoint(position);
            default:
                return position;
        }
    }

    public void updateCall()
    {
        if (GameHandler.isPaused || GameHandler.isGameOver || isDead)
            return;

        // Update position
        Move(Time.deltaTime);

        // Check if Crunchie has passed the finish line
        if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
        {
            passedFinishLine = true;
            gameHandler.AddLife((int)-hitpoints.x);
        }

        // Remove Crunchie if it is out of the camera view
        if (crunchieSpawner.checkObjectIsOutOfCameraView(transform.position))
            crunchieSpawner.removeCrunchie(this);
    }

    private void Move(float deltaTime)
    {
        float movementTime = deltaTime * UltimateMode.instance.currentMultiplier;
        float x = transform.position.x;
        if (crunchieType == eCrunchieTypes.Zigzag)
        {
            zigzagTime += movementTime;
            x = zigzagCenterX + zigzagAmplitude * Mathf.Sin((float)(zigzagTime * Mathf.PI * 2f / 1.8f)) * zigzagDirection;
        }
        transform.position = new Vector3(x, transform.position.y - movementTime * curMinMaxSpeed.x, transform.position.z);
    }

    public void setUltimateMode(Sprite face)
    {
        SpriteRenderer mainRenderer = bodyRenderer;
        SpriteRenderer childRenderer = faceRenderer;

        if (face == null)
        {
            mainRenderer.color = originColor;
            childRenderer.sprite = originFace;
        }
        else
        {
            Color tmpColor = originColor;
            tmpColor.a = 1;
            tmpColor.r = Mathf.Clamp01(tmpColor.r + 0.02f);  // Ensure the color values remain valid
            tmpColor.g = Mathf.Clamp01(tmpColor.g - 0.03f);
            tmpColor.b = Mathf.Clamp01(tmpColor.b - 0.03f);
            mainRenderer.color = tmpColor;
            childRenderer.sprite = face;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!passedFinishLine && collision.transform.CompareTag("UltimateSmash"))
        {
            death();
        }
    }

}
