using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class MainTab
{
    public static AssetView AssetView;

    public static void BuildUI(VerticalPanel panel)
    {
        Utility.UI.TitleProperty(panel, "Main");

        Utility.UI.TitleProperty(panel, "Asset Picker");
        var scroll = panel.AddChild<Panel>();
        scroll.Width = panel.Width;
        scroll.Height = 400;
        scroll.ScrollBars = ScrollBars.Vertical;
        scroll.AlwaysShowScrollbars = true;
        scroll.SetAnchorPreset(AnchorPresets.HorizontalStretchTop,true);
        AssetView = scroll.AddChild<AssetView>();
        AssetView.Width = scroll.Width;
    }
}