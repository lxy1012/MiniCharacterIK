using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.CM.Client.Gui;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform))]
public class TransformQuaterionEditor : Editor
{
    private const float LABEL_WIDTH = 120f;
    private SerializedProperty m_LocalPosition;
    private SerializedProperty m_LocalRotation;
    private SerializedProperty m_LocalScale;
    private SerializedProperty m_Position;

    private void OnEnable()
    {
        m_LocalPosition = serializedObject.FindProperty("m_LocalPosition");
        m_LocalRotation = serializedObject.FindProperty("m_LocalRotation");
        m_LocalScale = serializedObject.FindProperty("m_LocalScale");
        m_Position = serializedObject.FindProperty("m_Position");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //绘制默认位置和缩放
        DrawPosition();
        DrawRotationWithQuaternion();
        DrawScale();

        serializedObject.ApplyModifiedProperties();

    }

    private void DrawPosition()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_LocalPosition,new GUIContent("Position"));
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        Vector3 q = GetCurrentWorldPosition();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("World Position" ,GUILayout.Width(LABEL_WIDTH));
        GUI.enabled = false;
        EditorGUILayout.Vector3Field("",q);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    
    }

    void DrawScale()
    {
        EditorGUILayout.PropertyField(m_LocalScale, new GUIContent("Scale"));
    }

    void DrawRotationWithQuaternion()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // 绘制默认旋转字段
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_LocalRotation, new GUIContent("Rotation"));
        EditorGUILayout.EndHorizontal();

        //显示四元数信息
        EditorGUI.indentLevel++;
        Quaternion q = GetCurrentRotation();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Quaterion" ,GUILayout.Width(LABEL_WIDTH));
        GUI.enabled = false;
        EditorGUILayout.Vector4Field("",new Vector4(q.x,q.y,q.z,q.w));
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        //显示旋转角度信息
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Euler Angles",GUILayout.Width(LABEL_WIDTH));
        GUI.enabled = false;
        EditorGUILayout.Vector3Field("",q.eulerAngles);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
        
        if (targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multiple objects selected - showing first object's rotation", 
                MessageType.Info);
        }

/*         if (GUILayout.Button("Copy Quaternion", GUILayout.Width(120)))
        {
            GUIUtility.systemCopyBuffer = $"{q.x}f, {q.y}f, {q.z}f, {q.w}f";
        } */

        EditorGUILayout.EndVertical();
    }

    private Quaternion GetCurrentRotation()
    {
        if(targets.Length == 1)
        {
            return (target as Transform).localRotation;
        }

        return Quaternion.identity;
    }

    private Vector3 GetCurrentWorldPosition()
    {
        if(targets.Length == 1)
        {
            return (target as Transform).position;
        }

        return Vector3.zero;
    }
}
