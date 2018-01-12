using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject Player;
    public Transform Elevator;
    private Animator _arenaAnimator;
    private SphereCollider _sphereCollider;

    // Use this for initialization
    void Start()
    {
        _arenaAnimator = GetComponent<Animator>();
        _sphereCollider = GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        Camera.main.transform.parent.gameObject.GetComponent<CameraMovement>().enabled = false;
        Player.transform.parent = Elevator.transform;
        Player.GetComponent<PlayerController>().enabled = false;

        SoundManager.Instance.PlayOneShot(SoundManager.Instance.ElevatorArrived);
        _arenaAnimator.SetBool("OnElevator", true);
    }

    public void ActivatePlatform()
    {
        _sphereCollider.enabled = true;
    }
}