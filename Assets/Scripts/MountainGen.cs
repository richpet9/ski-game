using System.Collections;
using SkiGame.Maps;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MountainGen : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

    [SerializeField]
    private bool randomizeOnGenerate = true;

    [SerializeField]
    private AnimationCurve heightCurve; // Use this in Inspector to flatten areas for plateaus!

    public void ClearMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshFilter>().mesh = null;
        Destroy(mesh);
        Destroy(GetComponent<MeshCollider>());
    }

    public void Generate(MapConfig mapConfig, MapData mapData)
    {
        int width = mapConfig.Width;
        int height = mapConfig.Height;
        float scale = mapConfig.NoiseScale;

        Mesh mesh = new();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];

        Vector2 center = new(width / 2f, height / 2f);
        float xOffset = mapConfig.Seed + 9999f;
        float zOffset = mapConfig.Seed + 9999f;

        for (int z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                // 1. Calculate base noise
                float xCoord = (float)x / width * scale + xOffset;
                float zCoord = (float)z / height * scale + zOffset;
                float noise = Mathf.PerlinNoise(xCoord, zCoord);

                // 2. Apply "Cone" shape to force a central peak
                float distFromCenter = Vector2.Distance(new Vector2(x, z), center);
                float maxDist = width / 2f;
                float mask = 1f - Mathf.Clamp01(distFromCenter / maxDist);

                // 3. Apply Curve (Plateaus)
                // Evaluate the combined noise + mask through your curve
                float finalHeight = heightCurve.Evaluate(noise * mask) * mapConfig.MountainHeight;

                vertices[z * (width + 1) + x] = new Vector3(x, finalHeight, z);
            }
        }

        // Standard Unity Mesh Triangles generation (The boring part)
        int[] triangles = new int[width * height * 6];
        int tris = 0;
        int vert = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Essential for lighting
        GetComponent<MeshFilter>().mesh = mesh;

        // Add a MeshCollider so you can click on it later
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        Debug.Log("Generated!");
    }
}
