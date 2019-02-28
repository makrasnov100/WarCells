using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeVolume : MonoBehaviour
{
    
    public AudioSource bgMusic;
    public Slider slider;


    public void SetValue()
    {
        bgMusic.volume = slider.value;
    }
}
