using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;

public class EnvironmentInteractionStateMachine : StateManager<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    public enum EEnvironmentInteractionState
    {
        Search,
        Approach,
        Rise,
        Touch,
        Reset,
    }

    [SerializeField] private TwoBoneIKConstraint m_LeftIKConstraint;
    [SerializeField] private MultiRotationConstraint m_LeftMultiRotationConstraint;

    [SerializeField] private TwoBoneIKConstraint m_RightIKConstraint;
    [SerializeField] private MultiRotationConstraint m_RightMultiRotationConstraint;
    [SerializeField] private CharacterController m_Character;

    private EnvironmentInteractionContext m_Context;
    private void Awake() { 
        ValidateConstraints();
        m_Context = new EnvironmentInteractionContext(
            m_LeftIKConstraint,m_LeftMultiRotationConstraint,m_RightIKConstraint,m_RightMultiRotationConstraint,m_Character,transform.root);
        ConstructEnvironmenDetectionCollider();
        InitializationStates();
    }

/*     private void OnDrawGizmos() {
        
        Gizmos.color = Color.red;
        if(m_Context != null && m_Context.CloestPointOnColliderFromShoulder != Vector3.positiveInfinity){
            Gizmos.DrawSphere(m_Context.CloestPointOnColliderFromShoulder,0.03f);   
        }


        if(m_Context != null )
        Gizmos.DrawLine(m_Context.RealPositionFromShoulderPoint,m_Context.CloestPointOnColliderFromShoulder);
        
        
        if(m_Context != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Context.CurrentIKTargetPosition,0.03f);   
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_Context.CloestPointOnColliderFromShoulder,0.03f);
        }
        
    }

    private GUIStyle lableStyle;
    StringBuilder str = new StringBuilder();
   public void OnGUI()
    {
        if(lableStyle == null){
            lableStyle = new GUIStyle(GUI.skin.label);
            lableStyle.fontSize = 40;
        }

        str.Clear();
        str.Append(m_CurrentState.StateKey.ToString());      
        //str.Append("ik weight:" + m_Context.LeftMultiRotationConstraint.weight + "    "+ m_Context.RightIKConstraint.weight +"\n");
        GUILayout.Label(str.ToString() ,lableStyle);
    } */


    private void ValidateConstraints(){
        Assert.IsNotNull(m_LeftIKConstraint,"Left IK Constraint is not assigned");
        Assert.IsNotNull(m_LeftMultiRotationConstraint,"Left Multi Rotation Constraint is not assigned");
        Assert.IsNotNull(m_RightIKConstraint,"Right IK Constraint is not assigned");
        Assert.IsNotNull(m_RightMultiRotationConstraint,"Right Multi Rotation Constraint is not assigned");
        Assert.IsNotNull(m_Character,"CharacterController is not assigned");
    }

    private void InitializationStates()
    {
        m_States.Add(EEnvironmentInteractionState.Reset,new ResetState(m_Context,EEnvironmentInteractionState.Reset));
        m_States.Add(EEnvironmentInteractionState.Approach,new ApproachState(m_Context,EEnvironmentInteractionState.Approach));
        m_States.Add(EEnvironmentInteractionState.Rise,new RiseState(m_Context,EEnvironmentInteractionState.Rise));
        m_States.Add(EEnvironmentInteractionState.Search,new SearchState(m_Context,EEnvironmentInteractionState.Search));
        m_States.Add(EEnvironmentInteractionState.Touch,new TouchState(m_Context,EEnvironmentInteractionState.Touch));
        m_CurrentState = m_States[EEnvironmentInteractionState.Reset];
    } 

    private void ConstructEnvironmenDetectionCollider(){

        float wingspan = m_Character.height;

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(wingspan,wingspan,wingspan);
        box.center = new Vector3(m_Character.center.x,m_Character.center.y + (0.25f * wingspan),
            m_Character.center.z + (0.5f * wingspan));
        box.isTrigger = true;

        m_Context.ColliderCenterY = m_Character.center.y;  //腰部高度
    }

}
