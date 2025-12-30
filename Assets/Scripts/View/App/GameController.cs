using System.Collections;
using SkiGame.Model.Core;
using SkiGame.Model.Terrain;
using SkiGame.View.Data;
using SkiGame.View.World;
using UnityEngine;

namespace SkiGame.View.App
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private MapConfig _mapConfig;

        [SerializeField]
        private TerrainView _terrainView;

        private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

        private readonly MountainGenerator _mountainGen = new MountainGenerator();
        private bool _canGenerate = true;

        private void Start()
        {
            MapData mapData = new MapData(_mapConfig.Width, _mapConfig.Height);
            RenderMountainTerrain();

            GameContext.RegisterMap(mapData);
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
            _terrainView.Render(
                _mountainGen.GenerateMeshData(
                    _mapConfig.Width,
                    _mapConfig.Height,
                    _mountainGen.GenerateHeights(
                        _mapConfig.Width,
                        _mapConfig.Height,
                        _mapConfig.Seed,
                        _mapConfig.NoiseScale,
                        _mapConfig.MountainHeight,
                        _mapConfig.HeightCurve
                    )
                )
            );
        }
    }
}
