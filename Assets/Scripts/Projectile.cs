using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DeactivateProjectile();
    }

    private void OnCollisionEnter(Collision other)
    {
        DeactivateProjectile();
    }

    private void OnBecameInvisible()
    {
        DeactivateProjectile();
    }

    private void OnEnable()
    {
        Invoke("DeactivateProjectile", 2f);
    }

    private void OnDisable()
    {
        if (IsInvoking("DeactivateProjectile"))
        {
            CancelInvoke("DeactivateProjectile");
        }
    }

    private void DeactivateProjectile()
    {
        gameObject.SetActive(false);
    }
}