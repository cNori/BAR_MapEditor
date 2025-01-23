using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;

namespace Game;
public struct EditorSettings
{
    public EditorSettings() { }

    public static EditorSettings Instance;

    /// <summary>
    /// Curent map name
    /// </summary>
    public string Map;

    /// <summary>
    /// Curent map name version
    /// </summary>
    public string MapVersion;

    /// <summary>
    /// The beyond all reason path absolute path
    /// </summary>
    public string BeyondAllReasonPath;

    /// <summary>
    /// The beyond all reason engine version (name of the folder)
    /// </summary>
    public string BeyondAllReasonEngineVersion;


    public string MapHeightMapTextureSource = "maps/HeightMap.png";
    public string MapColorMapTextureSource = "maps/ColorMap.png";
    public string MapAssetsSource = "objects3d";
    public string MapAssetsTexturesSource = "unittextures";

    public static string BeyondAllReasonData => Path.Combine(Instance.BeyondAllReasonPath, "data");

    public static string BeyondAllReasonEngine => Path.Combine(BeyondAllReasonData, "engine", Instance.BeyondAllReasonEngineVersion, "spring.exe");

    public static void Save()
    {
        Debug.Log("Saved Settings");

        File.WriteAllText(Path.Join(Globals.ProjectFolder, "EditorSettings.json"), FlaxEngine.Json.JsonSerializer.Serialize(Instance));
    
    }
    public static void Load()
    {

        var p = Path.Join(Globals.ProjectFolder, "EditorSettings.json");
        Debug.Log("trying to load editor settings at path");
        Debug.Log(p);
        if (File.Exists(p))
        {
            Instance = FlaxEngine.Json.JsonSerializer.Deserialize<EditorSettings>(File.ReadAllText(p));
        }
        else
        {
            Debug.Log("Creating new EditorSettings.json");
            Instance = new EditorSettings();
            File.WriteAllText(Path.Join(Globals.ProjectFolder, "EditorSettings.json"), FlaxEngine.Json.JsonSerializer.Serialize(Instance));
        }
    }

    internal string GetMapHeightMapTextureSource()
    {
        if (Path.IsPathRooted(MapHeightMapTextureSource))
        {
            return MapHeightMapTextureSource;
        }
        return Path.Join(MapPath, MapHeightMapTextureSource);
    }

    internal string GetMapColorMapTextureSource()
    {
        if (Path.IsPathRooted(MapColorMapTextureSource))
        {
            return MapColorMapTextureSource;
        }
        return Path.Join(MapPath, MapColorMapTextureSource);
    }

    internal string GetMapAssetsSource()
    {
        if (Path.IsPathRooted(MapAssetsSource))
        {
            return MapAssetsSource;
        }
        return Path.Join(MapPath, MapAssetsSource);
    }

    internal string GetMapAssetsTexturesSource()
    {
        if (Path.IsPathRooted(MapAssetsTexturesSource))
        {
            return MapAssetsTexturesSource;
        }
        return Path.Join(MapPath, MapAssetsTexturesSource);
    }

    internal string MapPath => Path.Combine(BeyondAllReasonData, "maps", Map + ".sdd");
}