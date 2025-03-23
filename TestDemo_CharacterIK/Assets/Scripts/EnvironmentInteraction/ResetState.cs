using System;
using Unity.VisualScripting;
using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    float zero = 0;
    float m_elapsedTime = 0;
    float m_resetDuration = 2;
    float m_lerpDuration = 10;
    float m_rotationSpeed = 500;
    float m_interpolationTemp = 0;

    public ResetState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate)
        : base(context, estate)
    {

    }

    Quaternion expectedGroundRotation;

    public override void EnterState()
    {
        Debug.Log("Reset Enter");
        m_elapsedTime = 0;
        m_Context.CloestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        m_Context.CurrentInersectingCollider = null;    
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {   
        m_elapsedTime += Time.deltaTime;
        m_interpolationTemp = m_elapsedTime / m_lerpDuration;

        m_Context.InteractionPointYOffset = Mathf.Lerp(m_Context.InteractionPointYOffset,
            m_Context.ColliderCenterY, m_interpolationTemp);
        
        m_Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(m_Context.CurrentMultiRotationConstraint.weight,zero, m_interpolationTemp);

        m_Context.CurrentIKConstraint.weight = Mathf.Lerp(m_Context.CurrentIKConstraint.weight, zero, m_interpolationTemp);

        m_Context.CurrentIKTargetTransform.localPosition = Vector3.Lerp(m_Context.CurrentIKTargetTransform.localPosition, 
            m_Context.CurrentOrignalTargetPosition, m_interpolationTemp); 

        m_Context.CurrentIKTargetTransform.rotation = Quaternion.RotateTowards(m_Context.CurrentIKTargetTransform.rotation,
                m_Context.CurrentOrignalTargetRotation, m_rotationSpeed * Time.deltaTime);  //

    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {   
        //移动2秒，进入搜索状态
        bool isMoving = m_Context.Character.velocity != Vector3.zero;
        if (m_elapsedTime >= m_resetDuration && isMoving)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        }    

        return StateKey;
        
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerExit(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

}
