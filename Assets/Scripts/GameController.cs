using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public enum Event
{
    NONE,
    PROTEST,
    TRANSPORT,
}

public class GameController : MonoBehaviour
{
    public Transform citizenParent;
    public GameObject citizenPrefab;

    public int citizenTarget;
    public float citizenSpawnTime;

    public Event currentEvent;

    public int protestSize;
    public float protestGrowth;
    public float protestEndScale;

    public Text eventUIText;
    public Text speedUIText;

    private int protestStartSize;
    private int protestRemaining;

    private GameObject[] citizenSpawners;
    private float nextCitizenSpawn;
    private GameObject[] protestLocations;
    private GameObject[] transportLocations;
    
    private PlayerController pc;

    // Start is called before the first frame update
    void Start()
    {
        pc = FindObjectOfType<PlayerController>();
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
    }

    // Update is called once per frame
    void Update()
    {
        if (citizenParent.childCount < citizenTarget && Time.time > nextCitizenSpawn)
        {
            SpawnCitizen();
        }
        if (Input.GetButtonDown("Fire2"))
        {
            StartProtest();
        }
        switch (currentEvent)
        {
            case Event.PROTEST:
                if (protestRemaining <= (protestStartSize * protestEndScale))
                {
                    eventUIText.text = "Protest succesfully dispersed";
                    currentEvent = Event.NONE;
                    GameObject[] citizens = GameObject.FindGameObjectsWithTag("Citizen");
                    foreach (GameObject citizenObject in citizens)
                    {
                        Citizen citizen = citizenObject.GetComponent<Citizen>();
                        if (citizen.state == CitizenState.PROTEST)
                            citizen.SetWalk();
                    }
                }
                break;
        }

        // UI updates
        speedUIText.text = pc.speedmph + " mph";
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

    void StartProtest()
    {
        if (currentEvent != Event.NONE)
        {
            Debug.Log("Event already in progress");
            return;
        }

        Transform protestSite = protestLocations[Random.Range(0, protestLocations.Length)].transform;
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
        eventUIText.text = protestStartSize + " protestors rioting at " + protestSite.name;
    }

    public void RegisterProtesterStagger()
    {
        protestRemaining--;
    }

    public void RegisterCitizenStagger()
    {

    }

    public void RegisterProtesterKill()
    {
        protestRemaining--;
    }

    public void RegisterCitizenKill()
    {

    }
}
