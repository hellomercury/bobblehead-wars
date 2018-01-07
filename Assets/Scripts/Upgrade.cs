using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public Gun Gun;

    /// <summary>
    /// Index of this game object in the pickup pool.
    /// </summary>
    [HideInInspector] public int Index;

    private void OnTriggerEnter(Collider other)
    {
        Gun.UpgradeGun();
        GameManager.Instance.DisablePickup(Index);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.PowerUpPickup);
    }
}