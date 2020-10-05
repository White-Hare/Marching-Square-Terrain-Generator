using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MarchingSquares : MonoBehaviour
{
    [SerializeField, Range(1, 100)] private int width = 10, depth = 5;
    [SerializeField, Range(0f, 100f)] private float maxHeight = 0, noiseXOffset = 1f, noiseYOffset = 1f, noiseScale = 2f;
    [SerializeField, Range(0f, 1f)] private float threshholdValue = 0.5f;


    private Vector4[] positions;

    private List<Vector3> verticies;
    private List<int> indicies;
    private Dictionary<Vector3, int> vertexIndexRelationShip;
    private int lastIndex = 0;

    private Mesh mesh;


    private void Awake()
    {
        GenerateVerticies();
    }

    private void SetDefualtValues()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        positions = new Vector4[(width + 1) * (depth + 1)];
        
        verticies = new List<Vector3>();
        indicies = new List<int>();
        vertexIndexRelationShip = new Dictionary<Vector3, int>();
        lastIndex = 0;
    }

    public void GenerateVerticies()
    {
        SetDefualtValues();

        int index = 0;

        for (float z = -depth / 2f; z <= depth / 2f; z++)
        for (float x = -width / 2f; x <= width / 2f; x++)
        {
            float xCoord = noiseXOffset + (x + width / 2f) / width * noiseScale;
            float yCoord = noiseYOffset + (z + depth/ 2f) / depth * noiseScale;

            
            float w = Mathf.PerlinNoise(xCoord, yCoord);
            if (w < threshholdValue) w = 0;
            w = Mathf.Min(w, 1f);

            positions[index++] = new Vector4(x, (w - threshholdValue) * maxHeight, z, w);
        }
        ////////////////////////////////////////////////////////////////////////////////////


        for(int z = 0, i = 0; z < depth; z++, i++)
        for (int x = 0; x < width; x++, i++)
        {
            AddVerticies(
                positions[i + width + 2], 
                positions[i + width + 1], 
                positions[i + 1], 
                positions[i]);
        }
        

        vertexIndexRelationShip.Clear();
        vertexIndexRelationShip = null;

        mesh.vertices = verticies.ToArray();
        verticies.Clear();
        verticies = null;

        mesh.triangles = indicies.ToArray();
        indicies.Clear();
        indicies = null;

        mesh.RecalculateNormals();
    }

    private void AddVerticies(Vector4 v11, Vector4 v10, Vector4 v01,Vector4 v00)
    {
        int cs = (int)(Mathf.Ceil(v11.w) * 8 + Mathf.Ceil(v10.w) * 4 + Mathf.Ceil(v01.w) * 2 + Mathf.Ceil(v00.w) * 1);

        Vector3 center = (Vector3)(v00 + v01 + v10 + v11) / 4f;

        Vector3 a = (Vector3)(v00 + v01) / 2f;
        Vector3 b = (Vector3)(v01 + v11) / 2f; 
        Vector3 c = (Vector3)(v10 + v11) / 2f; 
        Vector3 d = (Vector3)(v00 + v10) / 2f;


        void AddVerticies(params Vector3[] vertexArr)
        {
            foreach (Vector3 vertex in vertexArr)
            {
                if (vertexIndexRelationShip.ContainsKey(vertex))
                    indicies.Add(vertexIndexRelationShip[vertex]);

                else
                {
                    vertexIndexRelationShip.Add(vertex, lastIndex);

                    this.verticies.Add(vertex);
                    this.indicies.Add(lastIndex++);
                }
            }    
        }

        switch (cs)
        {
            case 0:
                break;
            case 1:
                AddVerticies(d, a, (Vector3)v00);
                break;
            case 2:
                AddVerticies(a, b, (Vector3)v01);
                break;
            case 3:
                AddVerticies((Vector3)v00, d, b, (Vector3)v00, b, (Vector3)v01);
                break;
            case 4:
                AddVerticies(c, d, (Vector3)v10);
                break;
            case 5:
                AddVerticies((Vector3)v00, c, a, (Vector3)v00, (Vector3)v10, c);
                break;
            case 6:
                AddVerticies(
                    a, b, (Vector3)v01,
                    a, c, b,
                    a, d, c,
                    c, d, (Vector3)v10
                );
                break;
            case 7:
                AddVerticies(
                    (Vector3) v00, (Vector3) v10, (Vector3) v01,
                    (Vector3) v01, c, b,
                    (Vector3) v01, (Vector3) v10, c
                );
                break;
            case 8:
                AddVerticies(b, c,(Vector3)v11);
                break;
            case 9:
                AddVerticies(
                    a, (Vector3) v00, d,
                    a, c, b,
                    a, d, c,
                    b, c, (Vector3)v11
                );
                break;
            case 10:
                AddVerticies((Vector3)v01, a, (Vector3)v11, a, c, (Vector3)v11);
                break;
            case 11:
                AddVerticies(
                    (Vector3)v00, d, (Vector3)v01,
                    c, (Vector3)v01, d,
                    (Vector3)v01, c, (Vector3)v11
                );
                break;
            case 12:
                AddVerticies(d, (Vector3)v10, b, b, (Vector3)v10, (Vector3)v11);
                break;
            case 13:
                AddVerticies(
                    (Vector3)v00, (Vector3)v10, a,
                    a, (Vector3)v10, b,
                    b, (Vector3)v10, (Vector3)v11);
                break;
            case 14:
                AddVerticies(
                    a, (Vector3)v11, (Vector3)v01, 
                    a, d, (Vector3)v11,
                    d, (Vector3)v10, (Vector3)v11);
                break;
            case 15:
                AddVerticies((Vector3)v00, (Vector3)v10, (Vector3)v01, (Vector3)v01, (Vector3)v10, (Vector3)v11);
                break;

            default:
                throw new Exception("Wrong Value: " + cs);
        }
    }

    private void OnDrawGizmos()
    {
        if (positions != null)
        {
            foreach (var pos in positions)
            {
                Gizmos.color = new Color(pos.w, pos.w, pos.w);
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }

}
