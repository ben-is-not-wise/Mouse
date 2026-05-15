
using HackedDesign;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ad : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public List<Sprite> ads;
    [SerializeField] private float changeTime = 1;

    private int index = 0;

    private float timer; 

    //private SpriteRenderer spriteRenderer;
    //private Light2D light2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        this.AutoBind(ref spriteRenderer);
        //light2D = GetComponentInChildren<Light2D>();
    }

    void SetSprite()
    {
        var sprite = ads[index];

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timer)
        {
            SetSprite();
            timer = Time.time + changeTime;
            index++;
            if (index == ads.Count)
            {
                index = 0;
            }
        }
    }
}
