using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class BaseState<EState> where EState :Enum
{   

    //TEMP
    protected StringBuilder str = new StringBuilder();
    public string curResetReason {get {return str.ToString();}}
    public Vector3 start;
    public Vector3 end;
    //TEMP END


    private EState m_stateKey;
    public EState StateKey { get{return m_stateKey;}}
    public BaseState(EState stateKey){
        m_stateKey = stateKey;
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerExit(Collider other);
    public abstract void OnTriggerStay(Collider other);
}
