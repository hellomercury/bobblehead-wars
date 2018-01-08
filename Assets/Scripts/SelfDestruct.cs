using UnityEngine;
using UnityEngine.Events;

public class OnDestroyAlienHeadEvent : UnityEvent<GameObject>
{
}

public class SelfDestruct : MonoBehaviour
{
    public float DestructTime = 3.0f;

    public OnDestroyAlienHeadEvent OnDestroyAlienHeadEvent;

    private void Awake()
    {
        OnDestroyAlienHeadEvent = new OnDestroyAlienHeadEvent();
    }

    public void Initiate()
    {
        Invoke("SelfDestroyed", DestructTime);
    }

    private void SelfDestroyed()
    {
        OnDestroyAlienHeadEvent.Invoke(gameObject);
    }
}