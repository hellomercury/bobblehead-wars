using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Movement speed of the marine.
    /// </summary>
    public float MoveSpeed = 50f;

    /// <summary>
    /// Character Controller component of the game object.
    /// </summary>
    private CharacterController _characterController;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update()
    {
        var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _characterController.SimpleMove(moveDirection * MoveSpeed);
    }
}