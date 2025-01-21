using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

[ExecuteInEditMode]
public class UIRoot : Script
{
    private class Spacer : Image
    {
        public bool Draged;
        public override bool OnMouseDown(Float2 location, MouseButton button)
        {
            if (button == MouseButton.Left && !Draged)
            {
                StartMouseCapture();
                Draged = true;
                Cursor = CursorType.SizeWE;
                BackgroundColor = Style.Current.BackgroundSelected;
            }

            return base.OnMouseDown(location, button);
        }
        public override bool OnMouseUp(Float2 location, MouseButton button)
        {
            if (button == MouseButton.Left && Draged)
            {
                EndMouseCapture();
                BackgroundColor = Style.Current.BackgroundNormal;
                Draged = false;
                return true;
            }
            return base.OnMouseUp(location, button);
        }
        public override void OnMouseLeave()
        {
            if (!Draged)
                BackgroundColor = Style.Current.BackgroundNormal;
            base.OnMouseLeave();
        }
        public override void OnMouseEnter(Float2 location)
        {
            if (!Draged)
                BackgroundColor = Style.Current.BackgroundHighlighted;
            base.OnMouseEnter(location);
        }
    }


    public static Panel Root { get; private set; }

    private const float spacersize = 4;
    float inspectorsize = 300;
    private Spacer spacer;
    private ViewportPanel Viewport;
    private InspectorPanel Inspector;

    public static VerticalPanel TerainBrush;

    public override void OnAwake()
    {
#if FLAX_EDITOR
        var gamewindow = Editor.Instance.Windows.GameWin;
#else
        var gamewindow = Scene.AddChild<UICanvas>().GUI;
#endif
        //clear last mod
        for (int i = 1; i < gamewindow.Children.Count; i++)
        {
            gamewindow.Children[i].Dispose();
        }
        if (gamewindow.Children.Count > 1)
        {
            gamewindow.Children.RemoveRange(1, gamewindow.Children.Count - 1);
        }

        Root = gamewindow.AddChild<Panel>();
        Root.SetAnchorPreset(AnchorPresets.StretchAll, false);
    }

    public override void OnStart()
    {
        Viewport = Root.AddChild<ViewportPanel>();
        TerainBrush = Viewport.AddChild<VerticalPanel>();
        TerainBrush.Width = inspectorsize;
        TerainBrush.Height = inspectorsize;
        TerainBrush.BackgroundColor = Style.Current.Background;
        TerainBrush.Visible = false;
        Inspector = Root.AddChild<InspectorPanel>();

        spacer = Root.AddChild<Spacer>();
        spacer.KeepAspectRatio = false;
        spacer.BackgroundColor = Style.Current.BackgroundNormal;
        spacer.SetAnchorPreset(AnchorPresets.VerticalStretchRight, true);

        inspectorsize = 300;
        Viewport.Location = Float2.Zero;
        Viewport.Height = Root.Height;
        Viewport.Width = Root.Width - inspectorsize - spacersize;
        Inspector.Height = Root.Height;
        Inspector.Width = inspectorsize;
        Inspector.Location = new Float2(Viewport.Width + spacersize, Inspector.Location.Y);
        spacer.Location = new Float2(Viewport.Width, spacer.Location.Y);
        spacer.Height = Root.Height;
        spacer.Width = spacersize;

        Root.SizeChanged += updatespacer;

        Terrain.Instance.InitBrushSettingsUI();
    }

    private void updatespacer(Control obj)
    {
        Viewport.Location = Float2.Zero;
        Viewport.Height = Root.Height;
        Viewport.Width = Root.Width - inspectorsize - spacersize;
        Inspector.Height = Root.Height;
        Inspector.Width = inspectorsize;
        Inspector.Location = new Float2(Viewport.Width + spacersize, Inspector.Location.Y);
        spacer.Location = new Float2(Viewport.Width, spacer.Location.Y);
        spacer.Height = Root.Height;
        spacer.Width = spacersize;
    }

    public bool Drag = false;
    public override void OnUpdate()
    {
        if (!TerainBrush.Visible)
            TerainBrush.Location = Input.MousePosition;


        if (spacer == null)
            return;
        if (spacer.Draged)
        {
            inspectorsize = Root.Width - Input.MousePosition.X - spacersize * 0.5f;
            inspectorsize = Mathf.Clamp(inspectorsize, 200, 400);

            Viewport.Location = Float2.Zero;
            Viewport.Height = Root.Height;
            Viewport.Width = Root.Width - inspectorsize - spacersize;
            Inspector.Height = Root.Height;
            Inspector.Width = inspectorsize;
            Inspector.Location = new Float2(Viewport.Width + spacersize, Inspector.Location.Y);
            spacer.Location = new Float2(Viewport.Width, spacer.Location.Y);
            spacer.Height = Root.Height;
            spacer.Width = spacersize;
        }
        base.OnUpdate();
    }
}
