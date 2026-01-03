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

        private const int MAX_SEED_VALUE = 100000;

        private static readonly WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);

        private Map _map;
        private bool _canGenerate = true;

        private void Awake()
        {
            // Instantiate the core systems.
            _map = new Map(_mapConfig.Width, _mapConfig.Height);
            TickManager tickManager = new TickManager();
            NavigationService navService = new NavigationService();

            // Initialize the core game systems.
            navService.Initialize(_map);

            GameContext.Register(_map);
            GameContext.Register(tickManager);
            GameContext.Register<INavigationService>(navService);
        }

        private void Start()
        {
            // Generates the initial map on startup.
            GenerateAndBindMap();
        }

        private void Update()
        {
            // Listens for the 'G' key to regenerate the map for debugging purposes.
            if (Input.GetKeyDown(KeyCode.G) && _canGenerate)
            {
                StartCoroutine(RegenerationCooldownRoutine());
                if (_mapConfig.RandomizeOnGenerate)
                {
                    _mapConfig.Seed = Random.Range(0, MAX_SEED_VALUE);
                }
                GenerateAndBindMap();
            }
        }

        private void GenerateAndBindMap()
        {
            // Generates the map data and binds it to the view components.
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
                var rowStartIndex = z * (_mapConfig.Width + 1);
                var nextRowStartIndex = (z + 1) * (_mapConfig.Width + 1);
                for (int x = 0; x < _mapConfig.Width; x++)
                {
                    float h1 = heights[rowStartIndex + x];
                    float h2 = heights[rowStartIndex + x + 1];
                    float h3 = heights[nextRowStartIndex + x];
                    float h4 = heights[nextRowStartIndex + x + 1];
                    float avg = (h1 + h2 + h3 + h4) * 0.25f;
                    _map.SetTile(x, z, avg);
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
            _terrainView.Initialize(_map, _mapConfig.Width, _mapConfig.Height);
        }

        private IEnumerator RegenerationCooldownRoutine()
        {
            if (!_canGenerate)
                yield break;

            _canGenerate = false;
            yield return _waitForSeconds0_5;
            _canGenerate = true;
        }
    }
}
