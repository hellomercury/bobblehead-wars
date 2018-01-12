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
    /// Force values for the camera.
    /// </summary>
    public float[] HitForce;

    /// <summary>
    /// How long before the marine can get hit again.
    /// </summary>
    public float HitInterval = 2.5f;

    /// <summary>
    /// Animator for the body of the marine.
    /// </summary>
    public Animator BodyAnimator;

    /// <summary>
    /// Marine body.
    /// </summary>
    public Rigidbody MarineBody;

    /// <summary>
    /// Whether the marine is now dead.
    /// </summary>
    private bool _isDead = false;

    /// <summary>
    /// Intersection when raycasting to be used to turn the marine.
    /// </summary>
    private Vector3 _currentLookTarget = Vector3.zero;

    /// <summary>
    /// Character Controller component of the game object.
    /// </summary>
    private CharacterController _characterController;

    /// <summary>
    /// Whether the marine is hit.
    /// </summary>
    private bool _isHit;

    /// <summary>
    /// How long it is since the last hit.
    /// </summary>
    private float _timeSinceHit;

    /// <summary>
    /// How many times the marine has been hit.
    /// </summary>
    private int _hitNumber = -1;

    /// <summary>
    /// Death particle script attached to the marine death particle game object.
    /// </summary>
    private DeathParticles _deathParticles;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _deathParticles = gameObject.GetComponentInChildren<DeathParticles>();
    }

    // Update is called once per frame
    private void Update()
    {
        var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _characterController.SimpleMove(moveDirection * MoveSpeed);

        if (_isHit)
        {
            _timeSinceHit += Time.deltaTime;
            if (_timeSinceHit > HitInterval)
            {
                _isHit = false;
                _timeSinceHit = 0;
            }
        }
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

    private void OnTriggerEnter(Collider other)
    {
        var alien = other.gameObject.GetComponent<Alien>();

        if (alien != null)
        {
            if (!_isHit)
            {
                _hitNumber += 1;
                var cameraShake = Camera.main.GetComponent<CameraShake>();
                if (_hitNumber < HitForce.Length)
                {
                    cameraShake.Intensity = HitForce[_hitNumber];
                    cameraShake.Shake();
                }
                else
                {
                    Die();
                }

                _isHit = true;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.Hurt);
            }
        }
    }

    public void Die()
    {
        BodyAnimator.SetBool("IsMoving", false);
        MarineBody.transform.parent = null;
        MarineBody.isKinematic = false;
        MarineBody.useGravity = true;
        MarineBody.gameObject.GetComponent<CapsuleCollider>().enabled = true;
        MarineBody.gameObject.GetComponent<Gun>().enabled = false;

        Destroy(MarineHead.gameObject.GetComponent<HingeJoint>());
        MarineHead.transform.parent = null;
        MarineHead.useGravity = true;
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.MarineDeath);
        _deathParticles.Activate();
        Destroy(gameObject);
    }
}