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
                GameHandler.addScore((System.Int32)(hitpoints.y * 2));
                GameHandler.addDestroyed(1);

                death();
                GameHandler.setCombo();
            }
            else
            {
                //animation when boss clicked and not destroyed
                Vector3 clickPosition = transform.position;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                        clickPosition = Input.mousePosition;
                        clickPosition.z = 10.0f;
                        clickPosition = Camera.main.ScreenToWorldPoint(clickPosition);
                        break;
                    case RuntimePlatform.WindowsEditor:
                        clickPosition = Input.mousePosition;
                        clickPosition.z = 10.0f;
                        clickPosition = Camera.main.ScreenToWorldPoint(clickPosition);
                        break;
                    case RuntimePlatform.Android:
                        if (Input.touchCount > 0)
                        {
                            clickPosition = Input.GetTouch(0).position;
                            clickPosition.z = 10.0f;
                            clickPosition = Camera.main.ScreenToWorldPoint(clickPosition);
                        }
                        break;
                    default:
                        break;
                }

                StaticAudioHandler.playSound(crunchSounds[Random.Range(0, crunchSounds.Length)], "tmpCrunchSound", 0.8f, 0.2f);
                GameObject newBosClickAnimation = Instantiate(bossClickAnim, clickPosition, Quaternion.Euler(0, 0, Random.Range(0, 359)), transform.parent);
                bossClickAnim.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
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

    public void updateCall()
    {
        if (!GameHandler.isGameOver && !GameHandler.isPaused)//
        {
            //moving
            transform.position = new Vector2(transform.position.x, transform.position.y - Time.deltaTime * curMinMaxSpeed.x * UltimateMode.instance.currentMultiplier);

            //check crunchie reached finish line
            if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
            {
                passedFinishLine = true;
                GameHandler.addLife((int)-hitpoints.x);
            }
        }
        else if (GameHandler.isGameOver)//on gameover crunchie run faster
        {
            //moving
            transform.position = new Vector2(transform.position.x, transform.position.y - Time.deltaTime * curMinMaxSpeed.x * 1.5f * UltimateMode.instance.currentMultiplier);

            //set has reached the finish line
            if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
            {
                passedFinishLine = true;
            }
        }

        //Remove when out of bottom cam view
        if (CrunchieSpawner.checkObjectIsOutOfCameraView(transform.position))
            CrunchieSpawner.removeCrunchie(this);
    }

    public void setUltimateMode(Sprite face)
    {
        if (face == null)
        {
            GetComponent<SpriteRenderer>().color = originColor;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = originFace;
        }
        else
        {
            Color tmpColor = GetComponent<SpriteRenderer>().color;
            tmpColor.a = 1;
            tmpColor.r += 0.02f;
            tmpColor.g -= 0.03f;
            tmpColor.b -= 0.03f;
            GetComponent<SpriteRenderer>().color = tmpColor;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = face;
        }
    }


    public float getSpawnChance()
    {
        return spawnChance;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "UltimateSmash")
        {
            death();
        }
    }

}