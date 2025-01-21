using System;
using System.Collections.Generic;
using FlaxEngine;
namespace Game;

[ExecuteInEditMode]
public partial class Terrain : Script
{
    private Int2 size;
    public MaterialInstance material;
    public static Terrain Instance { get; private set; }
    public const int ChunkSize = 64;

    public float Scale = 1.0f;
    [NoSerialize]public Chunk[] Chunks = [];

    public Color BrushColor;
    public bool RemoveObjects;
    public float BrushSize = 10.0f;
    public float BrushStrength = 1.0f;
    public float BrushFalloff = 0.5f;
    public float BrushMinDistance = 10.0f;
    public Float2 BrushPosition;
    public Int2 Size { get =>size; set => Resize(value); }
    private bool isDirty;
    public bool testNoseHeightMap;

    public override void OnAwake() => Instance = this;
    public override void OnStart()
    {
        material = Content.CreateVirtualAsset<MaterialInstance>();
        material.BaseMaterial = Content.Load<Material>(Globals.ProjectContentFolder + "\\TerrainMasterMat.flax");
        Resize(Size);
        isDirty = true;
    }
    public void InitBrushSettingsUI()
    {
        Utility.UI.TitleProperty(UIRoot.TerainBrush, "Brush Settings");
        Utility.UI.CheakBoxProperty(UIRoot.TerainBrush, "Remove Objects", (bool b) =>
        {
            RemoveObjects = b;
        });
        Utility.UI.FloatProperty(UIRoot.TerainBrush, "Size", (float value) => { BrushSize = value; }, BrushSize, true, 1f, 100.0f);
        Utility.UI.FloatProperty(UIRoot.TerainBrush, "Strength", (float value) => { BrushStrength = value; }, BrushStrength, true, 0.01f, 1.0f);
        Utility.UI.FloatProperty(UIRoot.TerainBrush, "Falloff", (float value) => { BrushFalloff = value; }, BrushFalloff, true, 0.0f, 1.0f);
        Utility.UI.FloatProperty(UIRoot.TerainBrush, "Min Distance", (float value) => { BrushMinDistance = value; }, BrushMinDistance, true, 0.0f, 50.0f);
    }
    public override void OnDebugDraw()
    {
        if (Chunks == null) return;

        for (int i = 0; i < Chunks.Length; i++)
        {
            if (Chunks[i] == null) continue;
            Chunks[i].DDraw();
        }
        base.OnDebugDrawSelected();
    }
    public override void OnUpdate()
    {
        BuildMesh();

        BrushColor = RemoveObjects ? Color.Red : Color.LimeGreen;

        material?.SetParameterValue("BrushLocation", BrushPosition);
        material?.SetParameterValue("BrushColor", BrushColor);
        material?.SetParameterValue("BrushSize", BrushSize);
        material?.SetParameterValue("BrushStrength", BrushStrength);
        material?.SetParameterValue("BrushFalloff", BrushFalloff);
        //material?.SetParameterValue("Scale", Scale);
    }
    public void Resize(Int2 newsize)
    {
        size = newsize;
        var chunks = new Chunk[Size.X * Size.Y];
        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                bool add = true;
                for (int i = 0; i < Chunks.Length; i++)
                {
                    if (Chunks[i].Position.X == x && Chunks[i].Position.Y == y)
                    {
                        chunks[y * Size.X + x] = Chunks[i];
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    var c = new Chunk(this, new Int2(x, y));
                    c.BuildHeightMap();
                    chunks[y * Size.X + x] = c;
                }
                chunks[y * Size.X + x].UpdateScale();
            }
        }
        Chunks = chunks;
    }


    private bool IsRuning = false;

    public void BuildMesh()
    {
        if (!isDirty)
            return;
        if (IsRuning)
            return;
        Actor.DestroyChildren();
        for (int i = 0; i < Chunks.Length; i++)
        {
            StaticModel staticModel = Actor.AddChild<StaticModel>();
            var c = Chunks[i];

            staticModel.Position = new Vector3(c.Position.X, 0, c.Position.Y) * ChunkSize;
            staticModel.Scale = new Vector3(1, 1, 1);


            staticModel.AddChild<MeshCollider>();
            var cargo = staticModel.AddChild<EmptyActor>();
            cargo.Name = "Asset Cargo";
            Chunks[i].objectsCargo = cargo;


        }
        IsRuning = true;

        JobSystem.Dispatch((int __) =>
        {
            for (int i = 0; i < Chunks.Length; i++)
        {
            if (Chunks[i].IsDirty)
                Chunks[i].GenerateMesh(Actor.GetChild(i).As<StaticModel>());
        }
        IsRuning = false;
        isDirty = false;
        });
    }
    
    public void LoadHeightMap()
    {
        if (Import.HeightMapTexture == null)
            return;

        var t = Import.HeightMapTexture;
        var w = Mathf.FloorToInt(t.Width);
        var h = Mathf.FloorToInt(t.Height);
        t.GetPixels(out Color[] pixels);

        for (int x = 0; x <= ChunkSize * Size.X; x++)
        {
            for (int y = 0; y <= ChunkSize * Size.Y; y++)
            {
                SetHeight(x, y, pixels[y * h + x].R * 255);
            }
        }
        BuildMesh();
    }
    internal void SetColorMap()
    {
        if (Import.ColorMapTexture == null)
        {
            material.SetParameterValue("ColorMap", Content.Load<Texture>(Globals.ProjectContentFolder + "\\NoColorMap.flax"));
        }
        else
        {
            material.SetParameterValue("ColorMap", Import.ColorMapTexture);
        }
        for (int i = 0; i < Actor.Children.Length; i++)
        {
            if (Actor.Children[i] is StaticModel m)
            {
                if (m.Model)
                    m.Model.MaterialSlots[0].Material = material;
            }
        }

    }

    public float WorldGetHeight(float x, float y)
    {
        x = Mathf.Clamp(x, 0, (Size.X-1) * ChunkSize);
        y = Mathf.Clamp(y, 0, (Size.Y-1) * ChunkSize);


        return GetHeight(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }
    public Chunk WorldGetChunk(float x, float y)
    {
        x = Mathf.Clamp(x, 0, (Size.X - 1) * ChunkSize);
        y = Mathf.Clamp(y, 0, (Size.Y - 1) * ChunkSize);

        var ChunkCordsX = Mathf.Max(Mathf.FloorToInt(((float)x - 0.5f) / ChunkSize), 0);
        var ChunkCordsY = Mathf.Max(Mathf.FloorToInt(((float)y - 0.5f) / ChunkSize), 0);
        return Chunks[ChunkCordsY * Size.X + ChunkCordsX];
    }

    public float GetHeight(int x, int y)
    {
        var ChunkCordsX = Mathf.Max(Mathf.FloorToInt(((float)x - 0.5f) / ChunkSize), 0);
        var ChunkCordsY = Mathf.Max(Mathf.FloorToInt(((float)y - 0.5f) / ChunkSize), 0);
        var CordsInsideTheChunkX = x - (ChunkCordsX * ChunkSize);
        var CordsInsideTheChunkY = y - (ChunkCordsY * ChunkSize);
        var chunk = Chunks[ChunkCordsY * Size.X + ChunkCordsX];
        return chunk.GetHeight(CordsInsideTheChunkX, CordsInsideTheChunkY);
    }
    public void SetHeight(int x, int y, float value)
    {
        var ChunkCordsX = Mathf.Max(Mathf.FloorToInt(((float)x - 0.5f) / ChunkSize), 0);
        var ChunkCordsY = Mathf.Max(Mathf.FloorToInt(((float)y - 0.5f) / ChunkSize), 0);
        var CordsInsideTheChunkX = x - (ChunkCordsX * ChunkSize);
        var CordsInsideTheChunkY = y - (ChunkCordsY * ChunkSize);
        var chunk = Chunks[ChunkCordsY * Size.X + ChunkCordsX];
        chunk.SetHeight(CordsInsideTheChunkX, CordsInsideTheChunkY,value);
        isDirty = true;
    }
    internal List<Chunk> GetChunksWithObjects()
    {
        List<Chunk> list = new List<Chunk>();

        for (int i = 0; i < Chunks.Length; i++)
        {
            if (Chunks[i].objects.Count != 0)
                list.Add(Chunks[i]);
        }
        return list;
    }
}


