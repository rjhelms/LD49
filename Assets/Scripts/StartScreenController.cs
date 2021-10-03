using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    public GameObject musicPlayer;
    public float fadeTime;
    public ScreenState state;
    public Image FadeOverlay;
    public Text scoreText;
    public string nextScene;

    private float endFadeTime;

    // Start is called before the first frame update
    void Start()
    {
        endFadeTime = Time.unscaledTime + fadeTime;
        if (!GameObject.FindGameObjectWithTag("Music"))
        {
            GameObject music = Instantiate(musicPlayer);
            DontDestroyOnLoad(music);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case ScreenState.STARTING:
                if (Time.unscaledTime > endFadeTime)
                {
                    state = ScreenState.RUNNING;
                    FadeOverlay.color = Color.clear;
                }
                else
                {
                    FadeOverlay.color = Color.Lerp(Color.clear, Color.black, endFadeTime - Time.unscaledTime);
                }
                break;
            case ScreenState.RUNNING:
                if (Input.anyKey)
                {
                    state = ScreenState.ENDING;
                    endFadeTime = Time.unscaledTime + fadeTime;
                }
                break;
            case ScreenState.ENDING:
                if (Time.unscaledTime > endFadeTime)
                {
                    FadeOverlay.color = Color.black;
                    Time.timeScale = 1;
                    SceneManager.LoadScene(nextScene);
                }
                else
                {
                    FadeOverlay.color = Color.Lerp(Color.black, Color.clear, endFadeTime - Time.unscaledTime); ;
                }
                break;
        }
    }
}
