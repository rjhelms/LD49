using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum Event
{
    NONE,
    PROTEST,
    TRANSPORT1,
    TRANSPORT2,
}

public enum GameState
{
    STARTING,
    RUNNING,
    GAMEOVER,
}

public class GameController : MonoBehaviour
{
    public GameObject musicPlayer;
    public int stability;
    public int maxStability;

    public Transform citizenParent;
    public GameObject citizenPrefab;

    public int citizenTarget;
    public float citizenSpawnTime;
    public float targetIncreaseTime;
    public float targetGrowth;

    public Event currentEvent;

    public int protestSize;
    public float protestGrowth;
    public float protestEndScale;

    public float eventQuietTime;
    public float eventLerpDown;

    public Text eventUIText;
    public Text datetimeUIText;
    public Image stabilityImage;

    public Color stabilityColorLow;
    public Color stabilityColorMid;
    public Color stabilityColorHigh;

    public Image FadeOverlay;

    public float fadeTime = 1f;

    public Transform pointerArrow;
    public Transform arrowTargetTransform;
    public float arrowCircleSize;

    public GameObject pickupPrefab;
    public GameObject dropoffPrefab;

    public int stabilityCitizenStagger;
    public int stabilityProtesterStagger;
    public int stabilityCitizenKill;
    public int stabilityProtesterKill;

    public int stabilityProtestDispersed;
    public int stabilitySupplyDropComplete;

    public int stabilityTickNone;
    public int stabilityTickProtest;
    public int stabilityTickTransport;

    public float stabilityTickTime;

    public float protestChance;
    public float protestLerpUp;
    public float protestLerpDown;

    public AudioClip dispatchClip;
    public AudioClip eventCompleteClip;
    public AudioClip innocentHitClip;
    public AudioClip innocentKillClip;
    public AudioClip protesterHitClip;
    public AudioClip protesterKillClip;
    public AudioClip gameOverClip;

    public GameState gameState;

    private int protestStartSize;
    private int protestRemaining;

    private GameObject[] citizenSpawners;
    private float nextCitizenSpawn;
    private float nextTargetIncreaseTime;
    private GameObject[] protestLocations;
    private GameObject[] transportLocations;
    private int lastTransportIndex;
    private float nextEventTime;
    private float nextStabilityTick;
    private PlayerController pc;
    private AudioSource audioSource;
    private float endFadeTime;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        if (!GameObject.FindGameObjectWithTag("Music"))
        {
            GameObject music = Instantiate(musicPlayer);
            DontDestroyOnLoad(music);
        }
            
        stability = maxStability;
        pointerArrow.gameObject.SetActive(false);
        pc = FindObjectOfType<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        eventUIText.text = "";
        protestLocations = GameObject.FindGameObjectsWithTag("ProtestSite");
        transportLocations = GameObject.FindGameObjectsWithTag("TransportSite");
        citizenSpawners = GameObject.FindGameObjectsWithTag("CitizenSpawner");
        currentEvent = Event.NONE;
        // on load, spawn to 75%
        int i = 0;
        while (citizenParent.childCount < citizenTarget * 0.75)
        {
            SpawnCitizen();
            i++;
            if (i >= citizenTarget)
            {
                Debug.Log("Spawned too many citizens!");
                return;
            }
        }
        Debug.Log("Spawned " + i + " citizens");
        nextEventTime = Time.time + eventQuietTime;
        nextCitizenSpawn = Time.time + citizenSpawnTime;
        nextTargetIncreaseTime = Time.time + targetIncreaseTime;
        nextStabilityTick = Time.time + stabilityTickTime;
        gameState = GameState.STARTING;
        endFadeTime = -1;   // dummy value so fade doesn't start till we're in update
        startTime = Time.fixedTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (gameState == GameState.STARTING)
        {
            if (endFadeTime == -1)
            {
                endFadeTime = Time.unscaledTime + fadeTime;
                return;
            }

            if (Time.unscaledTime > endFadeTime)
            {
                gameState = GameState.RUNNING;
                FadeOverlay.color = Color.clear;
                Time.timeScale = 1;
            }
            else
            {
                FadeOverlay.color = Color.Lerp(Color.clear, Color.black, endFadeTime - Time.unscaledTime);
                Debug.Log("start fade: " + (endFadeTime - Time.unscaledTime));
                startTime = Time.fixedTime;
            }
        }  

