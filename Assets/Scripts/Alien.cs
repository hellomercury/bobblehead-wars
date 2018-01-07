using UnityEngine;
using UnityEngine.AI;

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
    /// GameManager script attached to the GameManager object.
    /// </summary>
    [HideInInspector] public GameManager GameManagerScript;

    /// <summary>
    /// The amount of time, in milliseconds, for when the alien should update its path.
    /// </summary>
    public float NavigationUpdate = 0.5f;

    /// <summary>
    /// Tracks how much time has passed since the previous update.
    /// </summary>
    private float _navigationTime = 0;

    /// <summary>
    /// Component attached to this game object.
    /// </summary>
    private NavMeshAgent _agent;

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
        GameManagerScript.DisableAlien(Index);
    }
}