using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState,BaseState<EState>> m_States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> m_CurrentState;
    protected EState m_NextStateKey;
    protected bool m_IsTransitioningState = false;
    public BaseState<EState> CurrentState { get {return m_CurrentState;}}

    
    void Start(){
        m_CurrentState.EnterState();
    }
    private void Update() {   
        m_NextStateKey = m_CurrentState.GetNextState();
        if(!m_IsTransitioningState && m_NextStateKey.Equals(m_CurrentState.StateKey)){
            m_CurrentState.UpdateState();
        }
        else if(!m_IsTransitioningState)
        {
            TransitionToNextState(m_NextStateKey);
        }
    }

    private void TransitionToNextState(EState nextKey){
        m_IsTransitioningState = true;
        m_CurrentState.ExitState();
        m_CurrentState = m_States[nextKey];
        m_CurrentState.EnterState();
        m_IsTransitioningState = false;
    }
    

    void OnTriggerEnter(Collider other){  
        m_CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other){
        m_CurrentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other){
        m_CurrentState.OnTriggerExit(other);
    }
}
