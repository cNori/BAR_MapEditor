using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class ExportTab
{

    public static void BuildUI(VerticalPanel panel)
    {
        Utility.UI.TitleProperty(panel, "Export");
        Utility.UI.ButtonProperty(panel, "Export Feature Placement", ()=> 
        {
            Export.ExportFP();
        });
    }
}