using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.World
{
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
            _map.OnFoliageChanged += SetDirty;
            _treeMesh = treeMesh;
            _treeMaterial = treeMaterial;
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

                        Vector3 position = MapUtil.GridToWorld(x, z, tile.Height);

                        // Use a deterministic random seed for consistent tree scaling
                        // and rotation. This ensures trees don't change scale or
                        // rotation every time Refresh() is called.
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
        }

        private void Update()
        {
            if (_treeMesh == null || _treeMaterial == null)
            {
                return;
            }

            for (int i = 0; i < _batches.Count; i++)
            {
                // public static void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, int layer, Camera camera, LightProbeUsage lightProbeUsage)
                Graphics.DrawMeshInstanced(
                    mesh: _treeMesh,
                    submeshIndex: 0,
                    _treeMaterial,
                    _batches[i],
                    _batches[i].Length,
                    properties: null,
                    castShadows: UnityEngine.Rendering.ShadowCastingMode.On,
                    receiveShadows: true,
                    layer: 0,
                    camera: null,
                    UnityEngine.Rendering.LightProbeUsage.Off // Performance optimization if not needed.
                );
            }
        }
    }
}
