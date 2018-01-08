using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class OnAlienDestroyedEvent : UnityEvent<int, GameObject>
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

    /// <summary>
    /// The alien head.
    /// </summary>
    public Rigidbody Head;

    /// <summary>
    /// Whether the alien is alive.
    /// </summary>
    public bool IsAlive = true;

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
        if (IsAlive)
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAlive)
        {
            Head.gameObject.GetComponent<SelfDestruct>().Initiate();
            OnDestroyEvent.Invoke(Index, gameObject);
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.AlienDeath);
        }
    }
}