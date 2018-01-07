using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    /// <summary>
    /// Projectil prefab that will be instantiated from this game object.
    /// </summary>
    public GameObject BulletPrefab;

    /// <summary>
    /// Launch position of the projectile.
    /// </summary>
    public Transform LaunchPosition;

    /// <summary>
    /// Launch speed of the projectile.
    /// </summary>
    public float LaunchSpeed = 200;

    /// <summary>
    /// How fast the launching of projectiles is.
    /// </summary>
    public float LaunchRate = 0.1f;

    /// <summary>
    /// How many projectiles in the pool.
    /// </summary>
    public int ProjectilePoolCount = 10;

    /// <summary>
    /// Whether the gun has been upgraded.
    /// </summary>
    public bool IsUpgraded;

    /// <summary>
    /// A game object to contain all of the pooled projectiles.
    /// </summary>
    public GameObject ProjectileContainer;

    /// <summary>
    /// How long the upgrade will last.
    /// </summary>
    public float UpgradeTime = 5.0f;

    /// <summary>
    /// Pool of projectiles that can be reused.
    /// </summary>
    private List<GameObject> _projectilePool;

    /// <summary>
    /// Audio source used when firing a projectile.
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// Keep track of how long has it been since a power-up was picked up.
    /// </summary>
    private float _currentUpgradeTime;

    private void Start()
    {
        // Populate the projectile pool.
        _projectilePool = new List<GameObject>();
        for (int i = 0; i < ProjectilePoolCount; i++)
        {
            var newBullet = Instantiate(BulletPrefab);
            newBullet.transform.parent = ProjectileContainer.transform;
            newBullet.SetActive(false);
            _projectilePool.Add(newBullet);
        }

        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {
        // If the user is pressing the left mouse button, repeatedly invoke the projectile firing method.
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsInvoking("FireBullet"))
            {
                InvokeRepeating("FireBullet", 0f, LaunchRate);
            }
        }

        // Once the left mouse button is lifted up, stop firing projectiles.
        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke("FireBullet");
        }

        // Remove upgrade once passed time limit.
        _currentUpgradeTime += Time.deltaTime;
        if (_currentUpgradeTime > UpgradeTime && IsUpgraded)
        {
            IsUpgraded = false;
        }
    }

    /// <summary>
    /// Fire a single projectile and play sound.
    /// </summary>
    private void FireBullet()
    {
        // Create new bullets if the pool is empty. If not, dequeue and reactivate it.
        var bullet = GetFreeProjectileWithRigidbody();
        bullet.velocity = transform.parent.forward * LaunchSpeed;

        // If upgraded, fire another two projectiles.
        if (IsUpgraded)
        {
            var bullet2 = GetFreeProjectileWithRigidbody();
            bullet2.velocity = (transform.right + transform.forward / 0.5f) * 100;
            var bullet3 = GetFreeProjectileWithRigidbody();
            bullet3.velocity = (transform.right * -1 + transform.forward / 0.5f) * 100;

            _audioSource.PlayOneShot(SoundManager.Instance.UpgradedGunFire);
        }
        else
        {
            _audioSource.PlayOneShot(SoundManager.Instance.GunFire);
        }
    }

    /// <summary>
    /// Return a projectile's Rigidbody from the project pool, but with its position set. 
    /// </summary>
    /// <returns></returns>
    private Rigidbody GetFreeProjectileWithRigidbody()
    {
        var bullet = GetProjectileFromPool();
        bullet.transform.position = LaunchPosition.position;
        return bullet.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Retrieve a projectile from the projectile pool. This method only returns a raw projectile.
    /// </summary>
    /// <returns>A projectile from the projectile pool</returns>
    private GameObject GetProjectileFromPool()
    {
        // If there is an available bullet in the pool, reuse it. Otherwise, create a new one.
        for (int i = 0; i < ProjectilePoolCount; i++)
        {
            if (!_projectilePool[i].activeInHierarchy)
            {
                var bullet = _projectilePool[i];
                bullet.SetActive(true);
                return bullet;
            }
        }

        var newBullet = Instantiate(BulletPrefab);
        newBullet.transform.parent = ProjectileContainer.transform;
        _projectilePool.Add(newBullet);
        ProjectilePoolCount += 1;
        return newBullet;
    }

    /// <summary>
    /// Upgrade the gun.
    /// </summary>
    public void UpgradeGun()
    {
        IsUpgraded = true;
        _currentUpgradeTime = 0;
    }
}