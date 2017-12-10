using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    /// <summary>
    /// Target that the camera will follow. Must not be null.
    /// </summary>
    public GameObject FollowTarget;

    /// <summary>
    /// The movement speed of the camera.
    /// </summary>
    public float MoveSpeed;

    // Update is called once per frame
    private void Update()
    {
        if (FollowTarget != null)
        {
            // Calculate the new position for the camera. 
            var startPoint = transform.position;
            var endPoint = FollowTarget.transform.position;
            var intermediatePoint = Time.deltaTime * MoveSpeed;
            transform.position = Vector3.Lerp(startPoint, endPoint, intermediatePoint);
        }
    }
}