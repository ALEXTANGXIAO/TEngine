using System;
using UnityEngine;
using System.Collections;
using System.IO;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/*
 * copy from https://github.com/keijiro/RippleEffect
 */
public class RippleEffect
{
    internal class Droplet
    {
        public Vector4 MakeShaderParameter = new Vector4(0, 0, 1000, 0);
        public float aspect = 1f;

        public void Reset()
        {
            MakeShaderParameter.x = Random.value;
            MakeShaderParameter.y = Random.value * aspect;
            MakeShaderParameter.z = 0;
        }

        public void Update()
        {
            MakeShaderParameter.z += Time.deltaTime;
        }
    }

    private float refractionStrength = 0.3f;
    private float reflectionStrength = 0.5f;
    private Color reflectionColor = Color.grey;
    private float waveSpeed = 1.3f;

    private float dropInterval = 1.6f;
    private float timer = 0;
    private int dropCount = 0;
    private float aspect = 1;

    private readonly float changeGradTexTime = 10f;
    private float changeGradTexTimer = 0;
    private int imageWidth = 0;

    private AnimationCurve waveform = null;
    private Droplet[] droplets = null;
    private Texture2D gradTexture = null;
    private Material material = null;

    void UpdateShaderParameters()
    {
        material.SetVector("_Drop1", droplets[0].MakeShaderParameter);
        material.SetVector("_Drop2", droplets[1].MakeShaderParameter);
        material.SetVector("_Drop3", droplets[2].MakeShaderParameter);
    }

    private void ChangeGradTexture()
    {
        if (material == null)
        {
            return;
        }

        waveform = new AnimationCurve(
            new Keyframe(0.00f, Random.Range(0.5f, 1.5f) * 0.50f),
            new Keyframe(0.05f, Random.Range(0.5f, 1.5f) * 1.00f),
            new Keyframe(0.15f, Random.Range(0.5f, 1.5f) * 0.10f),
            new Keyframe(0.25f, Random.Range(0.5f, 1.5f) * 0.80f),
            new Keyframe(0.35f, Random.Range(0.5f, 1.5f) * 0.30f),
            new Keyframe(0.45f, Random.Range(0.5f, 1.5f) * 0.60f),
            new Keyframe(0.55f, Random.Range(0.5f, 1.5f) * 0.40f),
            new Keyframe(0.65f, Random.Range(0.5f, 1.5f) * 0.55f),
            new Keyframe(0.75f, Random.Range(0.5f, 1.5f) * 0.46f),
            new Keyframe(0.85f, Random.Range(0.5f, 1.5f) * 0.52f),
            new Keyframe(0.99f, Random.Range(0.5f, 1.5f) * 0.50f)
        );

        if (gradTexture != null)
        {
            Object.DestroyImmediate(gradTexture);
        }

        gradTexture = new Texture2D(imageWidth, 1, TextureFormat.Alpha8, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        for (var i = 0; i < gradTexture.width; i++)
        {
            var a = waveform.Evaluate(1.0f / gradTexture.width * i);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }

        gradTexture.Apply();

        material.SetTexture("_GradTex", gradTexture);
    }

    public void Init(Material _material, float _imgWidth, float _imgHeight)
    {
        imageWidth = (int) Math.Max(_imgWidth, 1);
        aspect = _imgWidth / _imgHeight;
        droplets = new Droplet[3];
        droplets[0] = new Droplet
        {
            aspect = aspect
        };
        droplets[1] = new Droplet
        {
            aspect = aspect
        };
        droplets[2] = new Droplet
        {
            aspect = aspect
        };

        refractionStrength *= Random.Range(0.5f, 1.5f);
        reflectionStrength *= Random.Range(0.5f, 1.5f);
        reflectionColor *= Random.Range(0.5f, 1.5f);
        waveSpeed *= Random.Range(0.5f, 1.5f);

        material = _material;
        material.hideFlags = HideFlags.DontSave;
        material.SetColor("_Reflection", reflectionColor);
        material.SetVector("_Params1", new Vector4(aspect, 1, 1 / waveSpeed, 0));
        material.SetVector("_Params2", new Vector4(1, 1 / aspect, refractionStrength, reflectionStrength));

        ChangeGradTexture();
        UpdateShaderParameters();
    }

    public void Destroy()
    {
        Object.DestroyImmediate(gradTexture);
    }

    public void Update()
    {
        if (dropInterval > 0)
        {
            timer += Time.deltaTime;
            while (timer > dropInterval)
            {
                droplets[dropCount++ % droplets.Length].Reset();
                timer -= dropInterval;

                if (dropCount > int.MaxValue / 2)
                {
                    dropCount = 0;
                }
            }
        }

        foreach (var d in droplets)
        {
            d.Update();
        }

        changeGradTexTimer += Time.deltaTime;
        if (changeGradTexTime < changeGradTexTimer)
        {
            changeGradTexTimer = 0;
            ChangeGradTexture();
        }

        UpdateShaderParameters();
    }
}
