using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;


[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MarchingSquares : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float threshholdValue = 0.5f;
    [SerializeField, Range(1, 200)] private int width = 10, depth = 5;
    [SerializeField, Range(0f, 100f)] private float stepHeight = 1;
    [SerializeField, Range(-1000f, 1000f)] private float noiseXOffset = 0f, noiseYOffset = 0f;

    [HideInInspector]public float[] noiseScales;


    private Vector4[] positions;

    private List<Vector3> verticies;
    private List<Vector2> uv;
    private List<int> indicies;
    private Dictionary<Vector3, int> vertexIndexRelationShip;
    private int lastIndex = 0;

    private Mesh mesh;


    private void Awake()
    {
        GenerateVertices();
    }

    private void SetDefualtValues()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        positions = new Vector4[(width + 1) * (depth + 1)];
        
        verticies = new List<Vector3>();
        indicies = new List<int>();
        uv = new List<Vector2>();
        vertexIndexRelationShip = new Dictionary<Vector3, int>();
        lastIndex = 0;
    }

    public void GenerateVertices()
    {
        SetDefualtValues();

        int index = 0;

        for (float z = -depth / 2f; z <= depth / 2f; z++)
        for (float x = -width / 2f; x <= width / 2f; x++)
        {
            float w = 0 ,y = 0;
            for (int i = 0; i < noiseScales.Length; i++)
            {
                float xCoord = noiseYOffset + (z + depth/ 2f) / depth * noiseScales[i];
                float yCoord = noiseXOffset + (x + width / 2f) / width * noiseScales[i];

                float noise = Mathf.PerlinNoise(xCoord, yCoord);

                float dy = 1 / Mathf.Pow(2, i);
                y += dy;

                float dw = noise * dy ;
                w += dw;
            }


            w = Mathf.Min(Mathf.Max(0, w / y - threshholdValue), 1);
            y = (int)(w * stepHeight);

            positions[index++] = new Vector4(x, y, z, w);
        }
        ////////////////////////////////////////////////////////////////////////////////////


        for(int z = 0, i = 0; z < depth; z++, i++)
        for (int x = 0; x < width; x++, i++)
        {
            AddVertices(
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

        mesh.uv = uv.ToArray();
        uv.Clear();
        uv = null;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private void AddVertices(Vector4 v11, Vector4 v10, Vector4 v01,Vector4 v00)
    {
        int cs = (int)(Mathf.Ceil(v11.w) * 8 + Mathf.Ceil(v10.w) * 4 + Mathf.Ceil(v01.w) * 2 + Mathf.Ceil(v00.w) * 1);

        Vector3 center = (Vector3)(v00 + v01 + v10 + v11) / 4f;

        Vector3 a = (Vector3)(v00 + v01) / 2f;
        Vector3 b = (Vector3)(v01 + v11) / 2f; 
        Vector3 c = (Vector3)(v10 + v11) / 2f; 
        Vector3 d = (Vector3)(v00 + v10) / 2f;

        if (v00.w <= 0 || v01.w <= 0 || v10.w <= 0 || v11.w <= 0)
        {
            center.y = 0;
            a.y = 0;
            b.y = 0;
            c.y = 0;
            d.y = 0;
        }

        void AddVertices(params Vector3[] vertexArr)
        {
            foreach (Vector3 vertex in vertexArr)
            {
                if (vertexIndexRelationShip.ContainsKey(vertex))
                    indicies.Add(vertexIndexRelationShip[vertex]);

                else
                {


                    vertexIndexRelationShip.Add(vertex, lastIndex);

                    this.verticies.Add(vertex);


                    int s = 0;
                    foreach (Vector3 v2 in vertexArr)
                    {
                        if(vertex == v2) continue;

                        if (!vertex.y.Equals(v2.y))
                            s++;
                    }

                    Vector2 uvVec;
                    switch (s)
                    {
                        case 0:
                            uvVec = new Vector2(vertex.x / width + 0.5f, vertex.z / depth + 0.5f); 
                            break;

                        case 1:
                            uvVec = new Vector2((vertex.x - 1 + Mathf.Sqrt(2)) / width + 0.5f, vertex.z / depth + 0.5f); 
                            break;

                        default:
                            uvVec = new Vector2((vertex.x - 1 + Mathf.Sqrt(2)) / width + 0.5f, vertex.z / depth + 0.5f);
                            break;
                    }

                    this.uv.Add(uvVec);



                    this.indicies.Add(lastIndex++);
                }
            }    
        }

        switch (cs)
        {
            case 0:
                break;
            case 1:
                AddVertices(d, a, (Vector3)v00);
                break;
            case 2:
                AddVertices(a, b, (Vector3)v01);
                break;
            case 3:
                AddVertices((Vector3)v00, d, b, (Vector3)v00, b, (Vector3)v01);
                break;
            case 4:
                AddVertices(c, d, (Vector3)v10);
                break;
            case 5:
                AddVertices((Vector3)v00, c, a, (Vector3)v00, (Vector3)v10, c);
                break;
            case 6:
                AddVertices(
                    a, b, (Vector3)v01,
                    a, c, b,
                    a, d, c,
                    c, d, (Vector3)v10
                );
                break;
            case 7:
                AddVertices(
                    (Vector3) v00, (Vector3) v10, (Vector3) v01,
                    (Vector3) v01, c, b,
                    (Vector3) v01, (Vector3) v10, c
                );
                break;
            case 8:
                AddVertices(b, c,(Vector3)v11);
                break;
            case 9:
                AddVertices(
                    a, (Vector3) v00, d,
                    a, c, b,
                    a, d, c,
                    b, c, (Vector3)v11
                );
                break;
            case 10:
                AddVertices((Vector3)v01, a, (Vector3)v11, a, c, (Vector3)v11);
                break;
            case 11:
                AddVertices(
                    (Vector3)v00, d, (Vector3)v01,
                    c, (Vector3)v01, d,
                    (Vector3)v01, c, (Vector3)v11
                );
                break;
            case 12:
                AddVertices(d, (Vector3)v10, b, b, (Vector3)v10, (Vector3)v11);
                break;
            case 13:
                AddVertices(
                    (Vector3)v00, (Vector3)v10, a,
                    a, (Vector3)v10, b,
                    b, (Vector3)v10, (Vector3)v11);
                break;
            case 14:
                AddVertices(
                    a, (Vector3)v11, (Vector3)v01, 
                    a, d, (Vector3)v11,
                    d, (Vector3)v10, (Vector3)v11);
                break;
            case 15:
                AddVertices((Vector3)v00, (Vector3)v10, (Vector3)v01, (Vector3)v01, (Vector3)v10, (Vector3)v11);
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
