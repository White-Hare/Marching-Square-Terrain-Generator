using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MarchingSquares))]
public class MarchingSquareEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MarchingSquares ms = target as MarchingSquares;
        

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawDefaultInspector();

            if(check.changed)
                ms.GenerateVerticies();
        }
    }
}

