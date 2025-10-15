using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    enum PlayerState
    {
        Falling,
        Jumping
    }
    PlayerState playerState;

    public CharacterController controller;
    public Transform cam;
    [SerializeField]
    private Transform _playerBottom;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    public float jumpSpeed = 6f;
    public float mass = 1;
    const float _GRAVITY = 9.8f;
    public float gravMultiplier = 1f;
    float _moveValueY;
    float _turnSmoothVelocity;

    public void Awake()
    {
        controller.enableOverlapRecovery = false;
    }


    public void Move(Vector2 value) 
    {
        Vector3 axis = new Vector3(value.x, 0, value.y).normalized;
        Vector3 direction = Vector3.zero;

        if (axis.magnitude > 0.1f) 
        {
            float targetAngle = Mathf.Atan2(axis.x, axis.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            direction = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed;
        }
        StateMachine();
        direction.y += _moveValueY;
        controller.Move(direction * Time.deltaTime);
    }

    void StateMachine() 
    {
        switch (playerState) 
        {
            case PlayerState.Falling:
                Gravity();
                break;
            case PlayerState.Jumping:
                playerState = PlayerState.Falling;
                break;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        //prevents character controller from getting stuck inside walls and such.
        if(other != null && other.tag != "Player" && Grounded()) 
        {
            Vector3 direction = other.transform.position - transform.position;
            Slide(direction);
        }
    }


    void Slide(Vector3 direction) 
    {
        
        direction.Normalize();
        controller.Move(-direction * (speed + 1) * Time.deltaTime);
        Debug.Log("Character is stuck in something!");
    }

    public void Gravity()
    {

        //F = (G * m1 * m2) / d^2 might be useful for orbiting or something
        //Ray ray = new Ray(transform.position, Vector3.down);
        //RaycastHit hit;
        //float distance = 1;
        //if(controller.Raycast(ray, out hit, Mathf.Infinity)) 
        //{
        //    distance = hit.distance;
        //}
        _moveValueY += -_GRAVITY  * mass * Time.deltaTime * gravMultiplier;
        if (Grounded() && !EdgeTest())
        {
            Debug.Log("on an edge!");
        }
        if (Grounded())
        {
            _moveValueY = -1;
        }

    }

    public void Jump() 
    {
        if (Grounded()) 
        {
            playerState = PlayerState.Jumping;
            _moveValueY = jumpSpeed * (mass * 0.5f);

        }
    }

    public bool Grounded() 
    {
        return controller.isGrounded;
    }

    public bool EdgeTest() 
    {
        Ray ray = new Ray(_playerBottom.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) 
        {
            return hit.distance <= controller.skinWidth + 0.1f; 
        }
        return false;
    }


    public bool TouchingWalls() 
    {
        return (controller.collisionFlags & CollisionFlags.Sides) != 0; 
    }
}
