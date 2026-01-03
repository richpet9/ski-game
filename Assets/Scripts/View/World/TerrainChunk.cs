using SkiGame.Model.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class TerrainChunk : MonoBehaviour
    {
        private const float OCCLUSION_CHECK_INTERVAL = 0.2f;

        // Static plane array to avoid per-chunk allocations.
        private static readonly Plane[] _frustumPlanes = new Plane[6];
        private static int _lastPlaneUpdateFrame = -1;

        private MeshFilter _visualFilter;
        private MeshRenderer _visualRenderer;
        private MeshCollider _physicsCollider;

        private Map _map;
        private int _startX;
        private int _startZ;
        private int _chunkSize;
        private int _terrainLayer;

        private readonly Vector3[] _boundCorners = new Vector3[5];
        private float _nextOcclusionCheckTime;
        private bool _isOccluded;

        private void Awake()
        {
            _visualFilter = GetComponent<MeshFilter>();
            _visualRenderer = GetComponent<MeshRenderer>();
            _physicsCollider = GetComponent<MeshCollider>();
        }

        public void Initialize(Map map, int startX, int startZ, int size, int terrainLayer)
        {
            _map = map;
            _startX = startX;
            _startZ = startZ;
            _chunkSize = size;
            _terrainLayer = terrainLayer;
            gameObject.layer = terrainLayer;

            Rebuild();
        }

        public void Rebuild()
        {
            // 1. Visual Mesh (Voxels).
            MeshData visualData = TerrainMeshVoxelBuilder.Build(_map, _startX, _startZ, _chunkSize);
            Mesh visualMesh = new Mesh
            {
                name = $"Chunk_Visual_{_startX}_{_startZ}",
                indexFormat = IndexFormat.UInt32,
            };

            visualMesh.SetVertices(visualData.Vertices);
            visualMesh.SetTriangles(visualData.Triangles, 0);
            visualMesh.SetColors(visualData.Colors);
            visualMesh.SetUVs(0, visualData.UVs);
            visualMesh.RecalculateNormals();
            visualMesh.RecalculateBounds();

            _visualFilter.mesh = visualMesh;

            // 2. Physics Mesh (Smooth).
            MeshData physicsData = TerrainMeshPhysicsBuilder.Build(
                _map,
                _startX,
                _startZ,
                _chunkSize
            );
            Mesh physicsMesh = new Mesh
            {
                name = $"Chunk_Physics_{_startX}_{_startZ}",
                vertices = physicsData.Vertices,
                triangles = physicsData.Triangles,
            };

            physicsMesh.RecalculateNormals();
            _physicsCollider.sharedMesh = physicsMesh;
        }

        private void Update()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            if (_lastPlaneUpdateFrame != Time.frameCount)
            {
                GeometryUtility.CalculateFrustumPlanes(mainCamera, _frustumPlanes);
                _lastPlaneUpdateFrame = Time.frameCount;
            }

            // 1. Frustum Check (Always first - fastest).
            if (!GeometryUtility.TestPlanesAABB(_frustumPlanes, _visualRenderer.bounds))
            {
                _visualRenderer.enabled = false;
                return;
            }

            // 2. Occlusion Check.
            UpdateOcclusionState(mainCamera);
            _visualRenderer.enabled = !_isOccluded;
        }

        private void UpdateOcclusionState(Camera camera)
        {
            if (Time.time < _nextOcclusionCheckTime)
            {
                return;
            }

            _nextOcclusionCheckTime = Time.time + OCCLUSION_CHECK_INTERVAL;

            Bounds bounds = _visualRenderer.bounds;
            Vector3 camPos = camera.transform.position;

            // Setup points to check: Center and 4 corners of the top face.
            _boundCorners[0] = bounds.center;
            _boundCorners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            _boundCorners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            _boundCorners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            _boundCorners[4] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

            bool anyPointVisible = false;

            for (int i = 0; i < _boundCorners.Length; i++)
            {
                Vector3 target = _boundCorners[i];
                Vector3 direction = target - camPos;
                float distance = direction.magnitude;

                // If a ray hits NOTHING on the terrain layer, or hits something very close to the target,
                // then this part of the chunk is visible.
                if (
                    !Physics.Raycast(
                        camPos,
                        direction.normalized,
                        out RaycastHit hit,
                        distance,
                        1 << _terrainLayer
                    )
                )
                {
                    anyPointVisible = true;
                    break;
                }

                // Allow a small buffer (2 units) to account for hitting parts of the same chunk.
                if (hit.distance >= distance - 2f)
                {
                    anyPointVisible = true;
                    break;
                }
            }

            _isOccluded = !anyPointVisible;
        }
    }
}
