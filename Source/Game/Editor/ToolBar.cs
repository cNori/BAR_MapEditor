using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;


public static class ToolBar
{
    public static void BuildUI(HorizontalPanel horizontalPanel)
    {
        var rungame = horizontalPanel.AddChild<Button>();
        rungame.Size = new Float2(horizontalPanel.Height - horizontalPanel.Margin.Height);
        rungame.BackgroundBrush = new TextureBrush(Content.Load<Texture>(Path.Combine(Globals.ProjectContentFolder, "BlendIcons", "play.flax")));
        rungame.HasBorder = false;
        rungame.SetColors(Color.Green);

        rungame.Clicked += () =>
        {
            EditorSettings.Load();

            BAREditor.RunMap(EditorSettings.Instance.Map, () => { rungame.SetColors(Color.Red); }, () =>{ rungame.SetColors(Color.Green); });
        };
    }
}