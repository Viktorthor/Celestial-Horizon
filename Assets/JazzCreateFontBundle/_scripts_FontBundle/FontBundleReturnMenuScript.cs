using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class FontBundleReturnMenuScript : MonoBehaviour
{
    public AudioClip exitBtnSFX; //exit button sound effect
    private AudioSource exitBtnAudioSource;

    //get reference to audio source and set audio clip
    private void Start()
    {
        exitBtnAudioSource = GetComponent<AudioSource>();
        exitBtnAudioSource.clip = exitBtnSFX;
    }

    //load the menu
    public void LoadFontMenu()
    {
        exitBtnAudioSource.Play();
        SceneManager.LoadScene(0);
    }
}
