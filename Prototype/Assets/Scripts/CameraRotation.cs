using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField]
    private float _mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _distanceFromTarget = 3.0f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    private Vector3 nextRotation;

    [SerializeField]
    private float _smoothTime = 0.2f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(-40, 85);

    [SerializeField]
    private Vector2 _zoomMinMax = new Vector2(1,30);

    [SerializeField]
    private float _zoomSpeed = 5f;

    private void Start()
    {
        _currentRotation = transform.rotation.eulerAngles;
        transform.eulerAngles = _currentRotation;
    }

    void Update()
    {
        

        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

            _rotationY += mouseX;
            _rotationX += mouseY;

            // Apply clamping for x rotation 
            _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

            nextRotation = new Vector3(_rotationX, _rotationY);

            // Apply damping between rotation changes

            _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
            transform.localEulerAngles = _currentRotation;
            //TODO: Add Clipping Logic
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            
            //TODO: SetMaxZoom In / Out Distance
            if(_distanceFromTarget >= _zoomMinMax.x && _distanceFromTarget <= _zoomMinMax.y)
            {
                _distanceFromTarget += scroll * _zoomSpeed;
            }
            if(_distanceFromTarget < _zoomMinMax.x)
            {
                _distanceFromTarget = _zoomMinMax.x;
            }
            if (_distanceFromTarget > _zoomMinMax.y)
            {
                _distanceFromTarget = _zoomMinMax.y;
            }
        }


        // Substract forward vector of the GameObject to point its forward vector to the target
        transform.position = _target.position - transform.forward * _distanceFromTarget;
    }
}
