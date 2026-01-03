using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class FoliageView : MonoBehaviour
    {
        private const int BATCH_SIZE = 1023;
        private const float MIN_TREE_SCALE = 0.7f;
        private const float MAX_TREE_SCALE = 1.2f;

        private readonly List<Matrix4x4[]> _batches = new List<Matrix4x4[]>();
        private Map _map;
        private Vector2Int _startPos;
        private Vector2Int _endPos;
        private Mesh _treeMesh;
        private Material _treeMaterial;
        private float _treeScale;
        private bool _isDirty;

        private MeshRenderer _chunkRenderer;
        private RenderParams _renderParams;
        private Bounds _foliageBounds;

        public void Initialize(
            Map map,
            float treeScale,
            int startX,
            int startZ,
            int chunkSize,
            Mesh treeMesh,
            Material treeMaterial
        )
        {
            _map = map;
            _treeScale = treeScale;
            _startPos = new Vector2Int(startX, startZ);
            _endPos = new Vector2Int(startX + chunkSize, startZ + chunkSize);
            _treeMesh = treeMesh;
            _treeMaterial = treeMaterial;

            _chunkRenderer = GetComponent<MeshRenderer>();
            _renderParams = new RenderParams(_treeMaterial)
            {
                receiveShadows = true,
                shadowCastingMode = ShadowCastingMode.On,
                lightProbeUsage = LightProbeUsage.Off,
            };

            _map.OnFoliageChanged += SetDirty;
            Refresh();
        }

        private void SetDirty()
        {
            _isDirty = true;
        }

        private void LateUpdate()
        {
            if (_isDirty)
            {
                Refresh();
                _isDirty = false;
            }
        }

        public void Refresh()
        {
            _batches.Clear();
            List<Matrix4x4> currentBatch = new List<Matrix4x4>();

            // Track height for bounds calculation.
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            for (int x = _startPos.x; x < _endPos.x; x++)
            {
                for (int z = _startPos.y; z < _endPos.y; z++)
                {
                    TileData tile = _map.GetTile(x, z);

                    if (tile.Structure == StructureType.Tree)
                    {
                        if (currentBatch.Count >= BATCH_SIZE)
                        {
                            _batches.Add(currentBatch.ToArray());
                            currentBatch.Clear();
                        }

                        // NOTE: Consider adding the same noise offset here as used in
                        // TerrainMeshVoxelBuilder to ensure trees don't float/sink
                        // relative to voxel offsets.
                        Vector3 position = MapUtil.GridToWorld(x, z, tile.Height);
                        minHeight = Mathf.Min(minHeight, tile.Height);
                        maxHeight = Mathf.Max(maxHeight, tile.Height);

                        Random.InitState(x * 1000 + z);
                        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        Vector3 scale =
                            _treeScale * Random.Range(MIN_TREE_SCALE, MAX_TREE_SCALE) * Vector3.one;

                        currentBatch.Add(Matrix4x4.TRS(position, rotation, scale));
                    }
                }
            }

            if (currentBatch.Count > 0)
            {
                _batches.Add(currentBatch.ToArray());
            }

            // Calculate precise bounds for culling.
            Vector3 size = new Vector3(
                _endPos.x - _startPos.x,
                maxHeight - minHeight + 5f,
                _endPos.y - _startPos.y
            );
            Vector3 center = new Vector3(
                _startPos.x + size.x / 2f,
                minHeight + size.y / 2f,
                _startPos.y + size.z / 2f
            );
            _foliageBounds = new Bounds(center, size);
            _renderParams.worldBounds = _foliageBounds;
        }

        private void Update()
        {
            if (_treeMesh == null || _treeMaterial == null || _batches.Count == 0)
            {
                return;
            }

            // Mirror the chunk's visibility state.
            if (!_chunkRenderer.enabled)
            {
                return;
            }

            for (int i = 0; i < _batches.Count; i++)
            {
                Graphics.RenderMeshInstanced(
                    _renderParams,
                    _treeMesh,
                    0,
                    _batches[i],
                    _batches[i].Length
                );
            }
        }

        private void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnFoliageChanged -= SetDirty;
            }
        }
    }
}
