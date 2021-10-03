using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ScreenState
{
    STARTING,
    RUNNING,
    ENDING
}

public class EndScreenController : MonoBehaviour
{
    public float fadeTime;
    public ScreenState state;
    public Image FadeOverlay;
    public Text scoreText;

    private float endFadeTime;

    // Start is called before the first frame update
    void Start()
    {
        endFadeTime = Time.unscaledTime + fadeTime;
        scoreText.text = ScoreManager.Instance.score.ToString();
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
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                }
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
                    SceneManager.LoadScene("main"); // TODO - change this
                }
                else
                {
                    FadeOverlay.color = Color.Lerp(Color.black, Color.clear, endFadeTime - Time.unscaledTime); ;
                }
                break;
        }
    }
}
