using UnityEngine;

public class StarryBackground : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.1f;
    private Material backgroundMaterial;
    private Vector2 offset;

    void Start()
    {
        backgroundMaterial = GetComponent<Renderer>().material;
        offset = new Vector2(0, scrollSpeed);
    }

    void Update()
    {
        // Move the texture downwards to simulate star movement
        backgroundMaterial.mainTextureOffset += offset * Time.deltaTime;

        // Reset offset if needed to avoid large values, ensuring smooth infinite scroll
        if (backgroundMaterial.mainTextureOffset.y < -1)
        {
            backgroundMaterial.mainTextureOffset = new Vector2(0, 0);
        }
    }
}

