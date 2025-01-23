using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class LightSettingsTab
{
    public static void BuildUI(VerticalPanel panel)
    {
        Utility.UI.TitleProperty(panel, "LightSettings");
    }
}