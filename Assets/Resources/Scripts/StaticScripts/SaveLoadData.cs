using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadData : MonoBehaviour
{

    private static string KEY = Application.productName + "/";

    public enum keyname
    {
        musicState,
        soundState,

        bestScore,
        bestMultiplier,
        bestMultiplierScore
    }

    static public bool checkApplicationHasSaves()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    static public void removeAll()
    {
        PlayerPrefs.DeleteAll();
    }


    //-------------SAVES-------------------------
    static public void saveMusicState(bool musicState)
    {
        int state;

        if (musicState)
            state = 1;
        else
            state = 0;

        PlayerPrefs.SetInt(KEY + keyname.musicState, state);
        PlayerPrefs.Save();
    }

    static public void saveSoundState(bool soundState)
    {
        int state;

        if (soundState)
            state = 1;
        else
            state = 0;

        PlayerPrefs.SetInt(KEY + keyname.soundState, state);
        PlayerPrefs.Save();
    }


    static public void saveBestScore(ulong score)
    {
        PlayerPrefs.SetString(KEY + keyname.bestScore, score.ToString());
        PlayerPrefs.Save();
    }

    static public void saveBestMultiplier(int multiplier)
    {
        PlayerPrefs.SetInt(KEY + keyname.bestMultiplier, multiplier);
        PlayerPrefs.Save();
    }

    static public void saveBestMultiplierScore(ulong multiplierScore)
    {
        PlayerPrefs.SetString(KEY + keyname.bestMultiplierScore, multiplierScore.ToString());
        PlayerPrefs.Save();
    }


    //-------------Loads-------------------------
    static public bool loadMusicState()
    {
        var state = PlayerPrefs.GetInt(KEY + keyname.musicState, 1);

        if (state == 1)
            return true;
        else
            return false;
    }

    static public bool loadSoundState()
    {
        var state = PlayerPrefs.GetInt(KEY + keyname.soundState, 1);

        if (state == 1)
            return true;
        else
            return false;
    }


    static public ulong loadBestScore()
    {
        return stringToUlong(PlayerPrefs.GetString(KEY + keyname.bestScore, null));
    }

    static public int loadBestMultiplier()
    {
        return PlayerPrefs.GetInt(KEY + keyname.bestMultiplier, 0);
    }

    static public ulong loadBestMultiplierScore()
    {
        return stringToUlong(PlayerPrefs.GetString(KEY + keyname.bestMultiplierScore, null));
    }


    static private ulong stringToUlong(string ulongString)
    {
        ulong toLong;
        ulong.TryParse(ulongString, out toLong);
        return toLong;
    }

}
