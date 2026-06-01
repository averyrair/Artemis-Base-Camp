using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LanderDoor : InteractableObject
{
    [SerializeField]
    private List<ParticleSystem> _steamVents;
    private Animator _animator;
    private Astronaut _player;
    
    private bool _doorCloseAnimationFinishFlag;

    protected override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _player = GameObject.FindWithTag("Player").GetComponent<Astronaut>();
        _doorCloseAnimationFinishFlag = false;
    }

    protected override void OnInteract(InputAction.CallbackContext context)
    {
       if (!_isHovered)
        {
            return;
        }

        StartCoroutine(OpenDoor());

    }

    public void OnDoorCloseAnimationFinish()
    {
            Debug.Log("Door Finished Closing");
            _doorCloseAnimationFinishFlag = true;
    }

    private IEnumerator OpenDoor()
    {
        if (_player.IsInside)
        {
            // 1. put on suit
            _player.Outfit = Astronaut.OutfitOption.SPACESUIT;
            // 2. depressurize
            yield return new WaitForSeconds(0.5f);
            Pressurize();
            // 3. open door
            yield return new WaitForSeconds(3.5f);
            _animator.SetTrigger("Open"); 
            // wait for door to finish closing before pressurizing
            yield return new WaitUntil(() => _doorCloseAnimationFinishFlag);
            _doorCloseAnimationFinishFlag = false;
            // 4. pressurize
            yield return new WaitForSeconds(0.5f);
            Pressurize();
        }
        else
        {
            // 1. depressurize
            yield return new WaitForSeconds(0.5f);
            Pressurize();
            // 2. open door
            yield return new WaitForSeconds(3.5f);
            _animator.SetTrigger("Open");
            // wait for door to finish closing before pressurizing
            yield return new WaitUntil(() => _doorCloseAnimationFinishFlag);
            _doorCloseAnimationFinishFlag = false;
            // 3. pressurize
            yield return new WaitForSeconds(0.5f);
            Pressurize();
            // 4. remove suit
            yield return new WaitForSeconds(3.5f);
            _player.Outfit = Astronaut.OutfitOption.BLUE_JUMPSUIT;
            
        }

    }

    private void Pressurize()
    {
        foreach (var vent in _steamVents)
        {
            vent.Play();
        }
    }
}
