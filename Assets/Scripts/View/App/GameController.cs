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
        private MountainGen _mountainGen;

        [SerializeField]
        private CameraController _cameraController;

        private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

        private MapData _mapData;
        private bool _canGenerate = true;

        private void Start()
        {
            _mapData = new MapData(_mapConfig.Width, _mapConfig.Height);
            _mountainGen.Generate(_mapConfig, _mapData);

            GameContext.RegisterMap(_mapData);
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
                _mountainGen.ClearMesh();
                _mountainGen.Generate(_mapConfig, _mapData);
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
    }
}
