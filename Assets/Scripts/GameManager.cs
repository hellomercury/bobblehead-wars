using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    #region AlienHeadRegion

    /// <summary>
    /// The default position of the alien head with respect to the alien.
    /// </summary>
    private Vector3 _defaultAlienHeadPosition;

    /// <summary>
    /// The default rotation of the alien head with respect to the alien.
    /// </summary>
    private Quaternion _defaultAlienHeadRotation;

    #endregion

    private void Awake()
    {
        Instance = this;
        _alienGlobalLock = new Object();
        _pickupGlobalLock = new Object();
        var alienHeadTransform = AlienPrefab.GetComponent<Alien>().Head.transform;
        _defaultAlienHeadPosition = Utility.ClonePosition(alienHeadTransform.localPosition);
        _defaultAlienHeadRotation = Utility.CloneRotation(alienHeadTransform.localRotation);
    }

    private void Start()
    {
        _availableAlienPool = new List<GameObject>();
        for (int i = 0; i < AlienPoolSize; i++)
        {
            var newAlien = CreateAlien(i);
            Utility.FindChildWithTag(newAlien, "AlienHead").SetActive(false);
            Utility.FindChildWithTag(newAlien, "AlienBody").SetActive(false);
            newAlien.GetComponent<SphereCollider>().enabled = false;
            newAlien.GetComponent<NavMeshAgent>().enabled = false;
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
        if (Player == null)
        {
            return;
        }

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
    /// <param name="alienGameObject">The alien game object that is to be disabled</param>
    private void DisableAlien(int index, GameObject alienGameObject)
    {
        lock (_alienGlobalLock)
        {
            _aliensOnScreen -= 1;
            TotalAliens -= 1;

            var alienScript = alienGameObject.GetComponent<Alien>();
            alienScript.Body.SetActive(false);
            alienGameObject.GetComponent<SphereCollider>().enabled = false;
            alienGameObject.GetComponent<NavMeshAgent>().enabled = false;
            ToogleAlienHeadDetachment(alienScript, false);

            alienScript.Head.gameObject.SetActive(true);
            var force = new Vector3(Random.Range(0f, 30f), Random.Range(0f, 30f), Random.Range(0f, 30f));
//            alienScript.Head.AddRelativeForce(new Vector3(0, 26.0f, 3.0f), ForceMode.VelocityChange);
            alienScript.Head.AddRelativeForce(force, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// Return an alien game object from the pool.
    /// </summary>
    /// <returns>an alien game object from the pool</returns>
    private GameObject GetAlienFromPool()
    {
        GameObject alien = null;
        lock (_alienGlobalLock)
        {
            // Return any "free" alien in the alien pool.
            for (int i = 0; i < AlienPoolSize; i++)
            {
                var currentAlien = _availableAlienPool[i];
                var currentAlienHead = Utility.FindChildWithTag(currentAlien, "AlienHead");
                var currentAlienBody = Utility.FindChildWithTag(currentAlien, "AlienBody");
                if (!currentAlienBody.activeSelf && !currentAlienHead.activeSelf)
                {
                    alien = currentAlien;
                    alien.SetActive(true);
                    var alienScript = alien.GetComponent<Alien>();
                    ToogleAlienHeadDetachment(alienScript, true);
                    currentAlienHead.SetActive(true);
                    currentAlienHead.transform.localPosition = Utility.ClonePosition(_defaultAlienHeadPosition);
                    currentAlienHead.transform.localRotation = Utility.CloneRotation(_defaultAlienHeadRotation);
                    currentAlienBody.SetActive(true);
                    alien.GetComponent<SphereCollider>().enabled = true;
                    alien.GetComponent<NavMeshAgent>().enabled = true;
                    break;
                }
            }

            // Createa a new alien and return it after adding it to the alien pool.
            if (alien == null)
            {
                alien = CreateAlien(AlienPoolSize);
                _availableAlienPool.Add(alien);
                AlienPoolSize += 1;
            }
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
        var alien = Instantiate(AlienPrefab);
        var alienScript = alien.GetComponent<Alien>();
        alienScript.Index = i;
        alienScript.Target = Player.transform;
        alienScript.OnDestroyEvent.AddListener(DisableAlien);
        alien.transform.parent = AlienContainer.transform;
        return alien;
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
    /// Return an pickup game object from the pool.
    /// </summary>
    /// <returns>an pickup game object from the pool</returns>
    private GameObject GetPickupFromPool()
    {
        GameObject pickup = null;
        lock (_pickupGlobalLock)
        {
            // Return any "free" pickup in the alien pool.
            for (int i = 0; i < PickupPoolSize; i++)
            {
                if (!_availablePickupPool[i].activeInHierarchy)
                {
                    pickup = _availablePickupPool[i];
                    pickup.SetActive(true);
                    break;
                }
            }

            // Createa a new pickup and return it after adding it to the pickup pool.
            if (pickup == null)
            {
                pickup = CreatePickup(PickupPoolSize);
                _availablePickupPool.Add(pickup);
                PickupPoolSize += 1;
            }
        }

        return pickup;
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

    private void ToogleAlienHeadDetachment(Alien alienScript, bool isAlive)
    {
        alienScript.IsAlive = isAlive;
        alienScript.Head.GetComponent<Animator>().enabled = isAlive;
        alienScript.Head.isKinematic = isAlive;
        alienScript.Head.useGravity = !isAlive;
        alienScript.Head.GetComponent<SphereCollider>().enabled = !isAlive;
    }
}