﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
//[AddComponentMenu("")]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Color Adjustments/Sepia Tone")]
public class DemoShader : MonoBehaviour
{
    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader shader;
    private Material m_Material;

    protected virtual void Start()
    {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!shader || !shader.isSupported)
            enabled = false;
    }

    protected Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    protected virtual void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }

    //void Update()
    //{
    //    if(m_Material)
    //    {
    //        m_Material.SetFloat("_time", Time.timeSinceLevelLoad);
    //    }
    //}

    // Called by camera to apply image effect
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_Material)
        {
            m_Material.SetFloat("_time", Time.timeSinceLevelLoad);
            Debug.Log("setting a shader");
        }

        Graphics.Blit(source, destination, material);
    }
}