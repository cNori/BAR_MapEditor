using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;

namespace Game;

public static class Shared
{
#if FLAX_EDITOR
    public static string ProjectPath = "C:\\Users\\BlackBack\\AppData\\Local\\Programs\\Beyond-All-Reason\\data\\maps\\NATG.sdd";
#else
    public static string ProjectPath = "";
#endif


    public static bool CheakRootPath(string input)
    {
        if (Directory.Exists(input) && input.EndsWith(".sdd"))
        {
            return true;
        }
        return false;
    }

    public static string SetsddPath(string path)
    {
        if (Shared.CheakRootPath(path))
        {
            Shared.ProjectPath = path;
            return path;
        }
        return Shared.ProjectPath;
    }
}