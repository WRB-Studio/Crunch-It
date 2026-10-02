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
    private const double spawnInterval = 1.0 / 30.0;
    private double spawnElapsed;
    private double difficultyElapsed;
    private int lastBossSpawnKills;



    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        CancelInvoke();
        instance.spawnChance.x = instance.spawnChance.y;
        instance.increaseSpawnChancePerSecond.x = instance.increaseSpawnChancePerSecond.y;
        spawnElapsed = 0;
        difficultyElapsed = 0;
        lastBossSpawnKills = 0;

        for (int i = 0; i < instantiatedCrunchies.Count; i++)
        {
            if (instantiatedCrunchies[i] != null)
            {
                instantiatedCrunchies[i].gameObject.SetActive(false);
                Destroy(instantiatedCrunchies[i].gameObject);
            }
        }

        instantiatedCrunchies.Clear();
        foreach (Transform effect in transform)
        {
            effect.gameObject.SetActive(false);
            Destroy(effect.gameObject);
        }



        finishLine = GameObject.Find("FinishLine").transform;

        camBounds = instance.getCamBoundBoxPositions();

    }

    public void UpdateCall()
    {
        if (GameHandler.isGameOver || GameHandler.GetIsPause())
            return;
        AdvanceSpawning(Time.deltaTime);

        for (int i = instantiatedCrunchies.Count - 1; i >= 0; i--)
        {
            if (instantiatedCrunchies[i] != null)
                instantiatedCrunchies[i].updateCall();
        }

    }


    private void AdvanceSpawning(float deltaTime)
    {
        difficultyElapsed += deltaTime;
        while (difficultyElapsed >= 1f)
        {
            difficultyElapsed -= 1f;
            IncreaseSpawnChance();
        }

        // Use the same spawn rate on every platform and frame rate.
        spawnElapsed += deltaTime;
        while (spawnElapsed >= spawnInterval)
        {
            spawnElapsed -= spawnInterval;
            InstantiateCrunchie();
        }
    }

    private void IncreaseSpawnChance()
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

        if (GameHandler.curDestroyed > 0 && GameHandler.curDestroyed != lastBossSpawnKills && GameHandler.curDestroyed % bossCrunchie.spawnAfterKills == 0 && !checkCrunchieTypeIsSpawned(Crunchie.eCrunchieTypes.Boss))//spawnhandling for boss crunchie
        {
            newCrunchie = Instantiate(bossCrunchie.gameObject);
            lastBossSpawnKills = GameHandler.curDestroyed;
        }
        else if (randomVal <= getSpawnChance())
        {
            if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Fast).spawnChance)//spawnhandling for fast crunchie
            {
                newCrunchie = Instantiate(fastCrunchie.gameObject);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Splitter).spawnChance)
            {
                newCrunchie = Instantiate(splitterCrunchie.gameObject);
            }
            else if (Random.value <= getCrunchiePrefab(Crunchie.eCrunchieTypes.Normal).spawnChance)//spawnhandling for nomal crunchie
            {
                newCrunchie = Instantiate(normalCrunchie.gameObject);
            }
        }

        if (instantiatedCrunchies.Count <= 0 && newCrunchie == null)
        {
            newCrunchie = Instantiate(normalCrunchie.gameObject);
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
        if (!instantiatedCrunchies.Remove(crunchie))
            return;
        crunchie.gameObject.SetActive(false);
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
