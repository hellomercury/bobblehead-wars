using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The marine.
    /// </summary>
    public GameObject Player;

    /// <summary>
    /// Spawn points in the map.
    /// </summary>
    public GameObject[] SpawnPoints;

    /// <summary>
    /// Alien prefab.
    /// </summary>
    public GameObject Alien;

    /// <summary>
    /// Determine how many aliens appear on the screen at once
    /// </summary>
    public int MaxAliensOnScreen;

    /// <summary>
    /// The total number of aliens the player must vanquish to claim victory.
    /// </summary>
    public int TotalAliens;

    /// <summary>
    /// The minimum rate at which aliens appear.
    /// </summary>
    public float MinSpawnTime;

    /// <summary>
    /// The maximum rate at which aliens appear.
    /// </summary>
    public float MaxSpawnTime;

    /// <summary>
    /// How many aliens appear during a spawning event.
    /// </summary>
    public int AliensPerSpawn;

    /// <summary>
    /// The size of the game object pool.
    /// </summary>
    public int AlienPoolSize = 10;

    /// <summary>
    /// Whether to spawn aliens.
    /// </summary>
    public bool Spawning = true;

    /// <summary>
    /// Global lock used to sync between threads.
    /// </summary>
    private Object _globalLock;

    /// <summary>
    /// The total number of aliens currently displayed.
    /// </summary>
    private int _aliensOnScreen;

    /// <summary>
    /// Track the time between spawn events.
    /// </summary>
    private float _generatedSpawnTime;

    /// <summary>
    /// Track the milliseconds since the last spawn.
    /// </summary>
    private float _currentSpawnTime;

    /// <summary>
    /// Alien pool to be reused.
    /// </summary>
    private List<GameObject> _alienPool;

    private void Awake()
    {
        _globalLock = new Object();
    }

    private void Start()
    {
        _alienPool = new List<GameObject>();
        for (int i = 0; i < AlienPoolSize; i++)
        {
            var newAlien = CreateAlien(i);
            newAlien.SetActive(false);
            _alienPool.Add(newAlien);
        }
    }

    private void Update()
    {
        // Spawn once reached time limit.
        _currentSpawnTime += Time.deltaTime;
        if (_currentSpawnTime > _generatedSpawnTime)
        {
            // Reset timer and regenerate time to spawn next wave.
            _currentSpawnTime = 0;
            _generatedSpawnTime = Random.Range(MinSpawnTime, MaxSpawnTime);

            // Only spawn if there are still aliens to be spawned before reaching total aliens.
            if (Spawning && AliensPerSpawn > 0 && _aliensOnScreen < TotalAliens)
            {
                // Keep track of already used spawn locations.
                var previousSpawnLocations = new HashSet<int>();

                // Ensure that number of aliens per spawn does not exceed number of spawn locations.
                if (AliensPerSpawn > SpawnPoints.Length)
                {
                    AliensPerSpawn = SpawnPoints.Length - 1;
                }

                // Clam the number of aliens per spawn to less than total number of aliens.
                AliensPerSpawn = AliensPerSpawn > TotalAliens ? AliensPerSpawn - TotalAliens : AliensPerSpawn;

                // Spawn all aliens for this wave.
                for (int i = 0; i < AliensPerSpawn; i++)
                {
                    // Only spawn if the number of aliens on screem does not exceed the maximum number of aliens on
                    // screen.
                    if (_aliensOnScreen < MaxAliensOnScreen)
                    {
                        _aliensOnScreen += 1;

                        // Find a spawn location that has not been used in this wave.
                        int spawnPoint = -1;
                        while (spawnPoint == -1)
                        {
                            var randomNumber = Random.Range(0, SpawnPoints.Length - 1);
                            if (!previousSpawnLocations.Contains(randomNumber))
                            {
                                previousSpawnLocations.Add(randomNumber);
                                spawnPoint = randomNumber;
                            }
                        }

                        // Select a spawn location and create a new alien at the specified location. Also set the
                        // target for the alien to follow. 
                        var spawnLocation = SpawnPoints[spawnPoint];
                        var newAlien = GetAlienFromPool();
                        newAlien.transform.position = spawnLocation.transform.position;
                        var targetRotation = new Vector3(Player.transform.position.x,
                            newAlien.transform.position.y, Player.transform.position.z);
                        newAlien.transform.LookAt(targetRotation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Disable the alien object in the pool with given index.
    /// </summary>
    /// <param name="index">The index of the alien object to be disabled</param>
    public void DisableAlien(int index)
    {
        lock (_globalLock)
        {
            _alienPool[index].SetActive(false);
            _aliensOnScreen -= 1;
        }
    }

    /// <summary>
    /// Return an alien game object from the pool.
    /// </summary>
    /// <returns>an alien game object from the pool</returns>
    private GameObject GetAlienFromPool()
    {
        GameObject alien = null;
        for (int i = 0; i < AlienPoolSize; i++)
        {
            if (!_alienPool[i].activeInHierarchy)
            {
                alien = _alienPool[i];
                alien.SetActive(true);
                break;
            }
        }

        if (alien == null)
        {
            alien = CreateAlien(AlienPoolSize);
            _alienPool.Add(alien);
            AlienPoolSize += 1;
        }

        return alien;
    }

    /// <summary>
    /// Create an alien object and populate fields of the object.
    /// </summary>
    /// <param name="i">Index of the new alien object in the pool</param>
    /// <returns></returns>
    private GameObject CreateAlien(int i)
    {
        var alien = Instantiate(Alien);
        var alienScript = alien.GetComponent<Alien>();
        alienScript.GameManagerScript = this;
        alienScript.Index = i;
        alienScript.Target = Player.transform;
        alien.transform.parent = gameObject.transform;
        return alien;
    }
}