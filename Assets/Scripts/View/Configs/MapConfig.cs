using UnityEngine;

namespace SkiGame.View.Configs
{
    public sealed class MapConfig : ScriptableObject
    {
        [Header("Dimensions")]
        [Range(1, 512)]
        public int Width = 512;

        [Range(1, 512)]
        public int Height = 512;

        [Header("Generation")]
        public int Seed = 0;

        public bool RandomizeOnGenerate = true;

        [Range(1, 80)]
        public int MountainHeight = 80;

        [Header("Noise")]
        [Range(0, 20)]
        public int NoiseScale = 2;

        [Range(0, 1)]
        public float NoiseIntensity = 0.5f;

        public AnimationCurve HeightCurve;

        [Header("Foliage")]
        [Range(0, 1)]
        public float ForestDensity = 0.3f;

        [Range(0, 20)]
        public int ForestNoiseScale = 10;

        [Range(0, 1)]
        public float TreeLinePercent = 0.6f;

        [Range(0.01f, 2f)]
        public float TreeScale = 0.12f;
    }
}
