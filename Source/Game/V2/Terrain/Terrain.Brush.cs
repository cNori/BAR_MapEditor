using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public partial class Terrain : Script
{
    public virtual void Paint()
    {
        var selected = MainTab.AssetView.GetSelectedAssets();
        var size = BrushSize * 0.5f;
        Utility.Random random = new Utility.Random((uint)(BrushPosition.X + BrushPosition.Y));

        int pid = 1;

        //GUARD IT
        var space = Mathf.Max(Mathf.CeilToInt(BrushMinDistance),1);
        Float2[] points = new Float2[(int)(BrushSize * BrushSize)];
        points[0] = new Float2(BrushPosition.X, BrushPosition.Y);

            for (int x = Mathf.CeilToInt(BrushPosition.X - size); x < Mathf.CeilToInt(BrushPosition.X + size); x += space)
            {
                for (int y = Mathf.CeilToInt(BrushPosition.Y - size); y < Mathf.CeilToInt(BrushPosition.Y + size); y += space)
                {
                    random.Next();
                    float rx = random.Range(-0.5f, 0.5f) * space;
                    random.Next();
                    float ry = random.Range(-0.5f, 0.5f) * space;
                    var uv = new Float2(x + rx + (x - BrushPosition.X), y + ry + (y - BrushPosition.Y));
                    if (SampleBrushObjectPlacment(uv, random))
                    {
                        bool Add = true;
                        for (int i = 0; i < pid; i++)
                        {
                            if ((points[i] - uv).Length < BrushMinDistance)
                            {
                                Add = false;
                                break;
                            }
                        }
                        if (Add)
                        {
                            points[pid] = uv;
                            pid++;
                        }
                    }
                }
            }
        for (int i = 0; i < pid; i++)
        {

            Float3 p = new Float3(points[i].X, WorldGetHeight(points[i].X, points[i].Y), points[i].Y);
            var chunk = WorldGetChunk(points[i].X, points[i].Y);

            if (RemoveObjects)
            {
                for (int o = 0; o < chunk.objects.Count; o++)
                {
                    if (Float2.Distance(chunk.objects[o].Position, points[i]) <= BrushMinDistance)
                    {
                        DestroyNow(chunk.objects[o].Actor);
                        chunk.objects.RemoveAt(o);
                        o--;
                    }
                }
            }
            else
            {
                if (selected.Count == 0)
                    return;


                bool Canplace = true;
                for (int o = 0; o < chunk.objects.Count; o++)
                {
                    if (Float2.Distance(chunk.objects[o].Position, points[i]) <= BrushMinDistance)
                    {
                        Canplace = false;
                        break;
                    }
                }
                if (Canplace)
                {
                    random.Next();
                    var ro = Mathf.FloorToInt(random.Get01() * selected.Count);
                    var selectedAsset = selected[Mathf.Clamp(ro, 0, selected.Count - 1)];
                    if (IsPointInsideTheTerrainBounds2D(p))
                    {
                        chunk.SpawnAsset(selectedAsset, p, BrushGetRandomRotaction(random));
                    }
                }
            }
        }
    }

    private float BrushGetRandomRotaction(Utility.Random random)
    {
        return Mathf.Remap(random.Get01(), 0,1,-180,180);
    }

    public float SampleBrush(Float2 cords)
    {
        var uv = BrushPosition - cords;
        float cg = 1 - (uv * 2).Length;
        float Falloff = BrushFalloff * BrushSize;
        return Mathf.Clamp(((cg - (1 - (BrushSize - Falloff))) + Falloff) / Falloff, 0, 1) * BrushStrength;
    }
    public bool SampleBrushObjectPlacment(Float2 cords, Utility.Random random)
    {
        random.Next();
        float canplace = random.Get01();
        if (canplace < SampleBrush(cords))
        {
            return true;
        }
        return false;
    }
}