using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform citizenParent;
    public GameObject citizenPrefab;

    public int citizenTarget;
    public float citizenSpawnTime;
    
    private GameObject[] citizenSpawners;
    private float nextCitizenSpawn;
    
    // Start is called before the first frame update
    void Start()
    {
        citizenSpawners = GameObject.FindGameObjectsWithTag("CitizenSpawner");
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
        Debug.Log("Spawned " + i + "citizens");
    }

    // Update is called once per frame
    void Update()
    {
        if (citizenParent.childCount < citizenTarget && Time.time > nextCitizenSpawn)
        {
            SpawnCitizen();
        }
    }

    void SpawnCitizen()
    {
        Transform source = citizenSpawners[Random.Range(0, citizenSpawners.Length)].transform;
        Instantiate(citizenPrefab, source.position, Quaternion.identity, citizenParent);
        if (citizenParent.childCount < citizenTarget * 0.5f)
        {
            nextCitizenSpawn = Time.time + (citizenSpawnTime * 0.5f);
        } else
        {
            nextCitizenSpawn = Time.time + citizenSpawnTime;
        }
    }
}
