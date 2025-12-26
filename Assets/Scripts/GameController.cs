using System.Collections;
using SkiGame.Maps;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    MapConfig _mapConfig;

    [SerializeField]
    MountainGen _mountainGen;

    [SerializeField]
    CameraController _cameraController;

    static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

    MapData _mapData;
    bool _canGenerate = true;

    void Start()
    {
        _mapData = new MapData(_mapConfig);
        _mountainGen.Generate(_mapConfig, _mapData);
    }

    void Update()
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

    IEnumerator ActionRoutine()
    {
        if (!_canGenerate)
            yield break;

        _canGenerate = false;
        yield return _waitForSeconds0_5;
        _canGenerate = true;
    }
}
