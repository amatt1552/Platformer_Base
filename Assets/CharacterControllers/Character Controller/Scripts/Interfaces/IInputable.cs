using UnityEngine;

public interface IInputable 
{
    Vector2 GetAxisHorizontal();
    Vector2 GetAxisVertical();
    bool JumpPressed();
    bool JumpReleased();

}
