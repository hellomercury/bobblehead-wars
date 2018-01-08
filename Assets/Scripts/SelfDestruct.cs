using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float DestructTime = 3.0f;

    public void Initiate()
    {
        Invoke("SelfDestroyed", DestructTime);
    }

    private void SelfDestroyed()
    {
        gameObject.SetActive(false);
    }
}