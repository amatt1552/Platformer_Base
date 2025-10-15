using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviour
{
    CharacterActions actions;
    public InputAction move;
    public InputAction sprint;
    public InputAction walk;
    public InputAction fire;
    public InputAction interact;
    public InputAction jump;

    public InputAction look;
    bool fireCharged;

    private void Awake()
    {
        actions = new CharacterActions(); 
        Debug.Log("Enabling Player Input..");
        move = actions.Player.Move;
        move.Enable();

        sprint = actions.Player.Sprint;
        sprint.Enable();
        //sprint.performed += Sprint;
        //sprint.canceled += EndSprint;

        walk = actions.Player.Walk;
        walk.Enable();
        //walk.performed += ToggleWalk;

        look = actions.Player.Look;
        look.Enable();

        //fire = actions.Player.Fire;
        //fire.Enable();
        //fire.performed += FireCharged;
        //fire.canceled += Fire;

        //interact = actions.Player.Interact;
        //interact.Enable();
        //interact.performed += Interact;


        jump = actions.Player.Jump;
        jump.Enable();
        //jump.performed += Jump;
    }
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        Debug.Log("Disabling Player Input..");
        move.Disable();
        sprint.Disable();
        walk.Disable();
        //fire.Disable();
        //interact.Disable();
        jump.Disable();
        look.Disable();
    }
    
    //void Update()
    //{
    //    Vector2 axis = move.ReadValue<Vector2>();
    //    characterController.MoveXZ(axis);
    //    Vector2 lookValue = look.ReadValue<Vector2>();
    //    characterController.SetRotation(lookValue);
    //}
    ////private void FireCharged(InputAction.CallbackContext context)
    ////{
    ////    fireCharged = true;
    ////}
    ////private void Fire(InputAction.CallbackContext context) 
    ////{
    ////    if (fireCharged)
    ////    {
    ////        playerMovement.FireCharged();
    ////    }
    ////    else
    ////    {
    ////        playerMovement.Fire();
    ////    }
    ////    fireCharged = false;
    ////}
    ////private void Interact(InputAction.CallbackContext context) 
    ////{
    ////    playerMovement.TryInteract();
    ////}
    //private void Sprint(InputAction.CallbackContext context)
    //{
    //    characterController.SetRunning();
    //}
    //private void EndSprint(InputAction.CallbackContext context) 
    //{
    //    characterController.SetJogging();
    //}

    //private void ToggleWalk(InputAction.CallbackContext context) 
    //{
    //    characterController.ToggleWalking();
    //}
    //private void Jump(InputAction.CallbackContext context) 
    //{
    //    characterController.Jump();
    //}
}
