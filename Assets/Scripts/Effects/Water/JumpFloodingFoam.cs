using UnityEngine;
using UnityEngine.Rendering;

public class JumpFloodingFoam : MonoBehaviour
{
    [Header("Settings")]
    public Material jumpFloodMaterial;
    [Range(64, 512)] public int resolution = 256;

    [Header("Update Optimization")]
    [Tooltip("Minimum distance camera must move to trigger update")]
    [Range(0.01f, 1f)] public float positionThreshold = 0.1f;
    [Tooltip("Minimum angle camera must rotate to trigger update (degrees)")]
    [Range(0.1f, 5f)] public float rotationThreshold = 1f;
    [Tooltip("Force update every frame (disable optimization)")]
    public bool alwaysUpdate = false;

    private RenderTexture seedTexture;
    private RenderTexture[] pingPongTextures = new RenderTexture[2];
    private int currentSource = 0;

    private static readonly int DistanceFieldTexID = Shader.PropertyToID("_WaterDistanceField");
    private static readonly int StepSizeID = Shader.PropertyToID("_StepSize");
    private static readonly int SourceTexID = Shader.PropertyToID("_SourceTex");

    private Camera mainCamera;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private bool needsUpdate = true;

    void Start()
    {
        mainCamera = Camera.main;
        CreateTextures();
    }

    void OnEnable()
    {
        if (mainCamera != null)
        {
            Camera.onPreRender += OnCameraPreRender;
            lastCameraPosition = mainCamera.transform.position;
            lastCameraRotation = mainCamera.transform.rotation;
            needsUpdate = true;
        }
    }

    void OnDisable()
    {
        Camera.onPreRender -= OnCameraPreRender;
    }

    void OnDestroy()
    {
        ReleaseTextures();
    }

    private void CreateTextures()
    {
        ReleaseTextures();

        seedTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RGHalf);
        seedTexture.filterMode = FilterMode.Bilinear;
        seedTexture.wrapMode = TextureWrapMode.Clamp;
        seedTexture.name = "_SeedTexture";
        seedTexture.Create();

        for (int i = 0; i < 2; i++)
        {
            pingPongTextures[i] = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RGHalf);
            pingPongTextures[i].filterMode = FilterMode.Bilinear;
            pingPongTextures[i].wrapMode = TextureWrapMode.Clamp;
            pingPongTextures[i].name = "_JFPingPong" + i;
            pingPongTextures[i].Create();
        }
    }

    private void ReleaseTextures()
    {
        if (seedTexture != null)
        {
            seedTexture.Release();
            DestroyImmediate(seedTexture);
            seedTexture = null;
        }

        for (int i = 0; i < 2; i++)
        {
            if (pingPongTextures[i] != null)
            {
                pingPongTextures[i].Release();
                DestroyImmediate(pingPongTextures[i]);
                pingPongTextures[i] = null;
            }
        }
    }

    private void OnCameraPreRender(Camera cam)
    {
        if (cam != mainCamera || jumpFloodMaterial == null || seedTexture == null)
            return;

        // Check if update is needed
        bool shouldUpdate = alwaysUpdate || needsUpdate;

        if (!shouldUpdate)
        {
            // Check if camera moved significantly
            float positionDelta = Vector3.Distance(cam.transform.position, lastCameraPosition);
            float rotationDelta = Quaternion.Angle(cam.transform.rotation, lastCameraRotation);

            shouldUpdate = positionDelta > positionThreshold || rotationDelta > rotationThreshold;
        }

        if (shouldUpdate)
        {
            ExecuteJumpFlooding();
            lastCameraPosition = cam.transform.position;
            lastCameraRotation = cam.transform.rotation;
            needsUpdate = false;
        }
    }

    private void ExecuteJumpFlooding()
    {
        // Pass 0: Seed (generate initial distance field from depth discontinuities)
        Graphics.Blit(null, seedTexture, jumpFloodMaterial, 0);

        // Calculate number of passes needed (log2 of resolution)
        int numPasses = Mathf.CeilToInt(Mathf.Log(resolution, 2));
        currentSource = 0;

        // First pass uses seed texture directly
        int stepSize = (int)Mathf.Pow(2, numPasses - 1);
        jumpFloodMaterial.SetInt(StepSizeID, stepSize);
        Shader.SetGlobalTexture(SourceTexID, seedTexture);
        Graphics.Blit(seedTexture, pingPongTextures[0], jumpFloodMaterial, 1);

        // Remaining passes with ping-pong buffers
        for (int i = 1; i < numPasses; i++)
        {
            stepSize = (int)Mathf.Pow(2, numPasses - i - 1);
            jumpFloodMaterial.SetInt(StepSizeID, stepSize);

            int source = currentSource;
            int dest = 1 - currentSource;

            Shader.SetGlobalTexture(SourceTexID, pingPongTextures[source]);
            Graphics.Blit(pingPongTextures[source], pingPongTextures[dest], jumpFloodMaterial, 1);

            currentSource = dest;
        }

        // Set the final distance field as a global texture for the water shader
        Shader.SetGlobalTexture(DistanceFieldTexID, pingPongTextures[currentSource]);
    }

    void OnValidate()
    {
        // Recreate textures if resolution changes
        if (seedTexture != null && seedTexture.width != resolution)
        {
            CreateTextures();
            needsUpdate = true;
        }
    }

    /// <summary>
    /// Force an update on the next frame (useful if you know objects have moved)
    /// </summary>
    public void ForceUpdate()
    {
        needsUpdate = true;
    }
}
