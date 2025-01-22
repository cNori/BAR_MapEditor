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
        Utility.UI.TextProperty(panel, ".sdd Path", Shared.SetsddPath, Shared.ProjectPath);

        //Utility.UI.FloatProperty(panel, "UnitScale", (float f)=>{ Export.UnitScale = f; },1,false);
        Utility.UI.ButtonProperty(panel, "Export Feature Placement", ()=> 
        {
            Export.ExportFP();
        });
    }
}