using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


[CustomEditor(typeof(MarchingSquares))]
public class MarchingSquareEditor : Editor
{
    private MarchingSquares ms;
    private int noiseCount = 0;
    private int lastNoiseCount = 0;

    private bool showNoises = false;

    private void OnEnable()
    {
        ms = target as MarchingSquares;
    }

    public override void OnInspectorGUI()
    {


        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawDefaultInspector();


             showNoises = EditorGUILayout.BeginFoldoutHeaderGroup(showNoises, "Noises");

             if (showNoises)
             {
                 noiseCount = EditorGUILayout.IntField("Noise Count", noiseCount);
                 if (noiseCount < 1) noiseCount = 1;

                 for (int i = 0; i < ms.noiseScales.Length; i++)
                     ms.noiseScales[i] = EditorGUILayout.Slider("Noise " + i, ms.noiseScales[i], 0f, 100f);
             }

             EditorGUILayout.EndFoldoutHeaderGroup();

            
            if (check.changed)
            {
                if (noiseCount != lastNoiseCount)
                {
                    float[] ns = new float[noiseCount];

                    for (int i = 0; i < ns.Length && i < ms.noiseScales.Length; i++)
                        ns[i] = ms.noiseScales[i];


                    ms.noiseScales = ns;
                    lastNoiseCount = noiseCount;
                }

                ms.GenerateVerticies();
            }
        }
    }
}

