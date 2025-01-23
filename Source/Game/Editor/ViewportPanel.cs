using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public class ViewportPanel : ContainerControl
{
    public static ViewportPanel Instance { get; private set; }

    bool capstate = false;

    public ViewportPanel() : base()
    {
        Instance = this;
    }

    public void MouseCapture(bool Active)
    {
        if (Active == capstate)
            return;
        capstate = Active;
        if (Active)
            StartMouseCapture();
        else
            EndMouseCapture();
    }
}
