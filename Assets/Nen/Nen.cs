using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Nen : MonoBehaviour {

	private RenderTexture m_highlightRT;
	private RenderTargetIdentifier m_rtID;
	private CommandBuffer m_renderBuffer;
	public List<GameObject> highlightObjects;
	public Material _material;
	private Material m_highlightMaterial;

	// Use this for initialization
	void Start () {
		this.CreateBuffers();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CreateBuffers()
    {
        this.m_highlightRT = new RenderTexture(Screen.width, Screen.height, 0);
		this.m_rtID = new RenderTargetIdentifier(m_highlightRT);
		this.m_renderBuffer = new CommandBuffer();
    }

	private void ClearCommandBuffers() {
		this.m_renderBuffer.Clear();
	}

	private void RenderHighlights()
    {
        this.m_renderBuffer.SetRenderTarget(m_rtID);
        
        for (int i = 0; i < this.highlightObjects.Count; i++)
        {
            Renderer renderer = this.highlightObjects[i].GetComponent<Renderer>();
			this.m_renderBuffer.DrawRenderer(renderer, _material, 0, 1);
        }

        RenderTexture.active = m_highlightRT;
        Graphics.ExecuteCommandBuffer(m_renderBuffer);
        RenderTexture.active = null;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        this.ClearCommandBuffers();

        this.RenderHighlights();

        RenderTexture rt1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        //m_blur.OnRenderImage(m_highlightRT, rt1);
        // Excluding the original image from the blurred image, leaving out the areal alone
        m_highlightMaterial.SetTexture("_OccludeMap", m_highlightRT);
        Graphics.Blit(rt1, rt1, m_highlightMaterial, 0);
        // Just combining two textures together
        m_highlightMaterial.SetTexture("_OccludeMap", rt1);
        Graphics.Blit(source, destination, m_highlightMaterial, 1);

        RenderTexture.ReleaseTemporary(rt1);
    }
}
