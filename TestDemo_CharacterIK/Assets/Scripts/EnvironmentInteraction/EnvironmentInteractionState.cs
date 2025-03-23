using System.Collections;
using System.Collections.Generic;
using System.Text;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{   
    protected EnvironmentInteractionContext m_Context;
    private Vector3 touchPos = Vector3.positiveInfinity;
    private float m_movingAwayOffset = (float)0.05;
    private bool m_shouldReset;
    public EnvironmentInteractionState(EnvironmentInteractionContext context,EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey)
        :base(stateKey)
    {
        m_Context = context;
    }

    private Vector3 GetClosestPointOnCollider(Collider intersectingCollider,Vector3 positionToCheck){
        
        return intersectingCollider.ClosestPoint(positionToCheck);
    }

    
    protected bool CheckShouldReset()
    {   
        if(m_shouldReset)
        {
            m_Context.LowestDistance = Mathf.Infinity;
            m_shouldReset = false;
            return true;
        }

        bool isPlayerStopped = m_Context.Character.velocity == Vector3.zero;
        bool isMovingAway = CheckIsMovingAway();
        bool isBadAngle = CheckIsBadAngle();
        bool isPlayerJumping = Mathf.Round(m_Context.Character.velocity.y) >= 1;

        if(isPlayerStopped || isMovingAway || isBadAngle || isPlayerJumping){
            m_Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
        
    }

    protected bool CheckIsBadAngle(){
        if(m_Context.CurrentInersectingCollider == null)
        {
            return false;
        }
        Vector3 targetDirection = m_Context.CloestPointOnColliderFromShoulder - m_Context.CurrentShoulderTransform.position;
        Vector3 shoulderDirection = m_Context.CurrentBodySide == EnvironmentInteractionContext.EBodySide.RIGHT ?
            m_Context.RootTransform.right : -m_Context.RootTransform.right;
        
        float dotProduct = Vector3.Dot(shoulderDirection,targetDirection.normalized);
        return dotProduct < 0;
    }

    protected bool CheckIsMovingAway(){
        float curDistanceToTarget = Vector3.Distance(m_Context.RootTransform.position,m_Context.CloestPointOnColliderFromShoulder);
        
        bool isSearchingForNewInteraction = m_Context.CurrentInersectingCollider == null;
        if(isSearchingForNewInteraction){
            return false;
        }

        bool isGettingCloserToTarget = curDistanceToTarget <= m_Context.LowestDistance;
        if(isGettingCloserToTarget){
            m_Context.LowestDistance = curDistanceToTarget;
            return false;
        }

        bool isMovingAwayFromTarget = curDistanceToTarget > m_Context.LowestDistance + m_movingAwayOffset;
        if(isMovingAwayFromTarget)
        {
            Debug.Log("isMovingAwayFromTarget    :"+isMovingAwayFromTarget);
            m_Context.LowestDistance = Mathf.Infinity;
            return true;
        }
        return false;
    }


    protected void StartIKTargetPositionTracking(Collider intersectingCollider){
        if(intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && m_Context.CurrentInersectingCollider == null)
        {
            m_Context.CurrentInersectingCollider = intersectingCollider;
            var pos = GetClosestPointOnCollider(intersectingCollider,m_Context.RootTransform.position);
            m_Context.SetCurrentSide(pos);

            SetIKTargetPosition();
        }        
    }

    //更新IK 目标位置
    protected void UpdateIKTargetPosition(Collider intersectingCollider){
        
        if(intersectingCollider == m_Context.CurrentInersectingCollider){
            SetIKTargetPosition();
        }
    }

    //重置IK 目标位置
    protected void ResetIKTargetPositionTracking(Collider intersectingCollider){
            
        if(m_Context.CurrentInersectingCollider == intersectingCollider)
        { 
            m_Context.CurrentInersectingCollider = null;
            m_Context.CloestPointOnColliderFromShoulder = Vector3.positiveInfinity;
            m_shouldReset = true;
        }
    }

    //设置IK 目标位置
    private void SetIKTargetPosition(){
        
        if(m_Context == null  || m_Context.CurrentBodySide == EnvironmentInteractionContext.EBodySide.NOSIDE)
            return;
        
        touchPos = m_Context.RealPositionFromShoulderPoint;
        touchPos.y = m_Context.CharacterShoulderHeight;

        m_Context.CloestPointOnColliderFromShoulder = GetClosestPointOnCollider(m_Context.CurrentInersectingCollider,touchPos);
        
        Vector3 normalizedRayDirection = (m_Context.RealPositionFromShoulderPoint - m_Context.CloestPointOnColliderFromShoulder).normalized;
        float offsetDistance = 0.05f;
        Vector3 offset = normalizedRayDirection * offsetDistance;
        Vector3 offsetPosition = m_Context.CloestPointOnColliderFromShoulder + offset;
        
        m_Context.CurrentIKTargetTransform.position = new Vector3(offsetPosition.x,m_Context.InteractionPointYOffset,offsetPosition.z); 
        
    }
} 
