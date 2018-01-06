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
    /// Pool of projectiles that can be reused.
    /// </summary>
    private List<GameObject> _availablePools;

    private void Start()
    {
        // Populate the projectile pool.
        _availablePools = new List<GameObject>();
        for (int i = 0; i < ProjectilePoolCount; i++)
        {
            var newBullet = Instantiate(BulletPrefab);
            newBullet.SetActive(false);
            _availablePools.Add(newBullet);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // If the user is pressing the left mouse button, repeatedly invoke the bullet firing method.
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsInvoking("FireBullet"))
            {
                InvokeRepeating("FireBullet", 0f, LaunchRate);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke("FireBullet");
        }
    }

    private void FireBullet()
    {
        // Create new bullets if the pool is empty. If not, dequeue and reactivate it.
        var bullet = GetProjectile();
        bullet.transform.position = LaunchPosition.position;
        bullet.GetComponent<Rigidbody>().velocity = transform.parent.forward * LaunchSpeed;
    }

    private GameObject GetProjectile()
    {
        // If there is an available bullet in the pool, reuse it. Otherwise, create a new one.
        for (int i = 0; i < ProjectilePoolCount; i++)
        {
            if (!_availablePools[i].activeInHierarchy)
            {
                var bullet = _availablePools[i];
                bullet.SetActive(true);
                return bullet;
            }
        }

        var newBullet = Instantiate(BulletPrefab);
        _availablePools.Add(newBullet);
        return newBullet;
    }
}