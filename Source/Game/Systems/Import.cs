using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.Assertions;

namespace Game;

public static class Import
{
    public static Texture ColorMapTexture;
    public static Texture HeightMapTexture;

    public static List<Terrain.Asset> Assets = [];

    public static bool ImportProjectHeightMap()
    {
        var p = EditorSettings.Instance.GetMapHeightMapTextureSource();
        Debug.Log("Importing HeightMap from");
        Debug.Log(p);

        if (System.IO.Path.Exists(p))
        {
            HeightMapTexture = Texture.FromFile(p);
            return true;
        }
        return false;
    }
    public static bool ImportProjectColorMap()
    {
        var p = EditorSettings.Instance.GetMapColorMapTextureSource();
        Debug.Log("Importing ColorMap from");
        Debug.Log(p);
        if (System.IO.Path.Exists(p))
        {
            ColorMapTexture = Texture.FromFile(p);
            return true;
        }
        return false;
    }
    internal static void ImportAssets(Action<int /*Asset ID*/> OnAssetLoaded)
    {
        JobSystem.Dispatch((int _) =>
        {
            var p = EditorSettings.Instance.GetMapAssetsSource();

            string[] files = Directory.GetFiles(p, "*.s3o", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var model = S3O.Import(files[i]);
                if (model == null)
                    return;

                Terrain.Asset asset = new(files[i], model);
                Assets.Add(asset);
                OnAssetLoaded(i);
            }
        });
    }

    public static bool ProjectPathCheak(string input)
    {
        var p = EditorSettings.Instance.MapPath;
        if (System.IO.Path.Exists(p))
        {
            return true;
        }
        return false;
    }

    internal static string SetProjectHeightMapPath(string arg)
    {
        if (ProjectPathCheak(arg))
        {
            EditorSettings.Instance.MapHeightMapTextureSource = arg;
            return arg;
        }
        return arg;
    }

    internal static string SetProjectColorMapPath(string arg)
    {
        if (ProjectPathCheak(arg))
        {
            EditorSettings.Instance.MapColorMapTextureSource = arg;
            return arg;
        }
        return arg;
    }

    internal static string SetProjectAssetsTexturesPath(string arg)
    {
        if (ProjectPathCheak(arg))
        {
            EditorSettings.Instance.MapAssetsTexturesSource = arg;
            return arg;
        }
        return arg;
    }

    internal static string SetProjectAssetsPath(string arg)
    {
        if (ProjectPathCheak(arg))
        {
            EditorSettings.Instance.MapAssetsSource = arg;
            return arg;
        }
        return arg;
    }


    //public string Name = name;
    //public Float2 Position = position;
    //public float Rotation = rotation;
    internal static void ImportFP(string fp)
    {
        fp = fp.Replace(" ", "");
        fp = fp.RemoveNewLine();
        fp = fp.Replace("\t", "");

        int GetOffset(string value)
        {
            return fp.IndexOf(value) + value.Length;
        }
        var start = GetOffset("objectlist=");

        var b = 0;
        var i = start;
        while (true)
        {

            if (fp[i] == '{')
            {
                b++;
            }
            if (fp[i] == '}')
            {
                b--;
            }
            if (b == 0)
                break;

            i++;
        }
        var values = fp.Substring(start, i - start).Split(',');

        for (int s = 0; s < values.Length; s++)
        {
            values[s] = values[s].Replace("{", "").Replace("}", "").Replace("\"", "");
            if (string.Empty == values[s])
                continue;
            var ide = values[s].IndexOf('=');
            values[s] = values[s][(ide + 1)..].Replace('.', ',');
        }

        int j = 0;

        for (int s = 0; s < values.Length - 4; s += 4)
        {
            var name = values[s];
            var x = values[s + 1];
            var z = values[s + 2];
            var r = values[s + 3];

            if (!float.TryParse(x, out var nx))
                break;
            if (!float.TryParse(z, out var nz))
                break;
            if (!float.TryParse(r, out var nr))
                break;

            int id = -1;

            for (int k = 0; k < Assets.Count; k++)
            {
                if (Assets[k].Name == name)
                {
                    id = k;
                    break;
                }
            }
            if (id == -1)
            {
                //todo: remove it and add a pop up box if user likes to continue
                Assert.Fail("shit is missing","Cant load it one of the assets defines in the file is missing");
                continue;

            }
            var point = new Float2(nx, nz) * 0.125f;

            var chunk = Terrain.Instance.WorldGetChunk(point.X, point.Y);
            chunk.SpawnAsset(id, new Float3(point.X, Terrain.Instance.WorldGetHeight(point.X, point.Y), point.Y), nr);
            j++;
        }
    }
}