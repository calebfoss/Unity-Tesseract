using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class PlaneTesseract : MonoBehaviour
{
    MeshFilter[] meshFilters;
    [Range(0, 360)]
    public float rotationXY, rotationYZ, rotationZX, rotationXW, rotationYW, rotationZW;
    public Transform viewPoint;

    void Awake()
    {
        meshFilters = new MeshFilter[UtilsGeom4D.kTesseractPlanes.GetLength(0) * 2];
        int[] frontTriangles = { 0, 1, 3, 3, 1, 2 };
        int[] backTriangles = frontTriangles.Reverse().ToArray();

        Vector3[] normals = new Vector3[4] {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        Shader shader = Shader.Find("Standard");
        for (int i = 0; i < meshFilters.Length; i+=2)
        {
            Color color = Color.HSVToRGB((i * 0.5f) / meshFilters.Length, 1, 1);
            meshFilters[i] = createFace(vertices, frontTriangles, uv, normals, shader, color, $"Face_{(i/2).ToString("D2")}_front");
            meshFilters[i + 1] = createFace(vertices, backTriangles, uv, normals, shader, color, $"Face_{(i / 2).ToString("D2")}_back");
        }
        Project();
    }

    MeshFilter createFace(Vector3[] vertices, int[] tris,Vector2[] uv, Vector3[]normals, Shader shader, Color color, string name)
    {
        GameObject frontChild = new GameObject(name);
        frontChild.transform.parent = transform;
        MeshFilter face = frontChild.AddComponent<MeshFilter>();
        MeshRenderer renderer = frontChild.AddComponent<MeshRenderer>();
        Material mat = new Material(shader);
        mat.color = color;
        renderer.material = mat;
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;
        mesh.normals = normals;
        face.mesh = mesh;
        return face;
    }

    void Update()
    {
        if (viewPoint.hasChanged) Project();
    }

    void OnValidate()
    {
        if (Application.isPlaying) Project();

    }


    void Project()
    {
        Camera cam = Camera.main;
        float viewingAngle = cam.fieldOfView;

        Matrix4x4 matrixXY = UtilsGeom4D.CreateRotationMatrixXY(rotationXY * Mathf.Deg2Rad);
        Matrix4x4 matrixYZ = UtilsGeom4D.CreateRotationMatrixYZ(rotationYZ * Mathf.Deg2Rad);
        Matrix4x4 matrixZX = UtilsGeom4D.CreateRotationMatrixZX(rotationZX * Mathf.Deg2Rad);

        Matrix4x4 matrixXW = UtilsGeom4D.CreateRotationMatrixXW(rotationXW * Mathf.Deg2Rad);
        Matrix4x4 matrixYW = UtilsGeom4D.CreateRotationMatrixYW(rotationYW * Mathf.Deg2Rad);
        Matrix4x4 matrixZW = UtilsGeom4D.CreateRotationMatrixZW(rotationZW * Mathf.Deg2Rad);

        Matrix4x4 matrix = matrixXY * matrixYZ * matrixZX * matrixXW * matrixYW * matrixZW;

        // calculate view point vectors
        Vector3 tp = transform.position;
        Vector3 cp = viewPoint.position;
        Vector3 cu = viewPoint.up;
        Vector3 co = viewPoint.right;

        Vector4 toDir = new Vector4(tp.x, tp.y, tp.z, 0);
        Vector4 fromDir = new Vector4(cp.x, cp.y, cp.z, 0);
        Vector4 upDir = new Vector4(cu.x, cu.y, cu.z, 0);
        Vector4 overDir = new Vector4(co.x, co.y, co.z, 0);
        
        for (int i = 0; i < meshFilters.Length; i+=2)
        {
            Vector4[] points = new Vector4[4];
            for (int p = 0; p < 4; p++)
            {
                points[p] = UtilsGeom4D.kTesseractPoints[UtilsGeom4D.kTesseractPlanes[i/2, p]];
            }
            Vector3[] vertices = new Vector3[4];
            UtilsGeom4D.ProjectTo3DPerspective(points, matrix, ref vertices, viewingAngle, fromDir, toDir, upDir, overDir);
            Vector3 middlePos = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4f;
            for(int v = 0; v < vertices.Length; v++)
            {
                vertices[v] -= middlePos;
            }
            meshFilters[i].mesh.vertices = vertices;
            meshFilters[i + 1].mesh.vertices = vertices;
            meshFilters[i].mesh.RecalculateNormals();
            meshFilters[i + 1].mesh.RecalculateNormals();
            meshFilters[i].transform.position = middlePos;
            meshFilters[i + 1].transform.position = middlePos;
        }

    }

    async void OnApplicationQuit()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
    }
}
