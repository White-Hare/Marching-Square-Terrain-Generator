using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MarchingSquares))]
public class MarchingSquareEditor : Editor
{
    private MarchingSquares ms;
    private int noiseCount = 0;
    private int lastNoiseCount = 0;

    private bool showNoises = false;
    private bool showNormals = false;

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

             showNormals = EditorGUILayout.Toggle("Show Normals", showNormals);

            if (check.changed || GUILayout.Button("Recalculate"))
            {
                if (noiseCount != lastNoiseCount)
                {
                    float[] ns = new float[noiseCount];

                    for (int i = 0; i < ns.Length && i < ms.noiseScales.Length; i++)
                        ns[i] = ms.noiseScales[i];


                    ms.noiseScales = ns;
                    lastNoiseCount = noiseCount;
                }

                ms.GenerateVertices();
            }
        }
    }

    private void OnSceneGUI()
    {
        if (showNormals)
        {
            MarchingSquares t = target as MarchingSquares;

            if (t == null)
                return;

            MeshFilter meshFilter = t.GetComponent<MeshFilter>();

            if (meshFilter == null)
                return;


            Mesh mesh = meshFilter.sharedMesh;


            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Handles.matrix = t.transform.localToWorldMatrix;
                Handles.color = Color.yellow;
                Handles.DrawLine(
                    mesh.vertices[i],
                    mesh.vertices[i] + mesh.normals[i]);
            }
        }
    }
}

