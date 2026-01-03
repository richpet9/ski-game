using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.World
{
    public class FoliageView : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField]
        private Mesh _treeMesh;

        [SerializeField]
        private Material _treeMaterial;

        private const int BATCH_SIZE = 1023;
        private const float MIN_TREE_SCALE = 0.7f;
        private const float MAX_TREE_SCALE = 1.2f;

        private readonly List<Matrix4x4[]> _batches = new List<Matrix4x4[]>();
        private Map _map;
        private float _treeScale;
        private bool _isDirty;

        public void Initialize(Map map, float treeScale)
        {
            _map = map;
            _treeScale = treeScale;
            _map.OnFoliageChanged += SetDirty;
        }

        private void SetDirty()
        {
            _isDirty = true;
        }

        private void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnFoliageChanged -= SetDirty;
            }
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
            int totalTrees = 0;

            for (int x = 0; x < _map.Width; x++)
            {
                for (int z = 0; z < _map.Height; z++)
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
                        totalTrees++;
                    }
                }
            }

            if (currentBatch.Count > 0)
            {
                _batches.Add(currentBatch.ToArray());
            }

            Debug.Log($"[FoliageView] Generated {totalTrees} trees.");
        }

        private void Update()
        {
            if (_treeMesh == null || _treeMaterial == null)
            {
                return;
            }

            for (int i = 0; i < _batches.Count; i++)
            {
                Graphics.DrawMeshInstanced(
                    _treeMesh,
                    0,
                    _treeMaterial,
                    _batches[i],
                    _batches[i].Length,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true
                );
            }
        }
    }
}
