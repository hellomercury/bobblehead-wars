using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class OnAlienDestroyedEvent : UnityEvent<int>
{
}

public class Alien : MonoBehaviour
{
    /// <summary>
    /// Index of this alien in the pool.
    /// </summary>
    [HideInInspector] public int Index;

    /// <summary>
    /// Target to be followed by this game object.
    /// </summary>
    [HideInInspector] public Transform Target;

    /// <summary>
    /// The amount of time, in milliseconds, for when the alien should update its path.
    /// </summary>
    public float NavigationUpdate = 0.5f;

    /// <summary>
    /// Event to send when the alien is destroyed.
    /// </summary>
    public OnAlienDestroyedEvent OnDestroyEvent;

    /// <summary>
    /// Tracks how much time has passed since the previous update.
    /// </summary>
    private float _navigationTime;

    /// <summary>
    /// Component attached to this game object.
    /// </summary>
    private NavMeshAgent _agent;

    private void Awake()
    {
        OnDestroyEvent = new OnAlienDestroyedEvent();
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _navigationTime += Time.deltaTime;
        if (_navigationTime > NavigationUpdate)
        {
            if (Target != null)
            {
                _agent.destination = Target.position;
            }

            NavigationUpdate = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnDestroyEvent.Invoke(Index);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.AlienDeath);
    }
}