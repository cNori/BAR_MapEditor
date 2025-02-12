﻿using System;
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
    public static HorizontalPanel ToolBarPanel;
    public int Major;
    public int Minor;
    public int Pach;
    Label version;
    public override void OnAwake()
    {
        BAREditor.Init();

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

        version = Root.AddChild<Label>();
        version.HorizontalAlignment = TextAlignment.Near;
        version.VerticalAlignment = TextAlignment.Near;
        version.TextColor = Color.Black;
        version.TextColorHighlighted = Color.Black;
        version.Font.Size = 8;

        
    }

    public override void OnStart()
    {
        Viewport = Root.AddChild<ViewportPanel>();
        TerainBrush = Viewport.AddChild<VerticalPanel>();
        ToolBarPanel = Root.AddChild<HorizontalPanel>();
        ToolBarPanel.AutoSize = false;
        ToolBarPanel.Height = 32;
        ToolBarPanel.Width = Root.Width;
        ToolBarPanel.BackgroundColor = Style.Current.Background;
        ToolBarPanel.SetAnchorPreset(AnchorPresets.HorizontalStretchTop, true);

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
        updatespacer(null);
        Root.SizeChanged += updatespacer;

        Terrain.Instance.InitBrushSettingsUI();

        ToolBar.BuildUI(ToolBarPanel);

        version.Y = ToolBarPanel.Height;
    }

    private void updatespacer(Control obj)
    {
        Viewport.Location = new Float2(0, ToolBarPanel.Height);
        Viewport.Height = Root.Height - ToolBarPanel.Height;
        Viewport.Width = Root.Width - inspectorsize - spacersize;
        Inspector.Height = Root.Height;
        Inspector.Width = inspectorsize;
        Inspector.Location = new Float2(Viewport.Width + spacersize, 0);
        spacer.Location = new Float2(Viewport.Width, ToolBarPanel.Height);
        spacer.Height = Root.Height - ToolBarPanel.Height;
        spacer.Width = spacersize;

        ToolBarPanel.Width = Root.Width - inspectorsize;
    }

    public bool Drag = false;
    public override void OnUpdate()
    {
        version.Text = "Version " + Major + "." + Minor + "." + Pach;

        if (!TerainBrush.Visible)
        {
            TerainBrush.Location = Input.MousePosition;

            TerainBrush.Y -= ToolBarPanel.Height;
        }


        if (spacer == null)
            return;
        if (spacer.Draged)
        {
            inspectorsize = Root.Width - Input.MousePosition.X - spacersize * 0.5f;
            inspectorsize = Mathf.Clamp(inspectorsize, 200, 400);
            updatespacer(null);
        }
        base.OnUpdate();
    }

}
