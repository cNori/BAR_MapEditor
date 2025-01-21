using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FlaxEngine;
using Newtonsoft.Json.Linq;
using static Game.MapGen;

namespace Game;

[ExecuteInEditMode]
/// <summary>
/// MapGen Script.
/// </summary>
public class MapGen : Script
{
    public class Asset
    {
        public string Path;
        public Model model;
        public Asset(string path, Model model)
        {
            this.Path = path;
            this.model = model;
        }
    }
    public List<Asset> Assets = new List<Asset>();


    public StaticModel staticModel;
    private Texture HeightMap;
    private Texture ColorMap;

    public Int2 MapSize;

    public bool Generate;
    private bool isruning = false;
    private bool isDirty = false;
    //public bool ClearMesh;
    private Float3[] vertices;
    private int[] triangles;

    public Material material;
    public MaterialInstance materialInstance;

    private Color[] pixels;

    //public Chunk[] Chunks;

    public string HeightMapPath;
    public string ColorMapPath;
    public string AssetDirectory;


    public class Object
    {
        public Asset SourceAsset;
        public Float2 Location;
        public float Rotation;
        public StaticModel StaticModelActor;
    }
    public List<MapGen.Object> objects = new List<Object>();
    private Actor AssetContener;
    public void PlaceAsset(Asset asset,Float3 location, float rotation = 0)
    {
        if (AssetContener == null) 
        {
            AssetContener = Actor.GetChild(1);
        }
        StaticModel model = New<StaticModel>();
        objects.Add(new Object() 
        {
            SourceAsset = asset,
            Location = new Float2(location.X,location.Z),
            Rotation = rotation,
            StaticModelActor = model
        });
        model.Model = asset.model;
        model.SetParent(AssetContener,true);
        model.Position = location;
        model.Orientation = Quaternion.Euler(0, rotation, 0);
    }

    public override void OnStart()
    {
#if FLAX_EDITOR
        AssetContener.DestroyChildren();
#endif
        //Import();
    }

    public void Import()
    {
        JobSystem.Dispatch((int __) =>
        {
            if (File.Exists(ColorMapPath))
            {
                ColorMap = Texture.FromFile(ColorMapPath);
            }
            else
            {
                ColorMap = Content.Load<Texture>("Content/NoColorMap.flax");
            }
            materialInstance?.SetParameterValue("ColorMap", ColorMap);
        });
        JobSystem.Dispatch((int __) =>
        {
            if (File.Exists(HeightMapPath))
            {
                HeightMap = Texture.FromFile(HeightMapPath);
            }
            Generate = true;
        });
    }

