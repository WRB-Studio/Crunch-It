using UnityEngine;

public class Crunchie : MonoBehaviour
{
    public enum eCrunchieTypes
    {
        None, Normal, Fast, Boss,
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



    private void Start()
    {
        transform.localScale *= CrunchieSpawner.instance.crunchieSizeMultiplier; 

        curMinMaxSpeed.x = Random.Range(curMinMaxSpeed.y, curMinMaxSpeed.z);

        if (crunchieType == eCrunchieTypes.Boss)
        {
            hitpoints.x = Mathf.RoundToInt(GameHandler.curDestroyed / spawnAfterKills);
            if (hitpoints.x < 2)
                hitpoints.x = 2;

            float tmpScale = Mathf.Clamp(1 + hitpoints.x / 30, transform.localScale.x, 2);
            transform.localScale *= tmpScale;
        }

        originColor = GetComponent<SpriteRenderer>().color;
        originFace = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    }

    private void OnMouseDown()
    {
        if (GameHandler.getGameOver() || GameHandler.getIsPause() || passedFinishLine)
            return;

        onCrunchieClick();
    }

    private void onCrunchieClick()
    {
        hitpoints.x -= 1;

        if (crunchieType == eCrunchieTypes.Boss)//if a boss crunchie
        {
            //if boss killed
            if (hitpoints.x <= 0)
            {
                GameHandler.addScore(Mathf.RoundToInt(hitpoints.y * 2));
                GameHandler.addDestroyed(1);

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
                GameHandler.addScore(1);
                GameHandler.addDestroyed(1);

                death();
                GameHandler.setCombo();
            }
        }
    }

    public void death()
    {
        StaticAudioHandler.playSound(crunchSounds[Random.Range(0, crunchSounds.Length)], "tmpCrunchSound", 1, 0.2f);
        GameObject newExplosion = Instantiate(explosion, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 359)), transform.parent);
        explosion.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
        CrunchieSpawner.removeCrunchie(this);
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
        if (!GameHandler.isGameOver)
        {
            float speedModifier = GameHandler.isPaused ? 0 : (GameHandler.isGameOver ? 1.5f : 1.0f);
            // Update position
            transform.position = new Vector2(transform.position.x, transform.position.y - Time.deltaTime * curMinMaxSpeed.x * speedModifier * UltimateMode.instance.currentMultiplier);

            // Check if Crunchie has passed the finish line
            if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
            {
                passedFinishLine = true;
                if (!GameHandler.isPaused) // Only adjust life if the game is not paused
                    GameHandler.addLife((int)-hitpoints.x);
            }
        }

        // Remove Crunchie if it is out of the camera view
        if (CrunchieSpawner.checkObjectIsOutOfCameraView(transform.position))
            CrunchieSpawner.removeCrunchie(this);
    }

    public void setUltimateMode(Sprite face)
    {
        SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer childRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (face == null)
        {
            mainRenderer.color = originColor;
            childRenderer.sprite = originFace;
        }
        else
        {
            Color tmpColor = mainRenderer.color;
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
        if (collision.transform.tag == "UltimateSmash")
        {
            death();
        }
    }

}