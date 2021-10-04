using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class FontBundleMenuScript : MonoBehaviour
{
    public GameObject[] allMenuPages; //all pages of the font bundle ttf demo
    public AudioClip[] menuSfxClips; //array of audio clips used by menu
    public Sprite[] pagesImages; // set of images used to give effect of changing page of a book
    public Image menuBodyPanel; // image that will use page images to display when required by code
    public Text pageCounterTxt; // text to display current page out of all pages
    private int menuPageIndex = 0; //track current page
    private int truePageCount = 0; // variable to hold number of pages - 1 the array length, 
    private AudioSource menuSfxAudioSource; //Audio Source for menu sound effects

	//Set page to start
	void OnEnable ()
    {
        truePageCount = allMenuPages.Length - 1;
        menuBodyPanel.sprite = pagesImages[0];
        menuSfxAudioSource = GetComponent<AudioSource>();
        menuSfxAudioSource.clip = menuSfxClips[0];
        menuPageIndex = 0;
        pageCounterTxt.text = "page " + menuPageIndex + "/" + truePageCount;
        for(int i = 0; i < allMenuPages.Length; i++)
        {
            allMenuPages[i].SetActive(false); //set all pages active false
        }
        allMenuPages[menuPageIndex].SetActive(true);
	}
	
    //function to load FBX demo scenes
	public void LoadFBXScene(int sceneIndex)
    {
        menuSfxAudioSource.clip = menuSfxClips[1];
        menuSfxAudioSource.Play();
        SceneManager.LoadScene(sceneIndex);
    }

    //function for controlling next page, hooked to button
    public void NextPageBtn()
    {
        if(menuPageIndex < allMenuPages.Length -1 && !menuSfxAudioSource.isPlaying)
        {
            menuSfxAudioSource.clip = menuSfxClips[0];
            menuSfxAudioSource.Play();
            menuBodyPanel.sprite = pagesImages[1];
            Invoke("yieldImageChange", 0.4f);
            Invoke("YieldAudio", menuSfxAudioSource.clip.length);
           /* for (int i = 0; i < allMenuPages.Length; i++)
            {
                allMenuPages[i].SetActive(false);
            }
            */
            menuPageIndex++;
            pageCounterTxt.text = "page " + menuPageIndex + "/" + truePageCount;
        }
        else
        {
            //play audio, notify of end pages.
        }
    }

    //function for controlling previous page, hooked to button
    public void PrevPageBtn()
    {
        if(menuPageIndex > 0 && !menuSfxAudioSource.isPlaying)
        {
            menuSfxAudioSource.clip = menuSfxClips[0];
            menuSfxAudioSource.Play();
            menuBodyPanel.sprite = pagesImages[2];
            Invoke("yieldAlternate", 0.4f);
            Invoke("YieldAudio", menuSfxAudioSource.clip.length);
            for (int i = 0; i < allMenuPages.Length; i++)
            {
                allMenuPages[i].SetActive(false);
            }
            menuPageIndex--;
            pageCounterTxt.text = "page " + menuPageIndex + "/" + truePageCount;
        }
        else
        {
            //play audio, notify of begin page.
        }
    }
    
    //function to jump to first page, hooked to button
    public void FirstPageBtn()
    {
        menuSfxAudioSource.clip = menuSfxClips[1];
        menuSfxAudioSource.Play();
        menuPageIndex = 0;
        for (int i = 0; i < allMenuPages.Length; i++)
        {
            allMenuPages[i].SetActive(false);
        }
        allMenuPages[menuPageIndex].SetActive(true);
        pageCounterTxt.text = "page " + menuPageIndex + "/" + truePageCount;
    }

    //function to jump to last page, hooked to button
    public void LastPageBtn()
    {
        menuSfxAudioSource.clip = menuSfxClips[1];
        menuSfxAudioSource.Play();
        menuPageIndex = allMenuPages.Length -1;
        for (int i = 0; i < allMenuPages.Length; i++)
        {
            allMenuPages[i].SetActive(false);
        }
        allMenuPages[menuPageIndex].SetActive(true);
        pageCounterTxt.text = "page " + menuPageIndex + "/" + truePageCount;
    }

    //function called from NextPageBtn to perform page transition
    private void yieldImageChange()
    {
        menuBodyPanel.sprite = pagesImages[2];
        for (int i = 0; i < allMenuPages.Length; i++)
        {
            allMenuPages[i].SetActive(false);
        }
    }

    //function called from PrevPageBtn to reverse Image change of transition
    private void yieldAlternate()
    {
        menuBodyPanel.sprite = pagesImages[1];
        allMenuPages[menuPageIndex].SetActive(true);
    }

    //function called from NextPageBtn and PrevPageBtn yields time of audio clip of page turn SFX before setting new page
    public void YieldAudio()
    {
        allMenuPages[menuPageIndex].SetActive(true);
        menuBodyPanel.sprite = pagesImages[0];
        return;
    }
}
