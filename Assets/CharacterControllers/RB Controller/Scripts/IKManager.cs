using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKManager : MonoBehaviour
{
    //TODO: Make a way to find ik without being public
    public Transform leftShoulder;
    public Transform rightShoulder;
    public Transform centerShoulder;
    public Rig handFeetRig;
    public TwoBoneIKConstraint IKLeftHand;
    public TwoBoneIKConstraint IKRightHand;
    public TwoBoneIKConstraint IKLeftFoot;
    public TwoBoneIKConstraint IKRightFoot;

    public Transform baseTargetLeftHand;
    public Transform baseTargetRightHand;

    private List<IKData> ikData;
    private const int _NUM_OF_TARGETS = 4;
    private const float _DEFAULT_TIME = 0.2f;

    private void Awake()
    {
        //TODO: Make this automatic instead of using constant
        ikData = new List<IKData>();
        for (int i = 0; i < _NUM_OF_TARGETS; i++) 
        {
            ikData.Add(new IKData());
        }

    }

    public void LeftHandTarget(Vector3 targetPosition, Vector3 targetNormal, float targetWeight, float time = _DEFAULT_TIME) 
    {
        IKLeftHand = SetTarget(0, IKLeftHand, baseTargetLeftHand.position, targetPosition, targetNormal, targetWeight, time); 
    }
    public void RightHandTarget(Vector3 targetPosition, Vector3 targetNormal, float targetWeight, float time = _DEFAULT_TIME)
    {
        IKRightHand = SetTarget(1, IKRightHand, baseTargetRightHand.position, targetPosition, targetNormal, targetWeight, time);
    }
    public void LeftFootTarget(Vector3 targetPosition, Vector3 targetNormal, float targetWeight, float time = _DEFAULT_TIME)
    {
        IKLeftFoot = SetTarget(2, IKLeftFoot, Vector3.zero, targetPosition, targetNormal, targetWeight, time);
    }
    public void RightFootTarget(Vector3 targetPosition, Vector3 targetNormal, float targetWeight, float time = _DEFAULT_TIME)
    {
        IKRightFoot = SetTarget(3, IKRightFoot, Vector3.zero, targetPosition, targetNormal, targetWeight, time);
    }

    TwoBoneIKConstraint SetTarget(int weightIndex, TwoBoneIKConstraint targetIK, Vector3 baseTargetPosition, Vector3 targetPosition, Vector3 targetNormal, float targetWeight, float time) 
    {
        // Used for smoother transitions.
        time = time == 0? 0.01f : time;
        float timeScale = 1;
        float currentWeight = ikData[weightIndex].currentWeight;

        if (currentWeight > targetWeight)
        {
            currentWeight -= timeScale / time * Time.deltaTime;
            // Used to prevent weight from getting past the target
            if (currentWeight < targetWeight)
            {
                currentWeight = targetWeight;
            }
        }
        else if(currentWeight < targetWeight) 
        {
            currentWeight += timeScale / time * Time.deltaTime;
            // Used to prevent weight from getting past the target
            if (currentWeight > targetWeight)
            {
                currentWeight = targetWeight;
            }
        }

        //saves new data to array.
        ikData[weightIndex].currentWeight = currentWeight;
        //allows saving previous position and normal
        if (targetWeight > 0)
        {
            ikData[weightIndex].SetTarget(targetPosition, targetNormal);
        }
        else 
        {
            ikData[weightIndex].SetTarget(baseTargetPosition, Vector3.zero);
        }

        // Sets the values for ik
        targetIK.weight = currentWeight;
        targetIK.data.target.position = Vector3.MoveTowards(targetIK.data.target.position, ikData[weightIndex].GetTarget(), 2 * Time.deltaTime);
        targetIK.data.target.rotation = Quaternion.LookRotation(-ikData[weightIndex].GetTargetNormal());
        return targetIK;
    }
    
    class IKData 
    {
        public float currentWeight;
        private Vector3 target;
        private Vector3 targetNormal;
        public IKData() 
        {
            target = Vector3.one;
            targetNormal = Vector3.one;
        }
        public Vector3 GetTargetNormal()
        {
            return targetNormal;
        }
        public Vector3 GetTarget()
        {
            return target;
        }

        public void SetTarget(Vector3 newTarget, Vector3 newNormal)
        {
            target = newTarget == Vector3.zero ? target : newTarget;
            targetNormal = newNormal == Vector3.zero ? targetNormal : newNormal;
            
        }
    }
}
