using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathParticles : MonoBehaviour
{
    private ParticleSystem _deathParticles;

    void Start()
    {
        _deathParticles = GetComponent<ParticleSystem>();
    }

    public void Activate()
    {
        _deathParticles.Play();
    }

    public void SetDeathFloor(GameObject deathFloor)
    {
        if (_deathParticles == null)
        {
            _deathParticles = GetComponent<ParticleSystem>();
        }

        _deathParticles.collision.SetPlane(0, deathFloor.transform);
    }
}