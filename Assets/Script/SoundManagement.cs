using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagement : Singleton<SoundManagement>
{
    public AudioSource BGM;
    public AudioSource SFX;
    public AudioSource PlayerSFX;
    public List<AudioClip> SFXList;


    //≤•∑≈BGM
    public void PlayBGM()
    {
        BGM.Play();
    }

    //≤•∑≈“Ù–ß
    public void PlaySFX(int ID)
    {
        SFX.clip = SFXList[ID];
        SFX.Play();
    }
    public void PlayerNumberTwoSFX(int ID)
    {
        PlayerSFX.clip = SFXList[ID];
        PlayerSFX.Play();
    }
}
