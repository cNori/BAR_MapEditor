using System;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class ImportTab
{
    public static void BuildUI(VerticalPanel panel)
    {
        Utility.UI.TitleProperty(panel, "Import");

        Utility.UI.TitleProperty(panel, ".sdd Paths");
        Utility.UI.TextProperty(panel, "Project", Shared.SetsddPath, Shared.ProjectPath);
        Utility.UI.TextProperty(panel, "Height Map", Import.SetProjectHeightMapPath, Import.ProjectHeightMapTextureSource);
        Utility.UI.TextProperty(panel, "Color Map", Import.SetProjectColorMapPath, Import.ProjectColorMapTextureSource);
        Utility.UI.TextProperty(panel, "Assets", Import.SetProjectAssetsPath, Import.ProjectAssetsSource);
        Utility.UI.TextProperty(panel, "Assets Textures", Import.SetProjectAssetsTexturesPath, Import.ProjectAssetsTexturesSource);
        Utility.UI.IntProperty(panel, "Map Size X", (int x) => { Terrain.Instance.Size = new Int2(x, Terrain.Instance.Size.Y); }, Terrain.Instance.Size.X,false,1,64);
        Utility.UI.IntProperty(panel, "Map Size Y", (int y) => { Terrain.Instance.Size = new Int2(Terrain.Instance.Size.X, y); }, Terrain.Instance.Size.Y,false,1,64);

        
        Utility.UI.ButtonProperty(panel, "Import Height", () => 
        {
            Import.ImportProjectHeightMap();
            Terrain.Instance.LoadHeightMap();
        });
        Utility.UI.ButtonProperty(panel, "Import Color", () =>
        {
            JobSystem.Dispatch((int __) =>
            {
                Import.ImportProjectColorMap();
                Terrain.Instance.SetColorMap();
            });
        });
        Utility.UI.ButtonProperty(panel, "Import Maps", () =>
        {
            Import.ImportProjectColorMap();
            Import.ImportProjectHeightMap();
            Terrain.Instance.SetColorMap();
            Terrain.Instance.LoadHeightMap();
        });

        Utility.UI.ButtonProperty(panel, "Import Assets", () =>
        {
            Import.ImportAssets((int ID) =>
            { 
                var display = MainTab.AssetView.AddChild<AssetView.AssetDisplay>();
                display.Name = Path.GetFileNameWithoutExtension(Import.Assets[ID].Source).Replace('_', ' ');
                display.AssetID = ID;
            });
        });
        Utility.UI.ButtonProperty(panel, "Import FP", () =>
        {
            if (Import.Assets.Count == 0)
                return;
            var dir = Path.Join(Shared.ProjectPath, "mapconfig", "featureplacer", "set.lua");
            if (File.Exists(dir))
            {
                var fs = File.ReadAllText(dir);
                Import.ImportFP(fs);
            }
        });
        Utility.UI.TitleProperty(panel, "External Import");
        Utility.UI.TitleProperty(panel, "[To Do]");

        //Utility.UI.ButtonProperty(panel, "Import All", () =>
        //{
        //    Import.ImportProjectHeightMap();
        //    Import.ImportProjectColorMap();
        //    Terrain.Instance.SetColorMap();
        //    Terrain.Instance.LoadHeightMap();
        //});
    }
}