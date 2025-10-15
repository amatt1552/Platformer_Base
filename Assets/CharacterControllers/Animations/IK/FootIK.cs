using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootIK : MonoBehaviour
{
    [SerializeField]
    private Transform rightFoot;
    [SerializeField]
    private Transform rightIK;
    private Vector3 defaultRightIK;

    [SerializeField]
    private Transform leftFoot;
    [SerializeField]
    private Transform leftIK;
    private Vector3 defaultLeftIK;

    [SerializeField]
    LayerMask mask;
    [SerializeField]
    bool footRecognizesFloor = true;
    public float distanceToGround;

    private void Awake()
    {
        //defaultRightIK = rightIK.up;
        //defaultLeftIK = leftIK.up;
    }

    TwoBoneIKConstraint constraint;
    private void Update()
    {
        RaycastHit hit;
        if (footRecognizesFloor)
        {
            //animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            //animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

            //animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            Ray ray = new Ray(leftFoot.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, distanceToGround, mask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                leftIK.position = footPosition;
                leftIK.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
            }

            ray = new Ray(rightFoot.position + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, distanceToGround, mask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                rightIK.position = footPosition;
                rightIK.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
            }
        }
    }

    
}
