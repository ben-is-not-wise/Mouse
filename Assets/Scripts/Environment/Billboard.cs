
using HackedDesign;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Billboard : MonoBehaviour
{
    private const float defaultChance = 0.75f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public List<Sprite> billboards;
    [SerializeField] private bool change = false;
    [SerializeField] private float changeTimeMin = 10;
    [SerializeField] protected float changeTimeMax = 40;
    private readonly float billboardChance = defaultChance;

    private float timer; 

    //private SpriteRenderer spriteRenderer;
    private Light2D light2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        this.AutoBind(ref spriteRenderer);
        light2D = GetComponentInChildren<Light2D>();
    }
    void Start()
    {
        if (Random.value > billboardChance || billboards.Count == 0)
        {
            spriteRenderer.enabled = false;
            if (light2D != null)
            {
                light2D.intensity = 0;
                light2D.enabled = false;
                light2D.gameObject.SetActive(false);
            }

            return;
        }
        else
        {
            if(light2D != null)
            {
                light2D.gameObject.SetActive(true);
            }

            SetSprite();
        }

    }

    void SetSprite()
    {
        var sprite = billboards[Random.Range(0, billboards.Count)];

        if (sprite != null)
        {
            spriteRenderer.sprite = billboards[Random.Range(0, billboards.Count)];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(spriteRenderer.enabled && change)
        {
            if(Time.time > timer)
            {
                SetSprite();
                timer = Time.time + Random.Range(changeTimeMin, changeTimeMax);
            }
        }   
    }
}
