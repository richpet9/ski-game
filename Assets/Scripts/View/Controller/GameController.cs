using System.Collections;
using SkiGame.Model.Core;
using SkiGame.Model.Services;
using SkiGame.Model.Terrain;
using SkiGame.View.Configs;
using SkiGame.View.Services;
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

        [SerializeField]
        private FoliageView _foliageView;

        private static readonly WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);

        private Map _map;
        private bool _canGenerate = true;

        private void Awake()
        {
            _map = new Map(_mapConfig.Width, _mapConfig.Height);
            TickManager tickManager = new TickManager();

            GameContext.Register(_map);
            GameContext.Register(tickManager);
            GameContext.Register<INavigationService>(new NavigationService());
        }

        private void Start()
        {
            InitializeTerrain();
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
                InitializeTerrain();
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

        private void InitializeTerrain()
        {
            float[] heights = MountainGenerator.GenerateHeights(
                _mapConfig.Width,
                _mapConfig.Height,
                _mapConfig.Seed,
                _mapConfig.NoiseScale,
                _mapConfig.NoiseIntensity,
                _mapConfig.MountainHeight,
                _mapConfig.HeightCurve
            );

            for (int z = 0; z < _mapConfig.Height; z++)
            {
                for (int x = 0; x < _mapConfig.Width; x++)
                {
                    float h1 = heights[z * (_mapConfig.Width + 1) + x];
                    float h2 = heights[z * (_mapConfig.Width + 1) + x + 1];
                    float h3 = heights[(z + 1) * (_mapConfig.Width + 1) + x];
                    float h4 = heights[(z + 1) * (_mapConfig.Width + 1) + x + 1];
                    float avg = (h1 + h2 + h3 + h4) * 0.25f;
                    GameContext.Map.SetTile(x, z, avg);
                }
            }

            _map.ApplyTrees(
                TreesGenerator.GenerateTrees(
                    _mapConfig.Width,
                    _mapConfig.Height,
                    heights,
                    _mapConfig.MountainHeight,
                    _mapConfig.Seed,
                    _mapConfig.ForestDensity,
                    _mapConfig.ForestNoiseScale,
                    _mapConfig.TreeLinePercent
                )
            );

            _foliageView.Initialize(_map, _mapConfig.TreeScale);
            _terrainView.Render(
                MountainGenerator.GenerateMeshData(_mapConfig.Width, _mapConfig.Height, heights)
            );
        }
    }
}
