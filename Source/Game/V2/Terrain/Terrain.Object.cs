using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public partial class Terrain : Script
{
    public class Object : Script
    {
        public Chunk Owner;
        public int AssetID = -1;

        public Float2 Position;
    }
}