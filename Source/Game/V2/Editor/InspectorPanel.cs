using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

public class InspectorPanel : ContainerControl
{
    const float TabSize = 25;
    public class Tab : CheckBox
    {
        public Image image;
        public int id;

        public Tab() : base()
        {
            Width = TabSize;
            Height = TabSize;
            HasBorder = false;
            BoxSize = TabSize;
            BackgroundColor = Color.White;
            //image = AddChild<Image>();
            //image.SetAnchorPreset(AnchorPresets.StretchAll, false);
        }
        public void Deselect()
        {
            _state = CheckBoxState.Default;
        }
        public void Select()
        {
            _state = CheckBoxState.Checked;
        }
    }
    int selected;
    List<VerticalPanel> TabPanels = new List<VerticalPanel>();
    public InspectorPanel() : base()
    {
        BackgroundColor = Style.Current.Background;

        //background
        var bc = AddChild<Control>();
        bc.SetAnchorPreset(AnchorPresets.HorizontalStretchTop,false);
        bc.Height = TabSize;
        bc.BackgroundColor = Style.Current.LightBackground;

        var bc2 = AddChild<Control>();
        bc2.SetAnchorPreset(AnchorPresets.StretchAll, false);
        bc2.Height = Height - TabSize;
        bc2.BackgroundColor = Style.Current.LightBackground.RGBMultiplied(0.75f);
        bc2.X += TabSize;
        bc2.Y += TabSize;
        bc2.Width -= TabSize;

        VerticalPanel tabsPanel = AddChild<VerticalPanel>();
        tabsPanel.BackgroundColor = Style.Current.LightBackground;
        tabsPanel.Height = Height;
        //tabsPanel.AutoSize = false;
        tabsPanel.Width = TabSize;
        tabsPanel.Y += TabSize;

        tabsPanel.Spacing = 4;
        
        tabsPanel.Margin = Margin.Zero;
        string[] tabs =
            [
            "\\outliner.flax",
            "\\outliner_data_light.flax",
            "\\outliner_collection.flax",
            "\\outliner_ob_group_instance.flax"
            ];
        for (int i = 0; i < tabs.Length; i++)
        {
            var t = tabsPanel.AddChild<Tab>();
            t.BackgroundBrush = new TextureBrush(Content.Load<Texture>(Globals.ProjectContentFolder + tabs[i]));
            t.CheckedImage = t.BackgroundBrush;

            var vp = AddChild<VerticalPanel>();
            TabPanels.Add(vp);
            if (i == 0)
            {
                t.Checked = true;
                selected = i;
            }
            else
            {
                TabPanels[i].Visible = false;
                TabPanels[i].Enabled = false;
            }
            t.StateChanged += (CheckBox thiscb) =>
        {
            for (int j = 0; j < tabsPanel.Children.Count; j++)
            {
                if (tabsPanel.Children[j] is Tab cb)
                {
                    if (thiscb != tabsPanel.Children[j])
                    {
                        cb.Deselect();
                        TabPanels[cb.id].Visible = false;
                        TabPanels[cb.id].Enabled = false;
                    }
                    else
                    {
                        var tab = (thiscb as Tab);
                        tab.Select();
                        selected = tab.id;
                        TabPanels[tab.id].Visible = true;
                        TabPanels[tab.id].Enabled = true;
                    }
                }
            }
        };
            t.id = i;



            vp.SetAnchorPreset(AnchorPresets.StretchAll, false);
            vp.X += tabsPanel.Width;
            vp.Width -= tabsPanel.Width;
        }

        MainTab.BuildUI(TabPanels[0]);
        LightSettingsTab.BuildUI(TabPanels[1]);
        ImportTab.BuildUI(TabPanels[2]);
        ExportTab.BuildUI(TabPanels[3]);


    }
}