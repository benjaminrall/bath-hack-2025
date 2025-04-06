using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Renderer _renderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    public void UpdateMaterials(Material bodyMaterial, Material faceMaterial)
    {
        _renderer.materials = new[] { _renderer.materials[0], bodyMaterial, faceMaterial };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
