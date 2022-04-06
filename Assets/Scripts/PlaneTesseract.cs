using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTesseract : MonoBehaviour
{
    
    //[Range(0, 360)]
    public float rotationXY, rotationYZ, rotationZX, rotationXW, rotationYW, rotationZW;
    public float morphSpeed = 30;
    
    public Transform viewPoint;
    public Transform player;
    public float scale = 1;
    public bool createAssetsAndPrefab = false;
    MeshFilter[] faces;
    private float targetZW, targetZX, targetYZ;

    void Awake()
    {
        faces = GetComponentsInChildren<MeshFilter>();
    }

    void Update()
    {
        if (viewPoint.hasChanged) Project();
        if (Morphing) Morph();
    }

    void OnValidate()
    {
        if (Application.isPlaying && !Application.isEditor) Project();
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

        for (int i = 0; i < faces.Length; i += 2)
        {
            Vector4[] points = new Vector4[4];
            for (int p = 0; p < 4; p++)
            {
                points[p] = UtilsGeom4D.kTesseractPoints[UtilsGeom4D.kTesseractPlanes[i / 2, p]];
            }
            Vector3[] vertices = new Vector3[4];
            UtilsGeom4D.ProjectTo3DPerspective(points, matrix, ref vertices, viewingAngle, fromDir, toDir, upDir, overDir);
            for (int v = 0; v < vertices.Length; v++)
            {
                vertices[v] *= scale;
            }
            faces[i].mesh = UpdateMesh(faces[i], vertices);
            faces[i + 1].mesh = UpdateMesh(faces[i + 1], vertices);
        }
    }

    Mesh UpdateMesh(MeshFilter meshFilter, Vector3[] vertices)
    {
        Mesh updatedMesh = meshFilter.mesh;
        updatedMesh.vertices = vertices;
        updatedMesh.RecalculateNormals();
        updatedMesh.RecalculateTangents();
        updatedMesh.RecalculateBounds();
        return updatedMesh;
    }

    void Morph()
    {
        rotationZW = Mathf.MoveTowardsAngle(rotationZW, targetZW, morphSpeed * Time.deltaTime);
        rotationZX = Mathf.MoveTowardsAngle(rotationZX, targetZX, morphSpeed * Time.deltaTime);
        rotationYZ = Mathf.MoveTowardsAngle(rotationYZ, targetYZ, morphSpeed * Time.deltaTime);
    }

    bool Morphing
    {
        get
        {
            return targetZX != rotationZX || targetZW != rotationZW || targetYZ != rotationYZ;
        }
    }

    public void StartMorph(float x, float y, float z)
    {
        
        if (Morphing)
        {
            Debug.LogWarning("Tried to start a new morph while already morphing");
            return;
        }
        targetZW = (rotationZW + 90 * x + 360) % 360;
        targetYZ = (rotationYZ - 90 * y) % 360;
        targetZX = (rotationZX - 90 * z) % 360;
        
        //print($"Starting morph: ({x}, {y}, {z}) targetZW: {targetZW} targetYZ: {targetYZ} targetZX: {targetZX}");
    }


}
