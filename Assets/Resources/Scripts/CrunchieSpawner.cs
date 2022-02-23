using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrunchieSpawner : MonoBehaviour
{
    public Sprite[] faces;
    public Sprite[] bodys;
    public List<Crunchie> crunchiePrefabs;
    public float spawnChance = 0.2f;
    public float increaseSpawnChancePerSecond = 0.01f;

    private static List<Crunchie> instantiatedCrunchies = new List<Crunchie>();

    private static Bounds camBounds;

    public static Transform finishLine;

    public static CrunchieSpawner instance;



    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        finishLine = GameObject.Find("FinishLine").transform;

        camBounds = getCamBoundBoxPositions();

        InvokeRepeating("invokeIncreasePropabilitie", 1, 1);
    }

    void Update()
    {
        if (GameHandler.getGameOver() || GameHandler.getIsPause())
            return;

        instantiateCrunchie();

        for (int i = 0; i < instantiatedCrunchies.ToArray().Length; i++)
        {
            Crunchie currentCrunchie = instantiatedCrunchies[i];

            //moving
            currentCrunchie.transform.position = new Vector2(currentCrunchie.transform.position.x, currentCrunchie.transform.position.y - Time.deltaTime * currentCrunchie.curMinMaxSpeed.x);

            //set has reached the finish line
            if (!currentCrunchie.passedFinishLine && currentCrunchie.transform.position.y < finishLine.position.y)
            {
                currentCrunchie.passedFinishLine = true;
                GameHandler.addLife(-1);
            }

            //Remove when out of bottom cam view
            if (checkObjectIsOutOfCameraView(currentCrunchie.transform.position))
                removeCrunchie(currentCrunchie);
        }

    }


    private void invokeIncreasePropabilitie()
    {
        if (GameHandler.getGameOver() || GameHandler.getIsPause())
            return;

        spawnChance += increaseSpawnChancePerSecond;
    }


    private void instantiateCrunchie()
    {
        float randomVal = Random.value;
        GameObject newCrunchie = null;
        Color randColor;

        Crunchie normalCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal);
        Crunchie fastCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast);
        Crunchie bossCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Boss);

        
        if (GameHandler.curDestroyed > 0 && GameHandler.curDestroyed % bossCrunchie.spawnAfterKills == 0 && !checkCrunchieTypeIsSpawned(Crunchie.eCrunchieTypes.Boss))//spawnhandling for boss crunchie
        {
            newCrunchie = Instantiate(bossCrunchie.gameObject);
            newCrunchie.GetComponent<SpriteRenderer>().sprite = bodys[Random.Range(0, bodys.Length)];
            newCrunchie.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = faces[Random.Range(0, faces.Length)];

            updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond);
        }
        else if(randomVal <= spawnChance)
        {
            if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast).spawnChance)//spawnhandling for fast crunchie
            {
                newCrunchie = Instantiate(fastCrunchie.gameObject);
                newCrunchie.GetComponent<SpriteRenderer>().sprite = bodys[Random.Range(0, bodys.Length)];
                newCrunchie.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = faces[Random.Range(0, faces.Length)];

                updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal).spawnChance)//spawnhandling for nomal crunchie
            {
                newCrunchie = Instantiate(normalCrunchie.gameObject);
                newCrunchie.GetComponent<SpriteRenderer>().sprite = bodys[Random.Range(0, bodys.Length)];
                newCrunchie.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = faces[Random.Range(0, faces.Length)];

                randColor = Random.ColorHSV(0.1f, 0.1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
                newCrunchie.GetComponent<SpriteRenderer>().color = randColor;
                newCrunchie.transform.GetChild(1).GetComponent<SpriteRenderer>().color = randColor;

                updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Normal, increaseSpawnChancePerSecond);
                updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond);
                updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond);
            }
        }        

        if (newCrunchie != null)
        {
            newCrunchie.transform.position = randomSpawnpointOutOfCamView();
            newCrunchie.transform.parent = transform;
            instantiatedCrunchies.Add(newCrunchie.GetComponent<Crunchie>());
        }
    }

    public void updateCrunchiePropertiers(Crunchie.eCrunchieTypes crunchieType, float updateValue)
    {
        Crunchie crunchie = getCrunchiePrefab(crunchieType);

        crunchie.spawnChance += increaseSpawnChancePerSecond;
        crunchie.curMinMaxSpeed.y += increaseSpawnChancePerSecond;
        crunchie.curMinMaxSpeed.z += increaseSpawnChancePerSecond;
    }

    public bool checkCrunchieTypeIsSpawned(Crunchie.eCrunchieTypes crunchieType)
    {
        for (int i = 0; i < instantiatedCrunchies.Count; i++)
        {
            if (instantiatedCrunchies[i].crunchieType == crunchieType)
                return true;
        }

        return false;
    }

    public Crunchie getCrunchiePrefab(Crunchie.eCrunchieTypes crunchieType)
    {
        for (int i = 0; i < crunchiePrefabs.Count; i++)
        {
            if (crunchiePrefabs[i].GetComponent<Crunchie>().crunchieType == crunchieType)
                return crunchiePrefabs[i];
        }

        return null;
    }

    public static void removeCrunchie(Crunchie crunchie)
    {
        instantiatedCrunchies.Remove(crunchie);
        Destroy(crunchie.gameObject);
    }

    private Vector2 randomSpawnpointOutOfCamView()
    {
        float randPosX = Random.Range(camBounds.min.x, camBounds.max.x);
        float randPosY = Random.Range(camBounds.max.y + 1f, camBounds.max.y + 1.5f);

        return new Vector3(randPosX, randPosY, 0);
    }

    private Bounds getCamBoundBoxPositions()
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        Bounds bounds = new Bounds(
            Camera.main.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));

        return bounds;
        //Debug.Log("max x: " + bounds.max.x + "; max y: " + bounds.max.y + "; min x: " + bounds.min.x + "; min y: " + bounds.min.y);
    }

    public static bool checkObjectIsOutOfCameraView(Vector3 objectPosition)
    {
        if (objectPosition.x > camBounds.max.x + 1.5f ||
            objectPosition.x < camBounds.min.x - 1.5f ||
            objectPosition.y > camBounds.max.y + 1.5f ||
            objectPosition.y < camBounds.min.y - 1.5f)
        {
            return true;
        }
        return false;
    }

    public List<Crunchie> getCrunchyList()
    {
        return instantiatedCrunchies;
    }


}
