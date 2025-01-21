using FlaxEngine;
using System.Collections.Generic;
namespace Game;
public partial class Terrain : Script
{
    
    public class Chunk(Terrain owner, Int2 position)
    {
        internal bool IsDirty = false;
        [Serialize] public readonly Terrain Owner = owner;
        [Serialize] public readonly Int2 Position = position;
        [Serialize] public List<Object> objects = [];
        [Serialize] public Actor objectsCargo;
        private Float3[] HeightMap = [];
        private int HeightMapSize;

        internal void BuildHeightMap()
        {
            HeightMapSize = Terrain.ChunkSize + 1;
            HeightMap = new Float3[HeightMapSize * HeightMapSize];
            for (int x = 0; x < HeightMapSize; x++)
            {
                for (int y = 0; y < HeightMapSize; y++)
                {
                    HeightMap[x * HeightMapSize + y] = new Float3(x,0, y);
                }
            }
        }
        public void UpdateScale()
        {
            for (int x = 0; x < HeightMapSize; x++)
            {
                for (int y = 0; y < HeightMapSize; y++)
                {
                    HeightMap[x * HeightMapSize + y] = new Float3(x, 0, y);
                }
            }
        }


        public bool SpawnAsset(int AssetID, Float3 position, float rotation)
        {
            var asset = Import.Assets[AssetID];
            var model = objectsCargo.AddChild<StaticModel>();
            var obj = model.AddScript<Terrain.Object>();
            model.Name = asset.Name;
            model.Position = position;
            model.Scale = new(0.20f);

            model.Orientation = Quaternion.RotationY(rotation);
            model.Model = asset.ModelDisplay;

            obj.Position.X = position.X;
            obj.Position.Y = position.Z;

            objects.Add(obj);
            obj.Owner = this;
            return true;
        }

        public float GetHeight(int x, int y)
        {
            return HeightMap[x * HeightMapSize + y].Y;
        }

        public void SetHeight(int x, int y, float value)
        {
            HeightMap[x * HeightMapSize + y].Y = value;

            IsDirty = true;
        }
        public void GenerateMesh(StaticModel staticModel)
        {
            int[] triangles = new int[Terrain.ChunkSize * Terrain.ChunkSize * 6];

            for (int x = 0; x < Terrain.ChunkSize; x++)
            {
                for (int y = 0; y < Terrain.ChunkSize; y++)
                {
                    var vertexID = x * HeightMapSize + y;
                    var triangleID = (x * Terrain.ChunkSize + y) * 6;
                    triangles[triangleID + 2] = vertexID + Terrain.ChunkSize + 1;
                    triangles[triangleID + 1] = vertexID + 1;
                    triangles[triangleID + 0] = vertexID;
                    triangles[triangleID + 5] = vertexID + 1;
                    triangles[triangleID + 4] = vertexID + Terrain.ChunkSize + 1;
                    triangles[triangleID + 3] = vertexID + Terrain.ChunkSize + 2;
                }
            }
            for (int x = 0; x <= Terrain.ChunkSize; x++)
            {
                var vertexID = x * HeightMapSize;
                HeightMap[vertexID].Y = Owner.GetHeight(Position.X * ChunkSize + x, Position.Y * ChunkSize);
            }
            for (int y = 0; y <= Terrain.ChunkSize; y++)
            {
                var vertexID = y;
                HeightMap[vertexID].Y = Owner.GetHeight(Position.X * ChunkSize, Position.Y * ChunkSize + y);
            }
            Float3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
            {
                Float3 pointA = HeightMap[indexA];
                Float3 pointB = HeightMap[indexB];
                Float3 pointC = HeightMap[indexC];

                Float3 sideAB = pointB - pointA;
                Float3 sideAC = pointC - pointA;
                return Float3.Cross(sideAB, sideAC).Normalized;
            }
            Float3[] vertexNormals = new Float3[HeightMap.Length];
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

            staticModel.Model = Content.CreateVirtualAsset<Model>();
            staticModel.Model.SetupLODs([1]);
            staticModel.Model.LODs[0].Meshes[0].UpdateMesh(HeightMap, triangles, vertexNormals, null, null);
            staticModel.Model.SetupMaterialSlots(1);
            staticModel.Model.MaterialSlots[0].Material = Terrain.Instance.material;

            var mc = staticModel.GetChild<MeshCollider>();
            mc.CollisionData = Content.CreateVirtualAsset<CollisionData>();
            JobSystem.Wait(JobSystem.Dispatch((int id) =>
            {
                mc.CollisionData.CookCollision(CollisionDataType.TriangleMesh, HeightMap, triangles);
            }));
        }
        public void DDraw()
        {
            Vector3 pos = new Vector3(Position.X * Terrain.ChunkSize, 0, Position.Y * Terrain.ChunkSize);
            //for (int x = Position.X == 0 ? 0 : 1; x <= Terrain.ChunkSize; x++)
            //{
            //    for (int y = Position.Y == 0 ? 0 : 1; y <= Terrain.ChunkSize; y++)
            //    {
            //        var h = GetHeight(x, y);
            //        //var h = 0;
            //        Utility.DrawPoint(new Vector3(x,h, y) + pos, Color.Aqua, 0.1f);
            //    }
            //}
            Vector3 max = new Vector3(HeightMapSize - 1, 0, HeightMapSize - 1);
            var bb = new BoundingBox(pos, max + pos);

            DebugDraw.DrawWireBox(bb, Color.LightGreen);
            DebugDraw.DrawWireBox(bb, Color.LightGreen.AlphaMultiplied(0.25f));
        }
    }
}