using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public partial class Terrain : Script
{
    public class Asset
    {
        public string Name;
        public string Source;
        public Model ModelDisplay;
        public Texture ModelDisplayTexture0;
        public Texture ModelDisplayTexture1;

        public Asset(string source, Model model)
        {
            this.Source = source;
            this.ModelDisplay = model;
            this.Name = Path.GetFileNameWithoutExtension(source);
        }

        public bool Load()
        {
            var model = S3O.Import(Source);

            if (model == null)
                return false;
            return true;
        }
    }
}