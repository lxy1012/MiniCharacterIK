using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext
{
    public enum EBodySide{
        LEFT,
        RIGHT,
        NOSIDE
    }
    private TwoBoneIKConstraint m_LeftIKConstraint;
    private MultiRotationConstraint m_LeftMultiRotationConstraint;
    private TwoBoneIKConstraint m_RightIKConstraint;
    private MultiRotationConstraint m_RightMultiRotationConstraint;
    private CharacterController m_Character;
    private Transform m_RootTransform;
    private Vector3 m_LeftOriginalTargetPosition;
    private Vector3 m_RightOriginalTargetPosition;
    private const float offsetZ = 0.3f;


    public EnvironmentInteractionContext(TwoBoneIKConstraint leftIKConstraint,MultiRotationConstraint leftMultiRotationConstraint,
        TwoBoneIKConstraint rightIKConstraint,MultiRotationConstraint rightMultiRotationConstraint,CharacterController character,Transform rootTransform
    ){
        m_LeftIKConstraint = leftIKConstraint;
        m_LeftMultiRotationConstraint = leftMultiRotationConstraint;
        m_RightIKConstraint = rightIKConstraint;
        m_RightMultiRotationConstraint = rightMultiRotationConstraint;
        m_Character = character;
        m_RootTransform = rootTransform;
        CharacterShoulderHeight = leftIKConstraint.data.root.position.y;
        m_LeftOriginalTargetPosition = leftIKConstraint.data.target.transform.localPosition;
        m_RightOriginalTargetPosition = rightIKConstraint.data.target.localPosition;
        CurrentOrignalTargetRotation = leftIKConstraint.data.target.rotation;
        SetCurrentSide(Vector3.positiveInfinity);
    }

    public TwoBoneIKConstraint LeftIKConstraint => m_LeftIKConstraint;
    public MultiRotationConstraint LeftMultiRotationConstraint => m_LeftMultiRotationConstraint;
    public TwoBoneIKConstraint RightIKConstraint => m_RightIKConstraint;
    public MultiRotationConstraint RightMultiRotationConstraint => m_RightMultiRotationConstraint;
    public CharacterController Character => m_Character;
    public Transform RootTransform => m_RootTransform;
    

    public Collider CurrentInersectingCollider{ get;set;}
    public TwoBoneIKConstraint CurrentIKConstraint { get;private set;}
    public MultiRotationConstraint CurrentMultiRotationConstraint{ get;private set;}
    public Transform CurrentIKTargetTransform { get;private set;}
    public Transform CurrentShoulderTransform { get;private set;}
    public EBodySide CurrentBodySide { get;private set;}
    //public Vector3 CurrentIKTargetPosition {get;set;}  //IK 触发点
    public Vector3 RealPositionFromShoulderPoint{
        get{
            var offsetForward = m_Character.transform.forward.normalized * offsetZ;
            return CurrentShoulderTransform.position + offsetForward;
        }
    }
    public Vector3 CloestPointOnColliderFromShoulder {get;set;}
    public float CharacterShoulderHeight {get;private set;}
    public float InteractionPointYOffset {get; set;} = 0;
    public float ColliderCenterY {get; set;}
    public Vector3 CurrentOrignalTargetPosition {get;private set;}
    public Quaternion CurrentOrignalTargetRotation {get;private set;}
    public float LowestDistance {get;set;} = Mathf.Infinity;
    public Quaternion CurrentApproachTargetQuaternion{get;private set;}


    public void SetCurrentSide(Vector3 positionCheck){
        
        Vector3 leftShoulder = m_LeftIKConstraint.data.root.transform.position;
        Vector3 rightShoulder = m_RightIKConstraint.data.root.transform.position;

        float leftPos = Vector3.Distance(positionCheck , leftShoulder);
        float rightPos = Vector3.Distance(positionCheck , rightShoulder);
        bool isLeftCloser = leftPos < rightPos;
        if(isLeftCloser){

            CurrentBodySide = EBodySide.LEFT;
            CurrentIKConstraint = m_LeftIKConstraint;
            CurrentOrignalTargetPosition = m_LeftOriginalTargetPosition;
            CurrentMultiRotationConstraint = m_LeftMultiRotationConstraint;
        }
        else{
            
            CurrentBodySide = EBodySide.RIGHT;
            CurrentIKConstraint = m_RightIKConstraint;
            CurrentOrignalTargetPosition = m_RightOriginalTargetPosition;
            CurrentMultiRotationConstraint = m_RightMultiRotationConstraint;
        }

        CurrentIKTargetTransform = CurrentIKConstraint.data.target.transform;
        CurrentShoulderTransform = CurrentIKConstraint.data.root.transform;
    }

    /* /// <summary>
    /// reset阶段不更新IK position，由RESET状态自身控制
    /// </summary>
    public void CheckResetBodySide()
    {
        CurrentBodySide = EBodySide.NOSIDE;
    }

    public Quaternion MirrorRotation(Quaternion original)
    {
        return new Quaternion(-original.x,original.y,-original.z,original.w);
    } */

}
