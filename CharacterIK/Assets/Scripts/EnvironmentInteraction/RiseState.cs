using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RiseState : EnvironmentInteractionState
{
    float m_elapsedTime = 0;
    float m_lerpDuration = 5;
    float m_riseWeight = 1;
    float m_maxDistance = (float)0.5;
    float m_rotationSpeed = 1000;
    float m_touchDistanceThreshold = (float)0.05;
    float m_touchTimeThreshold = 1;

    protected LayerMask m_InteractableLayerMask = LayerMask.GetMask("Interactable");
    Quaternion m_exceptedHandleRotation;
    float m_interpolationTemp;

    public RiseState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate)
        : base(context, estate)
    {

    }

    public override void EnterState()
    {
        m_elapsedTime = 0;
        Debug.Log("Rise Enter");
    }

    public override void ExitState()
    {

    }


    public override void UpdateState()
    {
        //if (m_Context.CloestPointOnColliderFromShoulder.y == Mathf.Infinity) return;

        CalculateExprectedHandleRotation();

        
        m_interpolationTemp = m_elapsedTime / m_lerpDuration;

        m_Context.InteractionPointYOffset = Mathf.Lerp(m_Context.InteractionPointYOffset
                    ,m_Context.CloestPointOnColliderFromShoulder.y,m_interpolationTemp);

        m_Context.CurrentIKConstraint.weight = Mathf.Lerp(m_Context.CurrentIKConstraint.weight,m_riseWeight,m_interpolationTemp);
    
        m_Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(m_Context.CurrentMultiRotationConstraint.weight,m_riseWeight,m_interpolationTemp);


        m_Context.CurrentIKTargetTransform.rotation = Quaternion.RotateTowards(m_Context.CurrentIKTargetTransform.rotation
           , m_exceptedHandleRotation, m_rotationSpeed * Time.deltaTime);
        
        m_elapsedTime += Time.deltaTime;

    }

    RaycastHit hit;
    Vector3 targetForward;
    private void CalculateExprectedHandleRotation()
    {

        Vector3 startPos = m_Context.RealPositionFromShoulderPoint;
        Vector3 endPos = m_Context.CloestPointOnColliderFromShoulder;
        Vector3 dir = (endPos - startPos).normalized;


        if (Physics.Raycast(startPos, dir, out hit, m_maxDistance, m_InteractableLayerMask))
        {
            targetForward = -hit.normal;
            m_exceptedHandleRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        }

    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {

        if (CheckShouldReset() || Vector3.Distance(m_Context.CurrentIKTargetTransform.position, m_Context.CloestPointOnColliderFromShoulder) < m_touchDistanceThreshold
             && m_elapsedTime >= m_touchTimeThreshold)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Touch;
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
