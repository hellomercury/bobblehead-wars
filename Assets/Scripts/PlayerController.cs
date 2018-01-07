using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Movement speed of the marine.
    /// </summary>
    public float MoveSpeed = 50f;

    /// <summary>
    /// The head of marine to be applied with force.
    /// </summary>
    public Rigidbody MarineHead;

    /// <summary>
    /// Layer mask to be hit when raycasting.
    /// </summary>
    public LayerMask LayerMask;

    /// <summary>
    /// Toggle to display raycast.
    /// </summary>
    public bool DisplayRay;

    /// <summary>
    /// How fast the marine should turn.
    /// </summary>
    public float TurnSpeed = 10.0f;

    /// <summary>
    /// Animator for the body of the marine.
    /// </summary>
    public Animator BodyAnimator;

    /// <summary>
    /// Intersection when raycasting to be used to turn the marine.
    /// </summary>
    private Vector3 _currentLookTarget = Vector3.zero;

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

    private void FixedUpdate()
    {
        // Apply force to the head to bounce it around.
        MarineHead.AddForce(transform.right * 150, ForceMode.Acceleration);

        // Animate if walking.
        var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        BodyAnimator.SetBool("IsMoving", moveDirection != Vector3.zero);

        // Create a ray from the main camera to the mouse position.
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Cast the ray to find intersection between the ray and the designated layer.
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 2000, LayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.point != _currentLookTarget)
            {
                _currentLookTarget = hit.point;
            }
        }

        // Generate the position that the marine should look at and then turn the marine to that position.
        var targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        var rotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * TurnSpeed);

        // If enabled, display the ray in the view port.
        if (DisplayRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.green);
        }
    }
}