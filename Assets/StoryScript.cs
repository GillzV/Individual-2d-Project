using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryScript : MonoBehaviour
{
    public AudioSource src;
    public AudioClip ClickSound;
    public void Continue()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        src.PlayOneShot(ClickSound);
    }
}
