using UnityEngine;

public interface IMoveable 
{
    void MoveObject();
    void ThrowObject(float force, Vector3 direction);
    void PushObject(float force, Vector2 direction);
}
