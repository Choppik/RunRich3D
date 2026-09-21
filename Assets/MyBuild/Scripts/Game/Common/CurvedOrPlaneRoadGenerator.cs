using UnityEngine;

/// <summary>
/// Универсальный генератор дорожных секций:
/// - MeshType.Plane  — обычный плоскостной сегмент (quad) с заданной длиной и шириной.
/// - MeshType.Arc    — дугообразная секция с плавной внешней стороной.
/// Параметр innerCornerSharp, если true, сводит внутренний радиус почти к нулю, создавая острый внутренний угол.
/// Поддерживает опцию thickness (0 = плоский), MeshCollider и автоматическую сборку в редакторе.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedOrPlaneRoadGenerator : MonoBehaviour
{
    public enum MeshType { Plane, Arc }

    [Header("General")]
    public MeshType meshType = MeshType.Arc;
    public Material material;
    public bool addMeshCollider = false;
    public bool doubleSided = false;

    [Header("Plane settings")]
    public float planeWidth = 2f;
    public float planeLength = 4f;
    public int planeSegLength = 1; // число сегментов вдоль длины (>=1)
    public int planeSegWidth = 1;  // число сегментов по ширине (>=1)

    [Header("Arc settings")]
    [Tooltip("Угол дуги в градусах (положительный = CCW)")]
    public float angleDeg = 90f;
    [Tooltip("Радиус по центру дуги")]
    public float radius = 5f;
    [Tooltip("Ширина дорожного полотна")]
    public float width = 2f;
    [Range(4, 256)] public int arcSegments = 32;

    [Header("Inner corner behavior")]
    [Tooltip("Если true — внутренняя грань делается острой (inner radius ~ 0).")]
    public bool innerCornerSharp = false;
    [Tooltip("Если innerCornerSharp=true, этот маленький положительный радиус используется вместо 0 (во избежание вырожденных мешей).")]
    public float innerSharpMin = 0.001f;

    [Header("Thickness")]
    [Tooltip("Толщина секции (0 = плоский)")]
    public float thickness = 0f;

    // Build API
    [ContextMenu("Build Mesh")]
    public void Build()
    {
        if (meshType == MeshType.Plane)
            BuildPlane();
        else
            BuildArc();
    }

    private void BuildPlane()
    {
        planeSegLength = Mathf.Max(1, planeSegLength);
        planeSegWidth = Mathf.Max(1, planeSegWidth);

        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        if (material != null) mr.sharedMaterial = material;

        Mesh mesh = GeneratePlaneMesh(planeWidth, planeLength, planeSegWidth, planeSegLength, thickness, doubleSided);
        mf.sharedMesh = mesh;

        SetupCollider(mesh);
    }

    private void BuildArc()
    {
        arcSegments = Mathf.Clamp(arcSegments, 4, 1024);
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        if (material != null) mr.sharedMaterial = material;

        Mesh mesh = GenerateArcMesh(angleDeg, radius, width, arcSegments, thickness, doubleSided, innerCornerSharp ? innerSharpMin : -1f);
        mf.sharedMesh = mesh;

        SetupCollider(mesh);
    }

    private void SetupCollider(Mesh mesh)
    {
        if (addMeshCollider)
        {
            var mc = GetComponent<MeshCollider>();
            if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = false;
        }
        else
        {
            var mc = GetComponent<MeshCollider>();
            if (mc != null) DestroyImmediate(mc);
        }
    }

    // ----------------------
    // Plane generator
    // ----------------------
    private Mesh GeneratePlaneMesh(float roadWidth, float roadLength, int segW, int segL, float thick, bool doubleSided)
    {
        int cols = segW + 1;
        int rows = segL + 1;
        bool volumetric = thick > 0.0001f;

        var verts = new System.Collections.Generic.List<Vector3>();
        var uvs = new System.Collections.Generic.List<Vector2>();
        var normals = new System.Collections.Generic.List<Vector3>();
        var tris = new System.Collections.Generic.List<int>();

        float halfW = roadWidth * 0.5f;
        float startZ = -roadLength * 0.5f;
        float dz = roadLength / segL;
        float dx = roadWidth / segW;
        float yTop = volumetric ? (thick * 0.5f) : 0f;
        float yBottom = volumetric ? (-thick * 0.5f) : 0f;

        // Top surface vertices
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = -halfW + c * dx;
                float z = startZ + r * dz;
                verts.Add(new Vector3(x, yTop, z));
                uvs.Add(new Vector2((float)c / (cols - 1), (float)r / (rows - 1)));
                normals.Add(Vector3.up);
            }
        }

        // Top triangles
        for (int r = 0; r < segL; r++)
        {
            for (int c = 0; c < segW; c++)
            {
                int i00 = r * cols + c;
                int i10 = r * cols + c + 1;
                int i01 = (r + 1) * cols + c;
                int i11 = (r + 1) * cols + c + 1;
                tris.Add(i00); tris.Add(i11); tris.Add(i01);
                tris.Add(i00); tris.Add(i10); tris.Add(i11);
            }
        }

        int topVertexCount = verts.Count;

        if (volumetric)
        {
            // bottom vertices
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = -halfW + c * dx;
                    float z = startZ + r * dz;
                    verts.Add(new Vector3(x, yBottom, z));
                    uvs.Add(new Vector2((float)c / (cols - 1), (float)r / (rows - 1)));
                    normals.Add(Vector3.down);
                }
            }

            int bottomStart = topVertexCount;
            // bottom tris (reverse winding)
            for (int r = 0; r < segL; r++)
            {
                for (int c = 0; c < segW; c++)
                {
                    int i00 = bottomStart + r * cols + c;
                    int i10 = bottomStart + r * cols + c + 1;
                    int i01 = bottomStart + (r + 1) * cols + c;
                    int i11 = bottomStart + (r + 1) * cols + c + 1;
                    tris.Add(i00); tris.Add(i01); tris.Add(i11);
                    tris.Add(i00); tris.Add(i11); tris.Add(i10);
                }
            }

            // sides (4 sides)
            // front/back and left/right
            // front (r = 0)
            for (int c = 0; c < segW; c++)
            {
                int top0 = 0 * cols + c;
                int top1 = 0 * cols + c + 1;
                int bot0 = bottomStart + 0 * cols + c;
                int bot1 = bottomStart + 0 * cols + c + 1;
                // quad top0, top1, bot1, bot0
                tris.Add(top0); tris.Add(top1); tris.Add(bot1);
                tris.Add(top0); tris.Add(bot1); tris.Add(bot0);
            }
            // back (r = segL)
            int rback = segL;
            for (int c = 0; c < segW; c++)
            {
                int top0 = rback * cols + c;
                int top1 = rback * cols + c + 1;
                int bot0 = bottomStart + rback * cols + c;
                int bot1 = bottomStart + rback * cols + c + 1;
                tris.Add(top1); tris.Add(top0); tris.Add(bot1);
                tris.Add(top0); tris.Add(bot0); tris.Add(bot1);
            }
            // left (c=0)
            for (int r = 0; r < segL; r++)
            {
                int top0 = r * cols + 0;
                int top1 = (r + 1) * cols + 0;
                int bot0 = bottomStart + r * cols + 0;
                int bot1 = bottomStart + (r + 1) * cols + 0;
                tris.Add(top1); tris.Add(top0); tris.Add(bot1);
                tris.Add(top0); tris.Add(bot0); tris.Add(bot1);
            }
            // right (c=segW)
            int cright = segW;
            for (int r = 0; r < segL; r++)
            {
                int top0 = r * cols + cright;
                int top1 = (r + 1) * cols + cright;
                int bot0 = bottomStart + r * cols + cright;
                int bot1 = bottomStart + (r + 1) * cols + cright;
                tris.Add(top0); tris.Add(top1); tris.Add(bot1);
                tris.Add(top0); tris.Add(bot1); tris.Add(bot0);
            }
        }
        else if (doubleSided)
        {
            // duplicate triangles reversed
            int t = tris.Count;
            for (int i = 0; i < t; i += 3)
            {
                tris.Add(tris[i]); tris.Add(tris[i + 2]); tris.Add(tris[i + 1]);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = verts.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    // ----------------------
    // Arc generator (with optional sharp inner)
    // ----------------------
    // If innerSharpRadius >= 0 then inner radius will be set to that small value (sharp inner).
    private Mesh GenerateArcMesh(float angleDeg, float centerRadius, float roadWidth, int segs, float thick, bool doubleSided, float innerSharpRadius = -1f)
    {
        float half = roadWidth * 0.5f;
        float innerR_default = centerRadius - half;
        float innerR = innerR_default;
        if (innerSharpRadius >= 0f) innerR = Mathf.Max(innerSharpRadius, 0.000001f);

        float outerR = centerRadius + half;
        float angleRad = Mathf.Deg2Rad * angleDeg;
        int steps = Mathf.Max(1, segs);
        bool volumetric = thick > 0.0001f;

        var verts = new System.Collections.Generic.List<Vector3>();
        var uvs = new System.Collections.Generic.List<Vector2>();
        var normals = new System.Collections.Generic.List<Vector3>();
        var tris = new System.Collections.Generic.List<int>();

        float yTop = volumetric ? (thick * 0.5f) : 0f;
        float yBottom = volumetric ? (-thick * 0.5f) : 0f;

        // If innerCornerSharp requested and innerR is very small we collapse inner ring close to origin of circle.
        bool innerCollapsed = innerSharpRadius >= 0f;

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float a = t * angleRad;
            float x = Mathf.Sin(a);
            float z = Mathf.Cos(a);
            Vector3 dir = new Vector3(x, 0f, z);

            // outer top
            verts.Add(dir * outerR + Vector3.up * yTop);
            uvs.Add(new Vector2(t, 1f));
            normals.Add(Vector3.up);

            // inner top (if collapsed, use single vertex at small radius angle 0? We use radial small radius at same angle)
            verts.Add(dir * innerR + Vector3.up * yTop);
            uvs.Add(new Vector2(t, 0f));
            normals.Add(Vector3.up);

            if (volumetric)
            {
                verts.Add(dir * outerR + Vector3.up * yBottom);
                uvs.Add(new Vector2(t, 1f));
                normals.Add(Vector3.down);

                verts.Add(dir * innerR + Vector3.up * yBottom);
                uvs.Add(new Vector2(t, 0f));
                normals.Add(Vector3.down);
            }
        }

        int stride = volumetric ? 4 : 2;

        // top surface
        for (int i = 0; i < steps; i++)
        {
            int baseIdx = i * stride;
            int outer0 = baseIdx + 0;
            int inner0 = baseIdx + 1;
            int outer1 = baseIdx + stride + 0;
            int inner1 = baseIdx + stride + 1;

            // If innerCollapsed and innerR is almost zero, mesh becomes triangle-fan-ish but still works.
            tris.Add(outer0); tris.Add(outer1); tris.Add(inner1);
            tris.Add(outer0); tris.Add(inner1); tris.Add(inner0);

            if (volumetric)
            {
                int offset = (steps + 1) * 2;
                int boffset = offset + baseIdx;
                int b_outer0 = boffset + 0;
                int b_inner0 = boffset + 1;
                int b_outer1 = boffset + stride + 0;
                int b_inner1 = boffset + stride + 1;

                tris.Add(b_outer0); tris.Add(b_inner1); tris.Add(b_outer1);
                tris.Add(b_outer0); tris.Add(b_inner0); tris.Add(b_inner1);

                // outer side
                tris.Add(outer0); tris.Add(outer1); tris.Add(b_outer1);
                tris.Add(outer0); tris.Add(b_outer1); tris.Add(b_outer0);

                // inner side
                tris.Add(inner0); tris.Add(b_inner1); tris.Add(inner1);
                tris.Add(inner0); tris.Add(b_inner0); tris.Add(b_inner1);
            }
        }

        if (doubleSided && !volumetric)
        {
            int tcount = tris.Count;
            for (int i = 0; i < tcount; i += 3)
            {
                tris.Add(tris[i]); tris.Add(tris[i + 2]); tris.Add(tris[i + 1]);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = verts.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // rebuild in editor for quick feedback
        if (!Application.isPlaying) Build();
    }
#endif
}