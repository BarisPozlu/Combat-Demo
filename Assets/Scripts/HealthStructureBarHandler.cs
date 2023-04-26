using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthStructureBarHandler : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject structureBar;

    private Slider healthSlider;
    private Slider structureSlider;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider = healthBar.GetComponent<Slider>();
        structureSlider = structureBar.GetComponent<Slider>();
    }

    public void UpdateHealth(float value)
    {
        healthSlider.value = value;
    }

    public void UpdateStructure(float maxValue, float value)
    {
        structureSlider.value = maxValue - value;
    }

    public void DestroyBars()
    {
        Destroy(healthBar);
        Destroy(structureBar);
    }
}