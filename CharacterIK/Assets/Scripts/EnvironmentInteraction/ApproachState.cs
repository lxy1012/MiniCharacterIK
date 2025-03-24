using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    private float m_approachWeight = 0.5f;
    private float m_approachRotationWeight = 0.75f;
    private float m_elapsedTime = 0;
    private float m_lerpDuration = 5;
    private float m_rotationSpeed = 500;
    private float m_riseDistanceThreshold = (float)0.5;  //约等于升高的一半--臂展
    private float m_approachDuration = 2;
    private Quaternion expectedGroundRotation;
    float m_interpolationTemp = 0;


    public ApproachState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate)
        : base(context, estate)
    {
      
    }

    public override void EnterState()
    {
        Debug.Log("Approach Enter");
        m_elapsedTime = 0;
    }

    public override void ExitState() { }

    public override void UpdateState()
    {
        m_elapsedTime += Time.deltaTime;
        m_interpolationTemp = m_elapsedTime / m_lerpDuration;

        expectedGroundRotation = Quaternion.LookRotation(-Vector3.up, m_Context.RootTransform.forward);
        m_Context.CurrentIKTargetTransform.rotation = Quaternion.RotateTowards(m_Context.CurrentIKTargetTransform.rotation,
            expectedGroundRotation, m_rotationSpeed * Time.deltaTime);

        m_Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(m_Context.CurrentMultiRotationConstraint.weight, m_approachRotationWeight, m_interpolationTemp);

        m_Context.CurrentIKConstraint.weight = Mathf.Lerp(m_Context.CurrentIKConstraint.weight, m_approachWeight, m_interpolationTemp);
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {

        bool isOverStateLifeDuration = m_elapsedTime >= m_approachDuration;
        if (isOverStateLifeDuration || CheckShouldReset())
        {
            Debug.Log("Approach 2 Reset");
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        var dis = Vector3.Distance(m_Context.CloestPointOnColliderFromShoulder,
                   m_Context.CurrentShoulderTransform.position);

        bool isWithinArmsReach = dis < m_riseDistanceThreshold;
        bool isClosestPointOnColliderReal = m_Context.CloestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isWithinArmsReach && isClosestPointOnColliderReal)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Rise;
        }
        return StateKey;
    }

    public override void OnTriggerEnter(Collider other)
    {
        StartIKTargetPositionTracking(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        ResetIKTargetPositionTracking(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        UpdateIKTargetPosition(other);
    }


}
