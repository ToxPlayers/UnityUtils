using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule.Util;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
[Serializable]
public class BlitRequest
{
    public string Name = "NewBlitPass";
    public Material Material;
    public RenderPassEvent PassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public void AddBlit(RenderGraph graph, TextureHandle src, TextureHandle dst)
    { 
        RenderGraphUtils.BlitMaterialParameters para = new(src, dst, Material, 0);
        graph.AddBlitPass(para, passName: "Blit-" + Name);
    } 
}

[Serializable]
public class BlitInjection
{
    public BlitRequest Blit = new();
    public CameraType CamType = CameraType.Game | CameraType.SceneView;
    BlitRenderPass _pass;

    public void Inject()
	{
		if (!Blit.Material)
			throw new InvalidOperationException("Blit material not set");
		RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
	}
    public void UnInject() => RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
    void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (CamType.HasFlag(camera.cameraType) && camera.TryGetComponent(out UniversalAdditionalCameraData data) )
        {
            _pass ??= new BlitRenderPass(Blit.Name, RenderPassEvent.AfterRenderingPostProcessing, Blit);
            data.scriptableRenderer.EnqueuePass(_pass);
        }
    }
}


public class MultiBlitterRendererFeature : ScriptableRendererFeature
{
    static public MultiBlitterRendererFeature Instance { get; private set; }
    Dictionary<RenderPassEvent, MultiBlitRenderPass> _passEventToRenderPass = new();
    [ShowInInspector, ReadOnly]
    List<MultiBlitRenderPass> _passes = new();

    [SerializeField] List<BlitRequest> _allBlitRequests = new();

    private void OnValidate()
    {
        Instance = this;
        _passEventToRenderPass.Clear();
        _passes.Clear();
        Create();
    }

    public override void Create()
    {
        foreach (var blit in _allBlitRequests)
            AddToRenderPass(blit);
    }

    void AddToRenderPass(BlitRequest req)
    {
        var renderEvent = req.PassEvent;
        var hasPass = _passEventToRenderPass.TryGetValue(renderEvent, out MultiBlitRenderPass pass);
        if (!hasPass)
        {
            pass = new MultiBlitRenderPass("Multi-Blitter Feature", renderEvent);
            _passes.Add(pass);
            _passEventToRenderPass.Add(renderEvent, pass);
        }
        pass.Blits.Add(req);
    }

    public void AddBlitRequest(BlitRequest req)
    {
        AddToRenderPass(req);
        _allBlitRequests.Add(req);
    }

    public void RemoveRequest(BlitRequest req)
    {
        _allBlitRequests.Remove(req);
        if (_passEventToRenderPass.TryGetValue(req.PassEvent, out MultiBlitRenderPass renderPass))
            renderPass.Blits.Remove(req);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;
        foreach (var pass in _passes)
            renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _allBlitRequests.Clear();
        _passes.Clear();
    }
}

[Serializable]
class BlitRenderPass : ScriptableRenderPass
{
    public BlitRequest BlitReq = new();
    public BlitRenderPass(string name, RenderPassEvent passEvent, BlitRequest req)
    {
        profilingSampler = new ProfilingSampler(name);
        BlitReq = req;
        renderPassEvent = passEvent;
        requiresIntermediateTexture = true;
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
    { 
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError("Blit requires intermidate color texture");
            return;
        }

        var source = resourceData.activeColorTexture;
        var destDescriptor = graph.GetTextureDesc(source);
        destDescriptor.name = $"CameraTexture-{passName}";
        destDescriptor.clearBuffer = false;
        var dest1 = graph.CreateTexture(destDescriptor);
        var lastWritten = dest1; 
        BlitReq.AddBlit(graph, source, dest1); 

        resourceData.cameraColor = lastWritten;
    }

    
}
[Serializable]
class MultiBlitRenderPass : ScriptableRenderPass
{ 
    public List<BlitRequest> Blits = new();
    public MultiBlitRenderPass(string name, RenderPassEvent passEvent)
    {
        profilingSampler = new ProfilingSampler(name);
        Blits = new();
        renderPassEvent = passEvent;
        requiresIntermediateTexture = true;
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
    {
        if (Blits.Count == 0)
            return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError("MultiBlit requires intermidate color texture");
            return;
        }

        var source = resourceData.activeColorTexture;
        var destDescriptor = graph.GetTextureDesc(source);
        destDescriptor.name = $"CameraTexture-{passName}";
        destDescriptor.clearBuffer = false;
        var dest1 = graph.CreateTexture(destDescriptor);
        var lastWritten = dest1;
        for (int i = 0; i < Blits.Count; i++)
        {
            Blits[i].AddBlit(graph, source, dest1);
            lastWritten = dest1;
            (dest1, source) = (source, dest1);
        }

        resourceData.cameraColor = lastWritten;
    } 
}