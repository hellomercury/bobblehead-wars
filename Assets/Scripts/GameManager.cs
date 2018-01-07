﻿using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Single instance of the game manager to be referenced elsewhere.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// The marine.
    /// </summary>
    public GameObject Player;

    #region AlienRegion

    /// <summary>
    /// Spawn points in the map.
    /// </summary>
    public GameObject[] SpawnPoints;

    /// <summary>
    /// Alien prefab.
    /// </summary>
    public GameObject AlienPrefab;

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
    /// Container to be used to contain all of the pooled aliens.
    /// </summary>
    public GameObject AlienContainer;

    /// <summary>
    /// Whether to spawn aliens.
    /// </summary>
    public bool AlienSpawning = true;

    /// <summary>
    /// Global lock used to sync between threads.
    /// </summary>
    private Object _alienGlobalLock;

    /// <summary>
    /// The total number of aliens currently displayed.
    /// </summary>
    private int _aliensOnScreen;

    /// <summary>
    /// Track the time between spawn events.
    /// </summary>
    private float _generatedAlienSpawnTime;

    /// <summary>
    /// Track the milliseconds since the last spawn.
    /// </summary>
    private float _currentAlienSpawnTime;

    /// <summary>
    /// Alien pool to be reused.
    /// </summary>
    private List<GameObject> _availableAlienPool;

    #endregion

    #region PickupRegion

    /// <summary>
    /// Pickup prefab used for cloning.
    /// </summary>
    public GameObject PickupPrefab;

    /// <summary>
    /// Container to be used to contain all of the pooled pickups.
    /// </summary>
    public GameObject PickupContainer;

    /// <summary>
    /// How many pickups to be put into the pool.
    /// </summary>
    public int PickupPoolSize = 3;

    /// <summary>
    /// Whether to spawn pickups.
    /// </summary>
    public bool PickupSpawning = true;

    /// <summary>
    /// The gun to be upgraded.
    /// </summary>
    public Gun Gun;

    /// <summary>
    /// Maximum time that will pass before the upgrade spawns.
    /// </summary>
    public float UpgradeMaxTimeSpawn = 10f;

    /// <summary>
    /// Whether the upgrade has spawned or not since it can only spawn once.
    /// </summary>
    private bool _spawnedUpgrade;

    /// <summary>
    /// Time limit of the pickup.
    /// </summary>
    private float _actualUpgradeTime;

    /// <summary>
    /// Time that has passed since the last pickup was picked up.
    /// </summary>
    private float _currentUpgradeTime;

    /// <summary>
    /// Pool of pickup objects to be reused.
    /// </summary>
    private List<GameObject> _availablePickupPool;

    /// <summary>
    /// Global lock to be used to sync pickup-related operations.
    /// </summary>
    private Object _pickupGlobalLock;

    #endregion

    private void Awake()
    {
        Instance = this;
        _alienGlobalLock = new Object();
        _pickupGlobalLock = new Object();
    }

    private void Start()
    {
        _availableAlienPool = new List<GameObject>();
        for (int i = 0; i < AlienPoolSize; i++)
        {
            var newAlien = CreateAlien(i);
            newAlien.SetActive(false);
            _availableAlienPool.Add(newAlien);
        }

        _availablePickupPool = new List<GameObject>();
        for (int i = 0; i < PickupPoolSize; i++)
        {
            var pickup = CreatePickup(i);
            pickup.SetActive(false);
            _availablePickupPool.Add(pickup);
        }

        _actualUpgradeTime = Mathf.Abs(Random.Range(UpgradeMaxTimeSpawn - 3.0f, UpgradeMaxTimeSpawn));
    }

    private void Update()
    {
        // Spawn once reached time limit.
        _currentAlienSpawnTime += Time.deltaTime;
        if (_currentAlienSpawnTime > _generatedAlienSpawnTime)
        {
            // Reset timer and regenerate time to spawn next wave.
            _currentAlienSpawnTime = 0;
            _generatedAlienSpawnTime = Random.Range(MinSpawnTime, MaxSpawnTime);

            // Only spawn if there are still aliens to be spawned before reaching total aliens.
            if (AlienSpawning && AliensPerSpawn > 0 && _aliensOnScreen < TotalAliens)
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

        // Spawn pickup when time exceed.
        _currentUpgradeTime += Time.deltaTime;
        if (_currentUpgradeTime > _actualUpgradeTime)
        {
            _currentUpgradeTime = 0;
            if (PickupSpawning && !_spawnedUpgrade)
            {
                // Select a spawning location
                var randomNumber = Random.Range(0, SpawnPoints.Length - 1);
                var spawnLocation = SpawnPoints[randomNumber];
                // Get a pickup from the pickup pool.
                var upgrade = GetPickupFromPool();
                upgrade.transform.position = spawnLocation.transform.position;
                _spawnedUpgrade = true;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.PowerUpAppear);
            }
        }
    }

    /// <summary>
    /// Disable the alien object in the pool with given index.
    /// </summary>
    /// <param name="index">The index of the alien object to be disabled</param>
    public void DisableAlien(int index)
    {
        lock (_alienGlobalLock)
        {
            _availableAlienPool[index].SetActive(false);
            _aliensOnScreen -= 1;
        }
    }

    /// <summary>
    /// Disable the pickup object in the pool with given index.
    /// </summary>
    /// <param name="index">The index of the alien object to be disabled</param>
    public void DisablePickup(int index)
    {
        lock (_pickupGlobalLock)
        {
            _availablePickupPool[index].SetActive(false);
            _spawnedUpgrade = false;
        }
    }

    /// <summary>
    /// Return an alien game object from the pool.
    /// </summary>
    /// <returns>an alien game object from the pool</returns>
    private GameObject GetAlienFromPool()
    {
        // Return any "free" alien in the alien pool.
        for (int i = 0; i < AlienPoolSize; i++)
        {
            if (!_availableAlienPool[i].activeInHierarchy)
            {
                var alien = _availableAlienPool[i];
                alien.SetActive(true);
                return alien;
            }
        }

        // Createa a new alien and return it after adding it to the alien pool.
        var newAlien = CreateAlien(AlienPoolSize);
        _availableAlienPool.Add(newAlien);
        AlienPoolSize += 1;
        return newAlien;
    }

    /// <summary>
    /// Create an alien object and populate fields of the object.
    /// </summary>
    /// <param name="i">Index of the new alien object in the pool</param>
    /// <returns></returns>
    private GameObject CreateAlien(int i)
    {
        var alien = Instantiate(AlienPrefab);
        var alienScript = alien.GetComponent<Alien>();
        alienScript.Index = i;
        alienScript.Target = Player.transform;
        alien.transform.parent = AlienContainer.transform;
        return alien;
    }

    /// <summary>
    /// Return an pickup game object from the pool.
    /// </summary>
    /// <returns>an pickup game object from the pool</returns>
    private GameObject GetPickupFromPool()
    {
        // Return any "free" pickup in the alien pool.
        for (int i = 0; i < PickupPoolSize; i++)
        {
            if (!_availablePickupPool[i].activeInHierarchy)
            {
                var pickup = _availablePickupPool[i];
                pickup.SetActive(true);
                return pickup;
            }
        }

        // Createa a new pickup and return it after adding it to the pickup pool.
        var newPickup = CreatePickup(PickupPoolSize);
        _availablePickupPool.Add(newPickup);
        PickupPoolSize += 1;
        return newPickup;
    }

    /// <summary>
    /// Create an pickup object and populate fields of the object.
    /// </summary>
    /// <param name="i">Index of the new pickup object in the pool</param>
    /// <returns></returns>
    private GameObject CreatePickup(int i)
    {
        var pickup = Instantiate(PickupPrefab);
        var upgradeScript = pickup.GetComponent<Upgrade>();
        upgradeScript.Index = i;
        pickup.transform.parent = PickupContainer.transform;
        upgradeScript.Gun = Gun;
        return pickup;
    }
}