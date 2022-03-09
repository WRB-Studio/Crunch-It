using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrunchieSpawner : MonoBehaviour
{
    public float crunchieSizeMultiplier = 1;
    public float spawnChance = 0.2f;
    public float increaseSpawnChancePerSecond = 0.01f;

    public Sprite[] faces;
    public Sprite[] facesBoss;
    public Sprite[] bodys;
    public List<Crunchie> crunchiePrefabs;

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
        if (Application.platform != RuntimePlatform.Android)
        {
            increaseSpawnChancePerSecond /= 8;
            spawnChance /= 20;
        }

        finishLine = GameObject.Find("FinishLine").transform;

        camBounds = getCamBoundBoxPositions();

        InvokeRepeating("invokeIncreasePropabilitie", 1, 1);
    }

    void Update()
    {
        if (!GameHandler.isGameOver && !GameHandler.getIsPause())
            instantiateCrunchie();

        for (int i = 0; i < instantiatedCrunchies.ToArray().Length; i++)
        {
            if (instantiatedCrunchies[i] != null)
                instantiatedCrunchies[i].updateCall();
        }

    }


    private void invokeIncreasePropabilitie()
    {
        if (GameHandler.getGameOver() || GameHandler.getIsPause())
            return;

        spawnChance += getIncreaseSpawnChancePerSecond();
        if (spawnChance > 1)
            spawnChance = 1;
    }

    public float getIncreaseSpawnChancePerSecond()
    {
        return increaseSpawnChancePerSecond;
    }

    public float getSpawnChance()
    {
        return spawnChance;
    }

    private void instantiateCrunchie()
    {
        float randomVal = Random.value * (UltimateMode.instance.currentMultiplier / 20);
        GameObject newCrunchie = null;
        Color randColor;

        Crunchie normalCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal);
        Crunchie fastCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast);
        Crunchie bossCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Boss);

        if (GameHandler.curDestroyed > 0 && GameHandler.curDestroyed % bossCrunchie.spawnAfterKills == 0 && !checkCrunchieTypeIsSpawned(Crunchie.eCrunchieTypes.Boss))//spawnhandling for boss crunchie
        {
            newCrunchie = Instantiate(bossCrunchie.gameObject);
            newCrunchie.GetComponent<SpriteRenderer>().sprite = bodys[Random.Range(0, bodys.Length)];
            newCrunchie.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = facesBoss[Random.Range(0, facesBoss.Length)];

            updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond);
        }
        else if (randomVal <= getSpawnChance())
        {
            if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast).getSpawnChance())//spawnhandling for fast crunchie
            {
                newCrunchie = Instantiate(fastCrunchie.gameObject);
                newCrunchie.GetComponent<SpriteRenderer>().sprite = bodys[Random.Range(0, bodys.Length)];
                newCrunchie.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = faces[Random.Range(0, faces.Length)];

                updateCrunchiePropertiers(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal).getSpawnChance())//spawnhandling for nomal crunchie
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
            
            UltimateMode.instance.setUltimateMode(UltimateMode.instance.currentMode);
            
            instantiatedCrunchies.Add(newCrunchie.GetComponent<Crunchie>());
        }

        if (instantiatedCrunchies.Count <= 0)
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

            newCrunchie.transform.position = randomSpawnpointOutOfCamView();
            newCrunchie.transform.parent = transform;

            UltimateMode.instance.setUltimateMode(UltimateMode.instance.currentMode);

            instantiatedCrunchies.Add(newCrunchie.GetComponent<Crunchie>());
        }
    }

    public void updateCrunchiePropertiers(Crunchie.eCrunchieTypes crunchieType, float updateValue)
    {
        Crunchie crunchie = getCrunchiePrefab(crunchieType);

        int destroyedCrunchies = GameHandler.curDestroyed;

        if (destroyedCrunchies <= 0)
            destroyedCrunchies = 1;

        crunchie.curMinMaxSpeed.x *= getIncreaseSpawnChancePerSecond() * destroyedCrunchies;
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

    public void setUltimateMode(Sprite crunchieFace = null)
    {
        for (int i = 0; i < instantiatedCrunchies.Count; i++)
        {
            instantiatedCrunchies[i].setUltimateMode(crunchieFace);
        }
    }
}
