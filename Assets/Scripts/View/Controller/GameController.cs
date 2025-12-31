using System.Collections;
using SkiGame.Model.Core;
using SkiGame.Model.Economy;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using SkiGame.View.Configs;
using SkiGame.View.World;
using UnityEngine;

namespace SkiGame.View.Controller
{
    [DefaultExecutionOrder(-999999)]
    public class GameController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField]
        private MapConfig _mapConfig;

        [Header("Dependencies")]
        [SerializeField]
        private TerrainView _terrainView;

        private static readonly WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);

        private readonly MountainGenerator _mountainGen = new MountainGenerator();
        private bool _canGenerate = true;

        private void Awake()
        {
            MapManager map = new MapManager(_mapConfig.Width, _mapConfig.Height);
            EconomyManager economy = new EconomyManager();
            StructureManager structures = new StructureManager(map, economy);

            GameContext.Register(map);
            GameContext.Register(economy);
            GameContext.Register(structures);
            economy.AddMoney(200);
        }

        private void Start()
        {
            RenderMountainTerrain();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.G) && _canGenerate)
            {
                StartCoroutine(ActionRoutine());
                if (_mapConfig.RandomizeOnGenerate)
                {
                    _mapConfig.Seed = Random.Range(0, 100000);
                }
                _terrainView.ClearMesh();
                RenderMountainTerrain();
            }
        }

        private IEnumerator ActionRoutine()
        {
            if (!_canGenerate)
                yield break;

            _canGenerate = false;
            yield return _waitForSeconds0_5;
            _canGenerate = true;
        }

        private void RenderMountainTerrain()
        {
            float[] heights = _mountainGen.GenerateHeights(
                _mapConfig.Width,
                _mapConfig.Height,
                _mapConfig.Seed,
                _mapConfig.NoiseScale,
                _mapConfig.NoiseIntensity,
                _mapConfig.MountainHeight,
                _mapConfig.HeightCurve
            );

            for (int i = 0; i < heights.Length; i++)
            {
                int x = i % (_mapConfig.Width + 1);
                int z = i / (_mapConfig.Width + 1);
                GameContext.Map.SetTile(x, z, heights[i]);
            }

            _terrainView.Render(
                _mountainGen.GenerateMeshData(_mapConfig.Width, _mapConfig.Height, heights)
            );
        }
    }
}