    public override void OnUpdate()
    {
        if (Generate && !isruning && staticModel != null && material != null)
        {
            Generate = false;

            if (staticModel.Model == null)
            {
                staticModel.Model = Content.CreateVirtualAsset<Model>();
                staticModel.Model.SetupLODs([1]);
            }
            //Chunks = new Chunk[Mathf.FloorToInt((MapSize.X * MapSize.Y) / 64)];
            var teraingen = JobSystem.Dispatch((int __) =>
            {
                isruning = true;
                if (HeightMap != null)
                {
                    MapSize = new Int2((int)HeightMap.Size.X, (int)HeightMap.Size.Y) - 2;
                    HeightMap.GetPixels(out pixels);
                }
                vertices = new Float3[(MapSize.X + 1) * (MapSize.Y + 1)];
                triangles = new int[MapSize.X * MapSize.Y * 6];

                for (int x = 0; x < MapSize.X; x++)
                {
                    for (int y = 0; y < MapSize.Y; y++)
                    {
                        var msize1 = (MapSize.Y + 1);

                        var vertexID = x * msize1 + y;
                        var triangleID = (x * MapSize.Y + y) * 6;

                        triangles[triangleID + 2] = vertexID + MapSize.Y + 1;
                        triangles[triangleID + 1] = vertexID + 1;
                        triangles[triangleID + 0] = vertexID;
                        triangles[triangleID + 5] = vertexID + 1;
                        triangles[triangleID + 4] = vertexID + MapSize.Y + 1;
                        triangles[triangleID + 3] = vertexID + MapSize.Y + 2;


                        vertices[vertexID] = new Float3(x, SampleHeight(x, y), y);
                    }
                }
                //fix edges
                for (int i = 0; i < MapSize.Y; i++)
                {
                    var vertexID = MapSize.X * (MapSize.Y + 1) + i;
                    vertices[vertexID] = new Float3(MapSize.X, SampleHeight(MapSize.X, i), i);
                }
                for (int i = 0; i < MapSize.X; i++)
                {
                    var vertexID = i * (MapSize.Y + 1) + MapSize.Y;
                    vertices[vertexID] = new Float3(i, SampleHeight(i, MapSize.Y), MapSize.Y);
                }

                vertices[vertices.Length - 1] = new Float3(MapSize.X, SampleHeight(MapSize.X, MapSize.Y), MapSize.Y);
                isDirty = true;
                CollisionData data = Actor.GetChild(0).As<MeshCollider>().CollisionData;
                if (Actor.GetChild(0).As<MeshCollider>().CollisionData == null)
                {
                    data = Content.CreateVirtualAsset<CollisionData>();
                    Actor.GetChild(0).As<MeshCollider>().CollisionData = data;
                }
                data.CookCollision(CollisionDataType.TriangleMesh, vertices, triangles);
                isruning = false;
            }, 1);
        }
        if (isDirty)
        {
            Float3[] CalculateNormals()
            {
                Float3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
                {
                    Float3 pointA = vertices[indexA];
                    Float3 pointB = vertices[indexB];
                    Float3 pointC = vertices[indexC];

                    Float3 sideAB = pointB - pointA;
                    Float3 sideAC = pointC - pointA;
                    return Float3.Cross(sideAB, sideAC).Normalized;
                }
                Float3[] vertexNormals = new Float3[vertices.Length];
                int triangleCount = triangles.Length / 3;
                for (int i = 0; i < triangleCount; i++)
                {
                    int normalTriangleIndex = i * 3;
                    int vertexIndexA = triangles[normalTriangleIndex];
                    int vertexIndexB = triangles[normalTriangleIndex + 1];
                    int vertexIndexC = triangles[normalTriangleIndex + 2];

                    Float3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                    vertexNormals[vertexIndexA] += triangleNormal;
                    vertexNormals[vertexIndexB] += triangleNormal;
                    vertexNormals[vertexIndexC] += triangleNormal;
                }

                for (int i = 0; i < vertexNormals.Length; i++)
                {
                    vertexNormals[i].Normalize();
                }

                return vertexNormals;
            }
            staticModel.Model.LODs[0].Meshes[0].UpdateMesh(vertices, triangles, CalculateNormals(), null, null);
            staticModel.Model.SetupMaterialSlots(1);
            materialInstance = Content.CreateVirtualAsset<MaterialInstance>();
            materialInstance.BaseMaterial = material;
            staticModel.Model.MaterialSlots[0].Material = materialInstance;
            materialInstance.SetParameterValue("ColorMap", ColorMap);
            isDirty = false;
        }
        if (vertices != null)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                //DebugDraw.DrawBox(new BoundingBox(-Vector3.One + vertices[i], Vector3.One + vertices[i]), Color.Aqua);
            }
        }
    }
    internal float SampleHeightMapWorld(float x, float y)
    {
        return SampleHeight(Mathf.CeilToInt(x), Mathf.CeilToInt(y));
    }
    internal float SampleHeight(int x, int y)
    {
        if (HeightMap == null || pixels == null)
            return 0;
        x = (int)Mathf.Repeat(x, (int)(HeightMap.Size.X));
        y = (int)Mathf.Repeat(y, (int)(HeightMap.Size.Y));
        return pixels[y * (int)(HeightMap.Size.X) + x].R * 255;
    }

    internal void MaterialSetBrushPosition(float x, float y)
    {
        materialInstance?.SetParameterValue("BrushLocation",new Float2(x,y));
    }
    internal void MaterialSetBrushColor(Color color)
    {
        materialInstance?.SetParameterValue("BrushColor", color);
    }
    internal void MaterialSetBrushParamiters(float BrushSize, float BrushStrength,float BrushFalloff)
    {
        materialInstance?.SetParameterValue("BrushSize", BrushSize);
        materialInstance?.SetParameterValue("BrushStrength", BrushStrength);
        materialInstance?.SetParameterValue("BrushFalloff", BrushFalloff);
    }

    internal void LoadAssets(Action<Asset> OnAssetLoaded)
    {
        string[] files = Directory.GetFiles(AssetDirectory, "*.s3o");
        for (int i = 0; i < files.Length; i++)
        {
            var model = S3O.Import(files[i]);
            if (model == null)
                return;

            Asset asset = new(files[i], model);
            Assets.Add(asset);
            OnAssetLoaded(asset);
        }
    }
}
