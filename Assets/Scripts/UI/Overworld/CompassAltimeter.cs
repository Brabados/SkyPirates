using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompassAltimeter : MonoBehaviour
{
    [Header("Compass Settings")]
    public Transform player;
    public RectTransform compassNeedle;
    public TextMeshProUGUI headingText;
    public TextMeshProUGUI cardinalText;
    public float smoothSpeed = 5f;

    [Header("Altimeter Settings")]
    public TextMeshProUGUI altitudeMetersText;
    public TextMeshProUGUI altitudeFeetText;
    public Image altitudeBar;
    public RectTransform altimeterNeedle;
    public float maxAltitude = 1000f;
    public float needleMinAngle = -135f;
    public float needleMaxAngle = 135f;

    private float currentHeading;
    private float targetHeading;
    private float currentAltitude;

    void Start()
    {
        if (player == null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        UpdateCompass();
        UpdateAltimeter();
    }

    void UpdateCompass()
    {
        // Use player Y rotation
        targetHeading = player.eulerAngles.y;

        // Smooth rotation
        currentHeading = Mathf.LerpAngle(currentHeading, targetHeading, Time.deltaTime * smoothSpeed);

        // Rotate compass needle
        if (compassNeedle != null)
        {
            compassNeedle.localEulerAngles = new Vector3(0, 0, -currentHeading);
        }

        // Update heading text
        if (headingText != null)
        {
            headingText.text = Mathf.RoundToInt(currentHeading).ToString() + "°";
        }

        // Update cardinal direction
        if (cardinalText != null)
        {
            cardinalText.text = GetCardinalDirection(currentHeading);
        }
    }

    void UpdateAltimeter()
    {
        // Use player Y position
        currentAltitude = player.position.y;

        // Update altitude texts
        if (altitudeMetersText != null)
        {
            altitudeMetersText.text = Mathf.RoundToInt(currentAltitude).ToString() + "m";
        }

        if (altitudeFeetText != null)
        {
            float feet = currentAltitude * 3.28084f;
            altitudeFeetText.text = Mathf.RoundToInt(feet).ToString() + " ft";
        }

        // Update altitude bar
        if (altitudeBar != null)
        {
            float fillAmount = Mathf.Clamp01(currentAltitude / maxAltitude);
            altitudeBar.fillAmount = fillAmount;
        }

        // Update altimeter needle (analog meter)
        if (altimeterNeedle != null)
        {
            float normalizedAltitude = Mathf.Clamp01(currentAltitude / maxAltitude);
            float needleAngle = Mathf.Lerp(needleMinAngle, needleMaxAngle, normalizedAltitude);
            altimeterNeedle.localEulerAngles = new Vector3(0, 0, needleAngle);
        }
    }

    string GetCardinalDirection(float heading)
    {
        string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        int index = Mathf.RoundToInt(heading / 45f) % 8;
        return directions[index];
    }
}
