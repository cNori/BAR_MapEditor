using System;
using System.IO;
using System.Linq;
using System.Text;
using FlaxEngine;
namespace Game;

public class S3O
{
    enum Primitive : int
    {
        Triangle,
        TriangleStrip,
        Quad,
    }

    public static Model Import(string SourceFile)
    {
        using var stream = File.Open(SourceFile, FileMode.Open);
        using var binary = new BinaryReader(stream, Encoding.UTF8, false);
        string Texture1Name = "";
        string Texture2Name = "";
        {
            for (int i = 0; i < "Spring unit\0".Length; i++)
            {
                if (binary.ReadByte() != "Spring unit\0"[i])
                {
                    return null;
                }
            }

            int Version = binary.ReadInt32();
            float Radius = binary.ReadInt32();
            float Height = binary.ReadInt32();
            Float3 Mid = binary.ReadFloat3();
            int RootPieceOffset = binary.ReadInt32();
            int CollisionDataOffset = binary.ReadInt32();  // 0, unimplemented



            if (Version != 0 || CollisionDataOffset != 0)
            {
                return null;
            }

            //read texture names

            int Texture1Offset = binary.ReadInt32();
            int Texture2Offset = binary.ReadInt32();

            if (Texture1Offset != 0)
            {
                binary.BaseStream.Seek(Texture1Offset, SeekOrigin.Begin);
                while (true)
                {
                    char c = (char)binary.ReadByte();
                    Texture1Name += c;
                    if (c == '\0')
                        break;
                }
            }
            if (Texture2Offset != 0)
            {
                binary.BaseStream.Seek(Texture2Offset, SeekOrigin.Begin);
                while (true)
                {
                    char c = (char)binary.ReadByte();
                    Texture2Name += c;
                    if (c == '\0')
                        break;
                }
            }
            binary.BaseStream.Seek(RootPieceOffset, SeekOrigin.Begin);
        }
        //read root object
        {
            int NameOffset = binary.ReadInt32();
            int NumChildren = binary.ReadInt32();
            int ChildrenOffset = binary.ReadInt32();
            int NumVertices = binary.ReadInt32();
            int VerticesOffset = binary.ReadInt32();
            int VertexType = binary.ReadInt32();
            Primitive PrimitiveType = (Primitive)binary.ReadInt32();
            int ShapeTableSize = binary.ReadInt32();
            int ShapeTableOffset = binary.ReadInt32();
            int CollisionDataOffset = binary.ReadInt32();
            Float3 Offset = binary.ReadFloat3();

            string ObjectName = "";
            if (NameOffset != 0)
            {
                binary.BaseStream.Seek(NameOffset, SeekOrigin.Begin);
                while (true)
                {
                    char c = (char)binary.ReadByte();
                    ObjectName += c;
                    if (c == '\0')
                        break;
                }
            }
            Float3 [] vertices = new Float3[NumVertices];
            Float3 [] normals = new Float3[NumVertices];
            Float2 []uv = new Float2[NumVertices];
            int[] triangles = null;

            //read verts
            binary.BaseStream.Seek(VerticesOffset, SeekOrigin.Begin);
            for (int i = 0; i < NumVertices; i++)
            {
                vertices[i] = binary.ReadFloat3();
                normals[i] = binary.ReadFloat3();
                uv[i] = binary.ReadFloat2();
            }
            binary.BaseStream.Seek(ShapeTableOffset, SeekOrigin.Begin);

            if (PrimitiveType == Primitive.Triangle)
            {
                triangles = new int[ShapeTableSize];
                for (int i = 0; i < ShapeTableSize; i++)
                {
                    triangles[i] = binary.ReadInt32();
                }

                var Model = Content.CreateVirtualAsset<Model>();
                Model.SetupLODs([2]);
                Model.LODs[0].Meshes[0].UpdateMesh(vertices, triangles, normals, null, uv);
                var materialInstance = Content.CreateVirtualAsset<MaterialInstance>();
                var materialInstanceLOD1 = Content.CreateVirtualAsset<MaterialInstance>();
                materialInstance.BaseMaterial = Content.Load<Material>(Path.Join(Globals.ProjectContentFolder, "ObjectMaterial.flax"));


                if (!string.IsNullOrEmpty(Texture1Name) || !string.IsNullOrEmpty(Texture2Name))
                {
                    try
                    {



                        var p = EditorSettings.Instance.GetMapAssetsTexturesSource();
                        var files = Directory.EnumerateFiles(p, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".png") || s.EndsWith(".tga"));

                        if (!string.IsNullOrEmpty(Texture1Name))
                        {
                            Debug.Log("looking for " + Texture2Name);
                            for (int i = 0; i < files.Count(); i++)
                            {
                                var file = files.ElementAt(i);
                                var name = Path.GetFileName(file);
                                if (name.EndsWith(Texture1Name))
                                {
                                    Debug.Log(file);
                                    materialInstance.SetParameterValue("Texture0", Texture.FromFile(file));
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(Texture2Name))
                        {
                            Debug.Log("looking for " + Texture2Name);
                            for (int i = 0; i < files.Count(); i++)
                            {
                                var file = files.ElementAt(i);
                                var name = Path.GetFileName(file);
                                if (name.EndsWith(Texture2Name))
                                {
                                    Debug.Log(file);
                                    materialInstance.SetParameterValue("Texture1", Texture.FromFile(file));
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                Model.MaterialSlots[0].Material = materialInstance;

                return Model;
            }
            else
            {
                Debug.Log("PrimitiveType unsuported yet todo");
                return null;
            }
        }
    }
}



[ExecuteInEditMode]
/// <summary>
/// S3O_Importer Script.
/// </summary>
public class S3O_Importer : Script
{
    public StaticModel staticModel;
    public bool Run = false;
    public string Sourcefile;
    /// <inheritdoc/>
    public override void OnUpdate()
    {
        if (Run)
        {
            staticModel.Model = S3O.Import(Sourcefile);
            Run = false;
        }
    }

}
