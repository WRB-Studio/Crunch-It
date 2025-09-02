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
        bestCombo,
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


    static public void saveBestScore(Int32 score)
    {
        PlayerPrefs.SetString(KEY + keyname.bestScore, score.ToString());
        PlayerPrefs.Save();
    }

    static public void saveBestCombo(int combo)
    {
        PlayerPrefs.SetInt(KEY + keyname.bestCombo, combo);
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


    static public Int32 loadBestScore()
    {
        return stringToInt32(PlayerPrefs.GetString(KEY + keyname.bestScore, null));
    }

    static public int loadBestCombo()
    {
        return PlayerPrefs.GetInt(KEY + keyname.bestCombo, 0);
    }


    static private Int32 stringToInt32(string Int32String)
    {
        Int32 toInt32;
        Int32.TryParse(Int32String, out toInt32);
        return toInt32;
    }

}
