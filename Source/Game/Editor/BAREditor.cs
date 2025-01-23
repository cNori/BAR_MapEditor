using System;
using System.IO;
using FlaxEngine;

namespace Game;

public static class BAREditor
{
    public static void Init()
    {
        EditorSettings.Load();
    }
    public static void RunMap(string MapName,Action GameHasOpened, Action GameHasBeenClosed)
    {
        var bardata = EditorSettings.BeyondAllReasonData;
        var enginepath = EditorSettings.BeyondAllReasonEngine;
        var argspath = Path.Join(Globals.ProjectFolder, "Args.txt");
        File.WriteAllText(argspath,
            @"[game]
                        {
                            [allyteam1]
                            {
                                numallies = 0;
                            }
                            [team1]
                            {
                                teamleader = 0;
                                allyteam = 1;
                            }
                            [ai0]
                            {
                                shortname = NullAI;
                                name = NullAI;
                                version = 0.1;
                                team = 1;
                                host = 0;
                            }
                            [modoptions]
                            {


                            }
                            [allyteam0]
                            {
                                numallies = 0;
                            }
                            [team0]
                            {
                                teamleader = 0;
                                allyteam = 0;
                            }
                            [player0]
                            {
                                team = 0;
                                name = BAR_Editor;
                            }
                            mapname = " + MapName + @";
                            myplayername = BAR_Editor;
                            ishost = 1;
                            gametype = rapid://byar:test;
                            nohelperais = 0;
                        }");

        if (File.Exists(enginepath))
        {
            CreateProcessSettings processSettings = new()
            {
                Arguments = $"--isolation --write-dir {bardata} {argspath}",
                FileName = enginepath,
                HiddenWindow = false,
                WaitForEnd = false,
                LogOutput = true,
                SaveOutput = false,
                ShellExecute = false
            };

            JobSystem.Dispatch((int i) =>
            {
                Scripting.RunOnUpdate(GameHasOpened);
                Platform.CreateProcess(ref processSettings);
                Scripting.RunOnUpdate(GameHasBeenClosed);
            });
        }
    }
}
