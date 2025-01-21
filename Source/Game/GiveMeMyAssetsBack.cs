using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GiveMeMyAssetsBack Script.
/// </summary>
public class GiveMeMyAssetsBack : Script
{
    //the bs on top of bs when using Content.Load by code is not counting as ref so
    //assets are striped from the build...

    public Asset[] Assets;

    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update
    }
    
    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }
}
