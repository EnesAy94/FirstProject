using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CanvasParticles : MonoBehaviour
{
    [Header("Particle Settings")]
    public int particleCount = 20;
    public float minSpeed = 20f;
    public float maxSpeed = 80f;
    public float minSize = 2f;
    public float maxSize = 8f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 0.4f;
    public Color particleColor = Color.cyan;

    [Header("Behavior")]
    public float swayAmount = 20f;
    public float swaySpeed = 1f;

    private RectTransform canvasRect;
    private List<ParticleData> particles = new List<ParticleData>();
    private GameObject particlePrefab;

    private class ParticleData
    {
        public RectTransform rect;
        public Image img;
        public float speed;
        public float swayPhase;
        public float targetAlpha;
        public float originalX;
    }

    void Start()
    {
        canvasRect = GetComponent<RectTransform>();
        
        // Create a simple dot image to use as a prefab
        CreateParticlePrefab();
        
        // Spawn particles
        for (int i = 0; i < particleCount; i++)
        {
            SpawnParticle();
        }
    }

    private void CreateParticlePrefab()
    {
        particlePrefab = new GameObject("ParticleDot");
        particlePrefab.AddComponent<RectTransform>();
        Image img = particlePrefab.AddComponent<Image>();
        img.color = particleColor;
        // Optionally, assign a soft circle sprite here if you have one.
        particlePrefab.SetActive(false);
        particlePrefab.transform.SetParent(transform, false);
    }

    private void SpawnParticle()
    {
        GameObject pObj = Instantiate(particlePrefab, transform);
        pObj.SetActive(true);
        RectTransform r = pObj.GetComponent<RectTransform>();
        Image img = pObj.GetComponent<Image>();

        ParticleData p = new ParticleData { rect = r, img = img };
        ResetParticle(p, true);
        particles.Add(p);
    }

    private void ResetParticle(ParticleData p, bool randomY = false)
    {
        float size = Random.Range(minSize, maxSize);
        p.rect.sizeDelta = new Vector2(size, size);
        
        p.speed = Random.Range(minSpeed, maxSpeed);
        p.swayPhase = Random.Range(0f, Mathf.PI * 2f);
        p.targetAlpha = Random.Range(minAlpha, maxAlpha);

        Color c = particleColor;
        c.a = p.targetAlpha;
        p.img.color = c;

        // Position
        float rectWidth = canvasRect.rect.width;
        float rectHeight = canvasRect.rect.height;

        p.originalX = Random.Range(-rectWidth / 2f, rectWidth / 2f);
        float y = randomY ? Random.Range(-rectHeight / 2f, rectHeight / 2f) : -rectHeight / 2f - size;
        
        p.rect.anchoredPosition = new Vector2(p.originalX, y);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime; // Unscaled so they float even if game is paused
        float rectHeight = canvasRect.rect.height;

        foreach (var p in particles)
        {
            // Move up
            Vector2 pos = p.rect.anchoredPosition;
            pos.y += p.speed * dt;

            // Sway side to side
            p.swayPhase += swaySpeed * dt;
            pos.x = p.originalX + Mathf.Sin(p.swayPhase) * swayAmount;

            p.rect.anchoredPosition = pos;

            // Reset if out of bounds (top)
            if (pos.y > rectHeight / 2f + p.rect.sizeDelta.y)
            {
                ResetParticle(p);
            }
        }
    }
}
