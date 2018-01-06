using UnityEngine;

namespace Assets.Scripts
{
    public class Rotator : MonoBehaviour
    {
        public int RotationSpeed;

        private void Update()
        {
            transform.Rotate(new Vector3(0, RotationSpeed, 0) * Time.deltaTime);
        }
    }
}