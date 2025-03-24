using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchState : EnvironmentInteractionState
{   
    float m_elapsedTime = 0;
    float m_resetThreshold = (float)0.5;

    public TouchState(EnvironmentInteractionContext context,EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate)
        :base(context,estate)
    {

    }

    public override void EnterState(){
        m_elapsedTime = 0;
        Debug.Log("Enter Touch");
    }

    public override void ExitState(){}

    
    public override void UpdateState(){
        m_elapsedTime += Time.deltaTime;
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {   
        if(m_elapsedTime > m_resetThreshold || CheckShouldReset()){
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
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
