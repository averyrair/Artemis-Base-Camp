using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _smoothTime;
    [SerializeField]
    private Vector3 _offset;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 targetPosition = _target.position + _offset;
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime); 
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

}
