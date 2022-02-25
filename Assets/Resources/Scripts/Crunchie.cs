using System.Collections;
using System.Collections.Generic;
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



    
    private void Start()
    {
        curMinMaxSpeed.x = Random.Range(curMinMaxSpeed.y, curMinMaxSpeed.z);

        if (crunchieType == eCrunchieTypes.Boss)
        {
            hitpoints.x = Mathf.RoundToInt(GameHandler.curDestroyed / spawnAfterKills);
            if (hitpoints.x < 2)
                hitpoints.x = 2;
        }


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
                GameHandler.addScore((ulong)(hitpoints.y * 2));
                GameHandler.addDestroyed(1);

                death();
                GameHandler.setMulti();
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
                        clickPosition = Input.GetTouch(0).position;
                        clickPosition.z = 10.0f;
                        clickPosition = Camera.main.ScreenToWorldPoint(clickPosition);
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
                GameHandler.setMulti();
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
        if (!GameHandler.isGameOver && !GameHandler.isPaused)
        {
            //moving
            transform.position = new Vector2(transform.position.x, transform.position.y - Time.deltaTime * curMinMaxSpeed.x);

            //set has reached the finish line
            if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
            {
                passedFinishLine = true;
                GameHandler.addLife(-1);
            }

            //Remove when out of bottom cam view
            if (CrunchieSpawner.checkObjectIsOutOfCameraView(transform.position))
                CrunchieSpawner.removeCrunchie(this);
        }
        else if(GameHandler.isGameOver)//on gameover crunchie run faster
        {
            //moving
            transform.position = new Vector2(transform.position.x, transform.position.y - Time.deltaTime * curMinMaxSpeed.x * 1.5f);

            //set has reached the finish line
            if (!passedFinishLine && transform.position.y < CrunchieSpawner.finishLine.position.y)
            {
                passedFinishLine = true;
            }

            //Remove when out of bottom cam view
            if (CrunchieSpawner.checkObjectIsOutOfCameraView(transform.position))
                CrunchieSpawner.removeCrunchie(this);
        }      
    }

    public float getSpawnChance()
    {
        return spawnChance;
    }

}