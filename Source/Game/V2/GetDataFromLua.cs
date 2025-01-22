using System;
using System.Collections.Generic;
using System.Globalization;
using FlaxEngine;
namespace Game;

[ExecuteInEditMode]
public class GetDataFromLua : Script
{
    public string input;
    public struct ObjectData(string name, Float2 position, float rotation)
    {
        public string Name = name;
        public Float2 Position = position;
        public float Rotation = rotation;
    }

    public List<ObjectData> data = [];

    public override void OnUpdate()
    {
        input = input.Replace(" ", "");
        input = input.RemoveNewLine();
        input = input.Replace("\t", "");

        int GetOffset(string value)
        {
            return input.IndexOf(value) + value.Length;
        }
        var start = GetOffset("objectlist=");

        var b = 0;
        var i = start;
        while (true)
        {

            if (input[i] == '{')
            {
                b++;
            }
            if (input[i] == '}')
            {
                b--;
            }
            if (b == 0)
                break;

            i++;
        }
        var values = input.Substring(start, i - start).Split(',');

        for (int s = 0; s < values.Length; s++)
        {
            values[s] = values[s].Replace("{", "").Replace("}", "").Replace("\"", "");
            if (string.Empty == values[s])
                continue;
            var ide = values[s].IndexOf('=');
            values[s] = values[s][(ide + 1)..].Replace('.', ',');
        }

        for (int s = 0; s < values.Length - 4; s+=4)
        {
            var name = values[s];
            var x = values[s+1];
            var z = values[s+2];
            var r = values[s+3];

            if (!float.TryParse(x, out var nx))
                break;
            if (!float.TryParse(z, out var nz))
                break;
            if (!float.TryParse(r, out var nr))
                break;
            data.Add(new ObjectData(name, new Float2(nx, nz) * 0.125f, nr));
        }
    }
}
