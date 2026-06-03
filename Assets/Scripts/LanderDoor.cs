using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LanderDoor : InteractableObject
{
    [SerializeField]
    private Airlock _airlock;
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
       if (!_isSelected)
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
        _onCooldown = true;
        if (_player.IsInside)
        {
            _player.Outfit = Astronaut.OutfitOption.SPACESUIT;
        }
        // 2. depressurize
        yield return new WaitForSeconds(0.5f);
        _airlock.Trigger();
        // 3. open door
        yield return new WaitForSeconds(1.5f);
        _animator.SetTrigger("Open"); 
        // wait for door to finish closing before pressurizing
        yield return new WaitUntil(() => _doorCloseAnimationFinishFlag);
        _doorCloseAnimationFinishFlag = false;
        // 4. pressurize
        yield return new WaitForSeconds(0.5f);
        _airlock.Trigger();
        yield return new WaitForSeconds(1.5f);
        if (_player.IsInside)
        {
            _player.Outfit = Astronaut.OutfitOption.BLUE_JUMPSUIT;
        }
        _onCooldown = false;

    }
}
