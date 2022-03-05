using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateMode : MonoBehaviour
{
    public enum eUltimateModes
    {
        None, light, medium, full
    }

    public eUltimateModes currentMode = eUltimateModes.None;

    public float currentMultiplier = 1;
    public float multiplierLight = 2;
    public float multiplierMedium = 4;
    public float multiplierFull = 6;

    public int lightModeAt = 10;
    public int mediumModeAt = 25;
    public int fullModeAt = 50;

    public Sprite lightFace;
    public Sprite mediumFace;
    public Sprite fullFace;

    public int ultimateKills = 0;
    public int ultimateSmasherByKills = 10;

    public GameObject ultimateSmashPrefab;

    public static UltimateMode instance;



    private void Awake()
    {
        instance = this;
    }

    public void checkUltimateModes()
    {
        if (currentMode != eUltimateModes.None && GameHandler.multiCounter < lightModeAt)
        {
            setUltimateMode(eUltimateModes.None);
        }
        else if (currentMode != eUltimateModes.light && GameHandler.multiCounter >= lightModeAt && GameHandler.multiCounter < mediumModeAt)
        {
            setUltimateMode(eUltimateModes.light);
        }
        else if (currentMode != eUltimateModes.medium && GameHandler.multiCounter >= mediumModeAt && GameHandler.multiCounter < fullModeAt)
        {
            setUltimateMode(eUltimateModes.medium);
        }
        else if (currentMode != eUltimateModes.full && GameHandler.multiCounter >= fullModeAt)
        {
            setUltimateMode(eUltimateModes.full);
        }
    }

    public void setUltimateMode(eUltimateModes setModeVal)
    {
        currentMode = setModeVal;

        switch (setModeVal)
        {
            case eUltimateModes.None:
                currentMultiplier = 1;
                CrunchieSpawner.instance.setUltimateMode();
                break;
            case eUltimateModes.light:
                currentMultiplier = multiplierLight;
                CrunchieSpawner.instance.setUltimateMode(lightFace);
                break;
            case eUltimateModes.medium:
                currentMultiplier = multiplierMedium;
                CrunchieSpawner.instance.setUltimateMode(mediumFace);
                break;
            case eUltimateModes.full:
                currentMultiplier = multiplierFull;
                CrunchieSpawner.instance.setUltimateMode(fullFace);
                break;
            default:
                break;
        }

        CancelInvoke("cancelUltimateMode");
        Invoke("cancelUltimateMode", 5);
    }

    public void addUltimateKill(int addVal)
    {
        if (currentMode != eUltimateModes.None)
        {
            ultimateKills += addVal;

            if (ultimateKills >= ultimateSmasherByKills)
            {
                ultimateKills = 0;
                createUltimateSmash(Input.mousePosition);
            }
        }
    }

    public void createUltimateSmash(Vector3 position)
    {
        GameObject newSmasher = Instantiate(ultimateSmashPrefab);
        Vector3 tmpPosition = Camera.main.ScreenToWorldPoint(position);
        tmpPosition.z = 10;
        newSmasher.transform.position = tmpPosition;
        newSmasher.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 359));

        switch (currentMode)
        {
            case eUltimateModes.None:
                break;
            case eUltimateModes.light:
                newSmasher.transform.localScale *= 1;
                break;
            case eUltimateModes.medium:
                newSmasher.transform.localScale *= 1.1f;
                break;
            case eUltimateModes.full:
                newSmasher.transform.localScale *= 1.2f;
                break;
            default:
                break;
        }
            Destroy(newSmasher, 3);
    }

    private void cancelUltimateMode()
    {
        setUltimateMode(eUltimateModes.None);
    }

}
