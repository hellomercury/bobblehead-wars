using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public float ShakeDecay = 0.002f;
    public float Intensity = .3f;

    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private float _shakeIntensity;

    private void Update()
    {
        if (_shakeIntensity > 0)
        {
            transform.position = _originPosition + Random.insideUnitSphere * _shakeIntensity;
            transform.rotation = new Quaternion(
                _originRotation.x + Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.y + Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.z + Random.Range(-_shakeIntensity, _shakeIntensity) * .2f,
                _originRotation.w + Random.Range(-_shakeIntensity, _shakeIntensity) * .2f);
            _shakeIntensity -= ShakeDecay;
        }
    }

    public void Shake()
    {
        _originPosition = transform.position;
        _originRotation = transform.rotation;
        _shakeIntensity = Intensity;
    }
}