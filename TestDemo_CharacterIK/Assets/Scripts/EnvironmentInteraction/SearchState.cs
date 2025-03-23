using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : EnvironmentInteractionState
{   
    private float m_approachDistanceThreshold = (float)2;
    private Collider m_curCollider;

    public SearchState(EnvironmentInteractionContext context,EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate)
        :base(context,estate)
    {

    }

    public override void EnterState(){
        Debug.Log("Search Enter");
    }

    public override void ExitState(){
        m_curCollider = null;
    }

    public override void UpdateState(){

    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {   
        if(CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        } 
        
        bool isCloseToTarget = Vector3.Distance(m_Context.CloestPointOnColliderFromShoulder , m_Context.RootTransform.position)
                                < m_approachDistanceThreshold;

        bool isCloestPointOnColliderValid = m_Context.CloestPointOnColliderFromShoulder != Vector3.positiveInfinity;     
        if(isCloseToTarget && isCloestPointOnColliderValid)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
        }  
        return StateKey;
    }

    public override void OnTriggerEnter(Collider other)
    {
        m_curCollider = other;
        StartIKTargetPositionTracking(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        ResetIKTargetPositionTracking(other);
    }

    public override void OnTriggerStay(Collider other){

        //针对检测同一块大范围碰撞区域
        if(m_curCollider == null)
            OnTriggerEnter(other); 
            
        UpdateIKTargetPosition(other);
        
    }
}
