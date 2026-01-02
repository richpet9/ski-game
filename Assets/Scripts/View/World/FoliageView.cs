using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using SkiGame.View.Configs;
using UnityEngine;

namespace SkiGame.View.World
{
    public class FoliageView : MonoBehaviour
    {
        [Header("Assets")]
        public Mesh TreeMesh;
        public Material TreeMaterial;

        private const int BATCH_SIZE = 1023;

        private Map _map;
        private float _treeScale;
        private readonly List<Matrix4x4> _treeMatrices = new List<Matrix4x4>();

        public void Initialize(Map map, float treeScale)
        {
            _map = map;
            _treeScale = treeScale;
            Refresh();
        }

        public void Refresh()
        {
            _treeMatrices.Clear();

            if (_map == null)
            {
                Debug.LogError("[FoliageView] Map is null!");
                return;
            }

            // Iterate over the map size (hardcoded to 128 for now, ideally passed in)
            for (int x = 0; x < 128; x++)
            {
                for (int z = 0; z < 128; z++)
                {
                    TileData tile = _map.GetTile(x, z);

                    // Check if this tile has a tree
                    if (tile.Structure == StructureType.Tree)
                    {
                        Vector3 position = new Vector3(x, tile.Height, z);
                        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        Vector3 scale = _treeScale * Random.Range(0.8f, 1.2f) * Vector3.one;

                        _treeMatrices.Add(Matrix4x4.TRS(position, rotation, scale));
                    }
                }
            }

            Debug.Log($"[FoliageView] Generated {_treeMatrices.Count} trees.");
        }

        private void Update()
        {
            if (_treeMatrices.Count == 0)
                return;

            if (TreeMesh == null || TreeMaterial == null)
            {
                Debug.LogWarning("[FoliageView] Mesh or Material is missing!");
                return;
            }

            for (int i = 0; i < _treeMatrices.Count; i += BATCH_SIZE)
            {
                int count = Mathf.Min(BATCH_SIZE, _treeMatrices.Count - i);
                Graphics.DrawMeshInstanced(
                    TreeMesh,
                    0,
                    TreeMaterial,
                    _treeMatrices.GetRange(i, count),
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true
                );
            }
        }
    }
}
