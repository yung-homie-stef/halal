﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class KW_InteractWithWater : MonoBehaviour
{
    [Range(0.025f, 10)]
    public float Size = 0.15f;
    [Range(0.05f, 1)]
    public float Strength = 1.0f;
    [Range(-1.0f, 1.0f)]
    public float Pressure = 0.0f;
    public Vector3 Offset = Vector3.zero;
    

    [HideInInspector]
    Transform _t;

    float sizeRelativeToHeight;
    Vector3 startScale;
    public Transform t
    {
        get
        {
            if (_t == null) _t = transform;
            return _t;
        }
    }

    Vector3 lastPos;

    void Awake()
    {
        lastPos = t.position;
     //   startScale = t.localScale;
    }

    public float GetForce(float waterHeight)
    {
        var pos = t.TransformPoint(Offset);
        
        var force = (Vector3.Distance(pos, lastPos));
        force = Mathf.Min(force, 1) * Strength;

        var heightRelativeToWater = 1f - Mathf.Clamp01(Mathf.Abs(pos.y - waterHeight) / (Size * 0.5f)); // 0 -> non intersected, 1 -> full intersected
        sizeRelativeToHeight = heightRelativeToWater * Size;

        if (Size > 1) force = Mathf.Lerp(force * 1, force * 0.25f, Size / 10f);

        force -= Pressure;
        lastPos = pos;

        return force;
    }

    public float GetIntersectionSize()
    {
        return sizeRelativeToHeight;
    }

    void OnEnable()
    {
        KW_WaterDynamicScripts.AddInteractScript(this);
        lastPos = t.TransformPoint(Offset);
    }

    void OnDisable()
    {
        KW_WaterDynamicScripts.RemoveInteractScript(this);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(t.TransformPoint(Offset), Size * 0.5f);
    }
}
