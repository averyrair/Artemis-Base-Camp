using System.Collections.Generic;
using UnityEngine;

public class Airlock : MonoBehaviour
{
    private ParticleSystem[] _vents;
    private AudioSource _audioSource;


    private void Start()
    {
        _vents = GetComponentsInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Trigger()
    {
        foreach (var vent in _vents)
        {
            vent.Play();
        } 

        _audioSource.Play();
    }
}
