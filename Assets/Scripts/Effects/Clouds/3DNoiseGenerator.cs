using UnityEngine;

public class NoiseTextureGenerator : MonoBehaviour
{
    [Header("Texture Settings")]
    [SerializeField] private int resolution = 64;
    [SerializeField] private Material cloudMaterial;

    [Header("Base Noise - Large Cloud Structures")]
    [SerializeField] private float baseScale = 1.5f;
    [SerializeField] private int baseOctaves = 4;
    [SerializeField] private float basePersistence = 0.5f;
    [SerializeField] private float baseLacunarity = 2.0f;
    [SerializeField] [Range(0f, 1f)] private float baseContrast = 0.3f;

    [Header("Detail Noise - Edge Erosion")]
    [SerializeField] private float detailScale = 8.0f;
    [SerializeField] private int detailOctaves = 3;
    [SerializeField] private float detailPersistence = 0.5f;
    [SerializeField] private float detailLacunarity = 2.0f;
    [SerializeField] [Range(0f, 1f)] private float detailContrast = 0.2f;

    [Header("Performance")]
    [SerializeField] private bool generateOnStart = true;

    private Texture3D baseNoiseTexture;
    private Texture3D detailNoiseTexture;

    private void Start()
    {
        if (generateOnStart) GenerateTextures();
    }

    [ContextMenu("Generate Textures")]
    public void GenerateTextures()
    {
        baseNoiseTexture = GenerateNoiseTexture(baseScale, baseOctaves, basePersistence, baseLacunarity, baseContrast, 0);
        detailNoiseTexture = GenerateNoiseTexture(detailScale, detailOctaves, detailPersistence, detailLacunarity, detailContrast, 1000);
        ApplyTexturesToMaterial();
    }

    [ContextMenu("Regenerate with Random Seeds")]
    public void RegenerateRandom()
    {
        System.Random rng = new System.Random();
        baseNoiseTexture = GenerateNoiseTexture(baseScale, baseOctaves, basePersistence, baseLacunarity, baseContrast, rng.Next());
        detailNoiseTexture = GenerateNoiseTexture(detailScale, detailOctaves, detailPersistence, detailLacunarity, detailContrast, rng.Next());
        ApplyTexturesToMaterial();
    }

    private void ApplyTexturesToMaterial()
    {
        if (cloudMaterial != null)
        {
            cloudMaterial.SetTexture("_NoiseTex", baseNoiseTexture);
            cloudMaterial.SetTexture("_DetailNoiseTex", detailNoiseTexture);
        }
    }

    private Texture3D GenerateNoiseTexture(float scale, int octaves, float persistence, float lacunarity, float contrast, int seed)
    {
        Texture3D texture = CreateTexture3D();
        Color[] colors = new Color[resolution * resolution * resolution];
        Vector3 offset = GenerateRandomOffset(seed);

        GenerateNoiseValues(colors, scale, octaves, persistence, lacunarity, offset, out float minValue, out float maxValue);
        NormalizeAndApplyContrast(colors, minValue, maxValue, contrast);

        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    private Texture3D CreateTexture3D()
    {
        return new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, true)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 9
        };
    }

    private Vector3 GenerateRandomOffset(int seed)
    {
        System.Random prng = new System.Random(seed);
        return new Vector3(
            (float)prng.NextDouble() * 10000f,
            (float)prng.NextDouble() * 10000f,
            (float)prng.NextDouble() * 10000f
        );
    }

    private void GenerateNoiseValues(Color[] colors, float scale, int octaves, float persistence, float lacunarity,
        Vector3 offset, out float minValue, out float maxValue)
    {
        minValue = float.MaxValue;
        maxValue = float.MinValue;
        float invResolution = 1f / resolution;
        int resolutionSqr = resolution * resolution;

        for (int z = 0; z < resolution; z++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector3 samplePos = new Vector3(
                        x * invResolution * scale + offset.x,
                        y * invResolution * scale + offset.y,
                        z * invResolution * scale + offset.z
                    );

                    float noiseValue = FractalPerlinNoise3D(samplePos, octaves, persistence, lacunarity);
                    int index = x + y * resolution + z * resolutionSqr;
                    colors[index].r = noiseValue;

                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                }
            }
        }
    }

    private void NormalizeAndApplyContrast(Color[] colors, float minValue, float maxValue, float contrast)
    {
        float range = maxValue - minValue;
        float contrastMultiplier = 1f + contrast;

        for (int i = 0; i < colors.Length; i++)
        {
            float normalized = (colors[i].r - minValue) / range;
            float centered = (normalized - 0.5f) * 2f;
            float contrasted = Mathf.Clamp01(centered * contrastMultiplier * 0.5f + 0.5f);
            float smoothed = Mathf.SmoothStep(0f, 1f, contrasted);

            colors[i] = new Color(smoothed, smoothed, smoothed, smoothed);
        }
    }

    private float PerlinNoise3D(Vector3 pos)
    {
        float xy = Mathf.PerlinNoise(pos.x + pos.y * 0.5f, pos.y);
        float xz = Mathf.PerlinNoise(pos.x, pos.z + pos.x * 0.5f);
        float yz = Mathf.PerlinNoise(pos.y + pos.z * 0.5f, pos.z);
        float yx = Mathf.PerlinNoise(pos.y, pos.x + pos.y * 0.5f);
        float zx = Mathf.PerlinNoise(pos.z + pos.x * 0.5f, pos.x);
        float zy = Mathf.PerlinNoise(pos.z, pos.y + pos.z * 0.5f);

        return (xy + xz + yz + yx + zx + zy) * 0.166667f; // Division by 6
    }

    private float FractalPerlinNoise3D(Vector3 pos, int octaves, float persistence, float lacunarity)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total += PerlinNoise3D(pos * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    private void OnDestroy()
    {
        if (baseNoiseTexture != null) Destroy(baseNoiseTexture);
        if (detailNoiseTexture != null) Destroy(detailNoiseTexture);
    }
}