        if (gameState == GameState.GAMEOVER)
        {
            if (Time.unscaledTime > endFadeTime)
            {
                FadeOverlay.color = Color.black;
                Time.timeScale = 1;
                SceneManager.LoadScene("gameOver");
            }
            else
            {
                FadeOverlay.color = Color.Lerp(Color.black, Color.clear, endFadeTime - Time.unscaledTime); ;
            }
        }

        if (citizenParent.childCount < citizenTarget && Time.time > nextCitizenSpawn)
        {
            SpawnCitizen();
        }

        if (Time.time > nextTargetIncreaseTime)
        {
            int newCitizenTarget = (int)(citizenTarget * targetGrowth);
            if (newCitizenTarget == citizenTarget)
            {
                newCitizenTarget++;
            }
            citizenTarget = newCitizenTarget;
            nextTargetIncreaseTime = Time.time + targetIncreaseTime;
        }

        if (Time.time > nextStabilityTick)
        {
            nextStabilityTick = Time.time + stabilityTickTime;
            switch (currentEvent)
            {
                case Event.NONE:
                    stability += stabilityTickNone;
                    break;
                case Event.PROTEST:
                    stability += stabilityTickProtest;
                    break;
                case Event.TRANSPORT1:
                case Event.TRANSPORT2:
                    stability += stabilityTickTransport;
                    break;
            }
        }
        //if (Input.GetButtonDown("Fire2"))
        //{
        //    StartEvent();
        //}
        switch (currentEvent)
        {
            case Event.NONE:
                if (Time.time > nextEventTime)
                {
                    StartEvent();
                    eventQuietTime = Mathf.Lerp(eventQuietTime, 0, eventLerpDown);
                }
                pointerArrow.gameObject.SetActive(false);
                break;
            case Event.PROTEST:
                if (protestRemaining <= (protestStartSize * protestEndScale))
                {
                    eventUIText.text = "Protest dispersed";
                    stability += stabilityProtestDispersed;
                    currentEvent = Event.NONE;
                    audioSource.PlayOneShot(eventCompleteClip);
                    nextEventTime = Time.time + eventQuietTime;
                    GameObject[] citizens = GameObject.FindGameObjectsWithTag("Citizen");
                    foreach (GameObject citizenObject in citizens)
                    {
                        Citizen citizen = citizenObject.GetComponent<Citizen>();
                        if (citizen.state == CitizenState.PROTEST)
                            citizen.SetWalk();
                    }
                }
                pointerArrow.gameObject.SetActive(!arrowTargetTransform.GetComponent<SpriteRenderer>().isVisible);
                break;
            case Event.TRANSPORT1:
            case Event.TRANSPORT2:
                pointerArrow.gameObject.SetActive(!arrowTargetTransform.GetComponent<SpriteRenderer>().isVisible);
                break;
        }

        if (stability > maxStability)
            stability = maxStability;
        // UI updates
        datetimeUIText.text = (int)((Time.fixedTime - startTime) % 24) + ":00 DAY " + (int)((Time.fixedTime - startTime) / 24);
        if (pointerArrow.gameObject.activeSelf)
        {
            Vector2 arrowAimVector = (arrowTargetTransform.position - pc.transform.position).normalized;
            pointerArrow.position = arrowAimVector * arrowCircleSize;
            // force Z position
            pointerArrow.position = new Vector3(pointerArrow.position.x, pointerArrow.position.y, -190);

            float angle = Mathf.Atan2(arrowAimVector.y, arrowAimVector.x) * Mathf.Rad2Deg;
            pointerArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        stabilityImage.rectTransform.sizeDelta = new Vector2(stability, 8);

        if (stability <= 33)
        {
            stabilityImage.color = stabilityColorLow;
        } else if (stability <= 66)
        {
            stabilityImage.color = stabilityColorMid;
        } else
        {
            stabilityImage.color = stabilityColorHigh;
        }

        if (stability <= 0)
            GameOver();

        if (Input.GetKeyDown(KeyCode.R))
        {
            stability -= 10;
            pc.Reset();
        }
    }

    void SpawnCitizen()
    {
        Transform source = citizenSpawners[Random.Range(0, citizenSpawners.Length)].transform;
        Instantiate(citizenPrefab, source.position, Quaternion.identity, citizenParent);
        if (citizenParent.childCount < citizenTarget * 0.5f)
        {
            nextCitizenSpawn = Time.time + (citizenSpawnTime * 0.5f);
        }
        else
        {
            nextCitizenSpawn = Time.time + citizenSpawnTime;
        }
    }

