using UnityEngine;

public class Forces
{

    private Vector3 _addForceDir;
    private Vector3 _startAddForceDir;
    private Rigidbody _rb;
    public Forces(Rigidbody rb) 
    {
        _rb = rb;
    }

    /// <summary>
    /// Simulates using AddForce from physics.
    /// </summary>
    /// <param name="force"></param>
    /// <param name="direction"></param>
    public void AddForce(float force, Vector3 direction)
    {
        force = Mathf.Clamp(force, 0.1f, Mathf.Infinity) / _rb.mass;
        _addForceDir += direction * force;
        _startAddForceDir = _addForceDir;
    }

    public void LimitForce(Vector3 limits) 
    {
        _addForceDir.x = Mathf.Abs(_addForceDir.x) < limits.x ? _addForceDir.x : limits.x;
        _addForceDir.y = Mathf.Abs(_addForceDir.y) < limits.y ? _addForceDir.y : limits.y;
        _addForceDir.z = Mathf.Abs(_addForceDir.z) < limits.z ? _addForceDir.z : limits.z;
    }

    public void UpdateForce()
    {
        //update xz value based on air resistance and ground friction
        float xzDeceleration = 2;
        xzDeceleration = Mathf.Clamp(xzDeceleration, 0.1f, Mathf.Infinity);

        //make force in x approach 0
        _addForceDir.x.ApproachValue(_startAddForceDir.x, xzDeceleration);
        //make force in z approach 0 
        _addForceDir.z.ApproachValue(_startAddForceDir.z, xzDeceleration);


        //update y value based on gravity
        float yDeceleration = GameSettings.GRAVITY;
        yDeceleration = Mathf.Clamp(yDeceleration, 0.1f, Mathf.Infinity);

        //Make force in y approach 0
        _addForceDir.y.ApproachValue(_startAddForceDir.y, yDeceleration);
    }

}
