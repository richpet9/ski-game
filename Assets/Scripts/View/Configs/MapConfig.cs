using UnityEngine;

namespace SkiGame.View.Configs
{
    public class MapConfig : ScriptableObject
    {
        [Header("Dimensions")]
        [Range(1, 128)]
        public int Width = 128;

        [Range(1, 128)]
        public int Height = 128;

        [Header("Generation")]
        public int Seed = 0;

        public bool RandomizeOnGenerate = true;

        [Range(1, 80)]
        public int MountainHeight = 80;

        [Range(0, 20)]
        public int NoiseScale = 2;

        public AnimationCurve HeightCurve;
    }
}
