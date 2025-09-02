using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrunchieSpawner : MonoBehaviour
{
    public float crunchieSizeMultiplier = 1;
    public Vector2 spawnChance;
    public Vector2 increaseSpawnChancePerSecond;

    public Sprite[] faces;
    public Sprite[] facesBoss;
    public Sprite[] bodys;
    public List<Crunchie> crunchiePrefabs;

    public Color colorSplitter;
    public float splitterScale;

    private static List<Crunchie> instantiatedCrunchies = new List<Crunchie>();

    private static Bounds camBounds;

    public static Transform finishLine;

    public static CrunchieSpawner instance;



    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        instance.spawnChance.x = instance.spawnChance.y;

        for (int i = 0; i < instantiatedCrunchies.Count; i++)
        {
            Destroy(instantiatedCrunchies[i].gameObject);
        }

        instantiatedCrunchies = new List<Crunchie>();


        if (Application.platform != RuntimePlatform.Android)
        {
            instance.increaseSpawnChancePerSecond.x = instance.increaseSpawnChancePerSecond.y / 8;
            instance.spawnChance.x = instance.spawnChance.y / 20;
        }

        finishLine = GameObject.Find("FinishLine").transform;

        camBounds = instance.getCamBoundBoxPositions();

        instance.InvokeRepeating("invokeIncreasePropabilitie", 1, 1);
    }

    public void UpdateCall()
    {
        if (!GameHandler.isGameOver && !GameHandler.GetIsPause())
            instance.InstantiateCrunchie();

        for (int i = 0; i < instantiatedCrunchies.ToArray().Length; i++)
        {
            if (instantiatedCrunchies[i] != null)
                instantiatedCrunchies[i].updateCall();
        }

    }


    private void invokeIncreasePropabilitie()
    {
        if (GameHandler.GetGameOver() || GameHandler.GetIsPause())
            return;

        spawnChance.x += getIncreaseSpawnChancePerSecond();
        if (spawnChance.x > 1)
            spawnChance.x = 1;
    }

    public float getIncreaseSpawnChancePerSecond()
    {
        return increaseSpawnChancePerSecond.x;
    }

    public float getSpawnChance()
    {
        return spawnChance.x;
    }

    private GameObject InstantiateCrunchie()
    {
        float randomVal = Random.value * (UltimateMode.instance.currentMultiplier / 20);
        GameObject newCrunchie = null;

        Crunchie normalCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal);
        Crunchie fastCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast);
        Crunchie bossCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Boss);
        Crunchie splitterCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Splitter);

        if (GameHandler.curDestroyed > 0 && GameHandler.curDestroyed % bossCrunchie.spawnAfterKills == 0 && !checkCrunchieTypeIsSpawned(Crunchie.eCrunchieTypes.Boss))//spawnhandling for boss crunchie
        {
            newCrunchie = Instantiate(bossCrunchie.gameObject);

            updateCrunchieProperties(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond.x);
        }
        else if (randomVal <= getSpawnChance())
        {
            if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast).spawnChance)//spawnhandling for fast crunchie
            {
                newCrunchie = Instantiate(fastCrunchie.gameObject);

                updateCrunchieProperties(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond.x);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Splitter).spawnChance)
            {
                newCrunchie = Instantiate(splitterCrunchie.gameObject);

                updateCrunchieProperties(Crunchie.eCrunchieTypes.Splitter, increaseSpawnChancePerSecond.x);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal).spawnChance)//spawnhandling for nomal crunchie
            {
                newCrunchie = Instantiate(normalCrunchie.gameObject);

                updateCrunchieProperties(Crunchie.eCrunchieTypes.Normal, increaseSpawnChancePerSecond.x);
                updateCrunchieProperties(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond.x);
                updateCrunchieProperties(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond.x);
                updateCrunchieProperties(Crunchie.eCrunchieTypes.Splitter, increaseSpawnChancePerSecond.x);
            }
        }

        if (instantiatedCrunchies.Count <= 0 && newCrunchie == null)
        {
            newCrunchie = Instantiate(normalCrunchie.gameObject);

            updateCrunchieProperties(Crunchie.eCrunchieTypes.Normal, increaseSpawnChancePerSecond.x);
            updateCrunchieProperties(Crunchie.eCrunchieTypes.Fast, increaseSpawnChancePerSecond.x);
            updateCrunchieProperties(Crunchie.eCrunchieTypes.Boss, increaseSpawnChancePerSecond.x);
            updateCrunchieProperties(Crunchie.eCrunchieTypes.Splitter, increaseSpawnChancePerSecond.x);
        }

        if (newCrunchie != null)
        {
            Sprite body = bodys[Random.Range(0, bodys.Length)];
            Sprite face;

            if (newCrunchie.GetComponent<Crunchie>().crunchieType == Crunchie.eCrunchieTypes.Boss)
                face = facesBoss[Random.Range(0, facesBoss.Length)];
            else
                face = faces[Random.Range(0, faces.Length)];

            newCrunchie.transform.position = randomSpawnpointOutOfCamView();
            newCrunchie.transform.parent = transform;

            instantiatedCrunchies.Add(newCrunchie.GetComponent<Crunchie>());

            newCrunchie.GetComponent<Crunchie>().Init(body, face);
        }

        return newCrunchie;
    }

    public void SpawnAfterSplitterDeath(Vector2 deathPosition)
    {
        Crunchie normalCrunchie = getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal);

        GameObject newCrunchie1 = Instantiate(normalCrunchie.gameObject);
        GameObject newCrunchie2 = Instantiate(normalCrunchie.gameObject);

        newCrunchie1.transform.position = deathPosition;
        newCrunchie2.transform.position = deathPosition;

        newCrunchie1.transform.parent = transform;
        newCrunchie2.transform.parent = transform;

        instantiatedCrunchies.Add(newCrunchie1.GetComponent<Crunchie>());
        instantiatedCrunchies.Add(newCrunchie2.GetComponent<Crunchie>());

        newCrunchie1.GetComponent<Crunchie>().Init(bodys[Random.Range(0, bodys.Length)], faces[Random.Range(0, faces.Length)], splitterScale);
        newCrunchie2.GetComponent<Crunchie>().Init(bodys[Random.Range(0, bodys.Length)], faces[Random.Range(0, faces.Length)], splitterScale);
    }


    public void updateCrunchieProperties(Crunchie.eCrunchieTypes crunchieType, float updateValue)
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

    public void removeCrunchie(Crunchie crunchie)
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

    public bool checkObjectIsOutOfCameraView(Vector3 objectPosition)
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

    public void SetUltimateMode(Sprite crunchieFace = null)
    {
        for (int i = 0; i < instantiatedCrunchies.Count; i++)
        {
            instantiatedCrunchies[i].setUltimateMode(crunchieFace);
        }
    }
}
