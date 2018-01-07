using UnityEngine;

public class ArenaWall : MonoBehaviour
{
    private Animator _arenaAnimator;

    // Use this for initialization
    void Start()
    {
        _arenaAnimator = transform.parent.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _arenaAnimator.SetBool("IsLowered", true);
    }

    private void OnTriggerExit(Collider other)
    {
        _arenaAnimator.SetBool("IsLowered", false);
    }
}