    void StartEvent()
    {
        if (currentEvent != Event.NONE)
        {
            Debug.Log("Event already in progress");
            return;
        }

        if (Random.value < protestChance)
        {
            StartProtest();
            protestChance = Mathf.Lerp(protestChance, 0.0f, protestLerpDown);
        }
        else
        {
            StartTransport();
            protestChance = Mathf.Lerp(protestChance, 1.0f, protestLerpUp);
        }

        audioSource.PlayOneShot(dispatchClip);
    }

    void StartTransport()
    {

        lastTransportIndex = Random.Range(0, transportLocations.Length);
        Transform transportSite = transportLocations[lastTransportIndex].transform;
        arrowTargetTransform = transportSite;
        Instantiate(pickupPrefab, transportSite.position, Quaternion.identity);

        currentEvent = Event.TRANSPORT1;
        eventUIText.text = "Pick up supplies at " + transportSite.name;
    }


    void StartProtest()
    {
        Transform protestSite = protestLocations[Random.Range(0, protestLocations.Length)].transform;
        arrowTargetTransform = protestSite;
        GameObject[] citizens = GameObject.FindGameObjectsWithTag("Citizen");
        float[] distances = new float[citizens.Length];
        for (int i = 0; i < citizens.Length; i++)
        {
            distances[i] = Vector2.Distance(citizens[i].transform.position, protestSite.position);
        }
        Array.Sort(distances, citizens);
        protestStartSize = 0;
        int protestTarget = protestSize;
        for (int i = 0; i < protestTarget; i++)
        {
            if (citizens[i].GetComponent<Citizen>().state == CitizenState.WALK)
            {
                Debug.DrawLine(protestSite.position, citizens[i].transform.position, Color.red, 1);
                citizens[i].GetComponent<Citizen>().SetProtest(protestSite.position);
                protestStartSize++;
            }
            else
            {
                // if citizen isn't walking, gotta try one more
                protestTarget++;
            }

            // failsafe - can't check more citizens than there are
            if (protestTarget > citizens.Length)
                protestTarget = citizens.Length;
        }

        int newProtestSize = (int)(protestSize * protestGrowth);
        if (newProtestSize == protestSize)
        {
            newProtestSize++;
        }
        protestRemaining = protestStartSize;
        protestSize = newProtestSize;
        currentEvent = Event.PROTEST;
        eventUIText.text = "Illegal gathering at " + protestSite.name;
    }

    public void RegisterProtesterStagger()
    {
        protestRemaining--;
        stability += stabilityProtesterStagger;
        audioSource.PlayOneShot(protesterHitClip);
    }

    public void RegisterCitizenStagger()
    {
        stability += stabilityCitizenStagger;
        audioSource.PlayOneShot(innocentHitClip);
    }

    public void RegisterProtesterKill()
    {
        protestRemaining--;
        stability += stabilityProtesterKill;
        audioSource.PlayOneShot(protesterKillClip);
    }

    public void RegisterCitizenKill()
    {
        stability += stabilityCitizenKill;
        audioSource.PlayOneShot(innocentKillClip);
    }

    public void AdvanceTransportState()
    {
        if (currentEvent == Event.TRANSPORT1)
        {
            currentEvent = Event.TRANSPORT2;
            int nextTransportIndex = lastTransportIndex;
            while (nextTransportIndex == lastTransportIndex)
            {
                nextTransportIndex = Random.Range(0, transportLocations.Length);
            }
            Transform transportSite = transportLocations[nextTransportIndex].transform;
            arrowTargetTransform = transportSite;
            Instantiate(dropoffPrefab, transportSite.position, Quaternion.identity);
            eventUIText.text = "Drop supplies at " + transportSite.name;
            audioSource.PlayOneShot(dispatchClip);
        }
        else
        {
            eventUIText.text = "Supply drop complete";
            nextEventTime = Time.time + eventQuietTime;
            stability += stabilityProtestDispersed;
            currentEvent = Event.NONE;
            audioSource.PlayOneShot(eventCompleteClip);
        }
    }

    void GameOver()
    {
        if (gameState == GameState.RUNNING)
        {
            gameState = GameState.GAMEOVER;
            Time.timeScale = 0;
            eventUIText.text = "GAME OVER";
            eventUIText.color = new Color32(0xD7, 0x73, 0x55, 0xFF);
            audioSource.PlayOneShot(gameOverClip);
            endFadeTime = Time.unscaledTime + fadeTime;
            ScoreManager.Instance.score = (int)((Time.fixedTime - startTime) / 24);
        }
    }
}
