using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class SettingsTab
{
    public static void BuildUI(VerticalPanel panel)
    {
        Utility.UI.TitleProperty(panel, "Settings");

        TextBox[] textBoxes = new TextBox[8];


        Utility.UI.ButtonProperty(panel, "Load", () =>
        {
            EditorSettings.Load();
            textBoxes[0].Text = EditorSettings.Instance.BeyondAllReasonPath;
            textBoxes[1].Text = EditorSettings.Instance.BeyondAllReasonEngineVersion;
            textBoxes[2].Text = EditorSettings.Instance.Map;
            textBoxes[3].Text = EditorSettings.Instance.MapVersion;
            textBoxes[4].Text = EditorSettings.Instance.MapHeightMapTextureSource;
            textBoxes[5].Text = EditorSettings.Instance.MapColorMapTextureSource;
            textBoxes[6].Text = EditorSettings.Instance.MapAssetsSource;
            textBoxes[7].Text = EditorSettings.Instance.MapAssetsTexturesSource;
        });
        Utility.UI.ButtonProperty(panel, "Save", () => { EditorSettings.Save(); });

        string OnBarPathSet(string path)
        {
            string cpath = Path.Join(path, "Beyond-All-Reason.exe");
            if (Path.Exists(cpath))
            {
                EditorSettings.Instance.BeyondAllReasonPath = path;
                return path;
            }
            return EditorSettings.Instance.BeyondAllReasonPath;
        }
        textBoxes[0] = Utility.UI.TextProperty(panel, "Beyond All Reason Path", OnBarPathSet,                                               EditorSettings.Instance.BeyondAllReasonPath);
        textBoxes[1] = Utility.UI.TextProperty(panel, "Engine Version", (string s) => { EditorSettings.Instance.BeyondAllReasonEngineVersion = s; return s; },  EditorSettings.Instance.BeyondAllReasonEngineVersion);
        textBoxes[2] = Utility.UI.TextProperty(panel, "Map", (string s) => { EditorSettings.Instance.Map = s; return s; },                                      EditorSettings.Instance.Map);
        textBoxes[3] = Utility.UI.TextProperty(panel, "Map Version", (string s) => { EditorSettings.Instance.MapVersion = s; return s; },                       EditorSettings.Instance.MapVersion);
        Utility.UI.TitleProperty(panel, "Import paths");
        textBoxes[4] = Utility.UI.TextProperty(panel, "Height Map", Import.SetProjectHeightMapPath,                                         EditorSettings.Instance.MapHeightMapTextureSource);
        textBoxes[5] = Utility.UI.TextProperty(panel, "Color Map", Import.SetProjectColorMapPath,                                           EditorSettings.Instance.MapColorMapTextureSource);
        textBoxes[6] = Utility.UI.TextProperty(panel, "Assets", Import.SetProjectAssetsPath,                                                EditorSettings.Instance.MapAssetsSource);
        textBoxes[7] = Utility.UI.TextProperty(panel, "Assets Textures", Import.SetProjectAssetsTexturesPath,                               EditorSettings.Instance.MapAssetsTexturesSource);
    }
}