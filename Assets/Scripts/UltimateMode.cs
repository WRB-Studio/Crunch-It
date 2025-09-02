using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltimateMode : MonoBehaviour
{
    public enum eUltimateModes
    {
        None, light, medium, full
    }

    [Header("Multiplier")]
    public eUltimateModes currentMode = eUltimateModes.None;
    public float currentMultiplier = 1;
    public float multiplierLight = 2;
    public float multiplierMedium = 4;
    public float multiplierFull = 6;

    [Header("start values")]
    public int lightModeAt = 10;
    public int mediumModeAt = 25;
    public int fullModeAt = 50;

    [Header("ultimate kill values")]
    public int ultimateKills = 0;
    public int ultimateSmasherByKills = 2;
    public GameObject ultimateSmashPrefab;
    public Image imgUltimateFrame;

    [Header("Crunchie designs")]
    public Sprite lightFace;
    public Sprite mediumFace;
    public Sprite fullFace;

    public static UltimateMode instance;



    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        instance.currentMode = eUltimateModes.None;

        instance.currentMultiplier = 1;

        instance.ultimateKills = 0;

        updateCall();
    }

    public void updateCall()
    {
        if (GameHandler.comboCounter > 0)
        {
            Color tmpColor = instance.imgUltimateFrame.color;
            tmpColor.a = Mathf.Lerp(0, 1, (float)GameHandler.comboCounter / instance.fullModeAt);
            instance.imgUltimateFrame.color = tmpColor;
        }
        else
        {
            Color tmpColor = instance.imgUltimateFrame.color;
            tmpColor.a = 0;
            instance.imgUltimateFrame.color = tmpColor;
        }

        instance.checkUltimateModes();
    }


    public void checkUltimateModes()
    {
        if (currentMode != eUltimateModes.None && GameHandler.comboCounter < lightModeAt)
        {
            setUltimateMode(eUltimateModes.None);
        }
        else if (currentMode != eUltimateModes.light && GameHandler.comboCounter >= lightModeAt && GameHandler.comboCounter < mediumModeAt)
        {
            setUltimateMode(eUltimateModes.light);
        }
        else if (currentMode != eUltimateModes.medium && GameHandler.comboCounter >= mediumModeAt && GameHandler.comboCounter < fullModeAt)
        {
            setUltimateMode(eUltimateModes.medium);
        }
        else if (currentMode != eUltimateModes.full && GameHandler.comboCounter >= fullModeAt)
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
                CrunchieSpawner.instance.SetUltimateMode();
                break;
            case eUltimateModes.light:
                currentMultiplier = multiplierLight;
                CrunchieSpawner.instance.SetUltimateMode(lightFace);
                break;
            case eUltimateModes.medium:
                currentMultiplier = multiplierMedium;
                CrunchieSpawner.instance.SetUltimateMode(mediumFace);
                break;
            case eUltimateModes.full:
                currentMultiplier = multiplierFull;
                CrunchieSpawner.instance.SetUltimateMode(fullFace);
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
                newSmasher.transform.localScale *= 0.6f;
                break;
            case eUltimateModes.medium:
                newSmasher.transform.localScale *= 0.8f;
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
