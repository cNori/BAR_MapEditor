using System;
using System.Collections.Generic;
using System.Diagnostics;
using FlaxEngine;

namespace Game;

[ExecuteInEditMode]
public class SpringArm : Script
{
    public float Distance = 10;
    public Float2 CameraAngle;
    [Space(1)]
    public float MinDistance = 20;
    public float MaxDistance = 10000;
    public float DistanceStepSize = 1;
    public float AngleStepSize = 1;
    public float FallowStepSize = 1;
    private Float2 cameraAngle;
    private float distance = 10;
    private Camera camera;

    public Camera Camera => camera;

    private Vector3 LastPosition;
    public override void OnStart()
    {
        camera = Actor.GetChild<Camera>();
    }
    public override void OnUpdate()
    {
        if (camera == null)
            return;

        Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
        distance = Mathf.SmoothStep(distance, Distance, DistanceStepSize * Time.UnscaledDeltaTime);
        //var cameraAngleXt = Mathf.SmoothStep(cameraAngle.X, CameraAngle.X, AngleStepSize * Time.UnscaledDeltaTime);
        //var cameraAngleYt = Mathf.SmoothStep(cameraAngle.Y, CameraAngle.Y, AngleStepSize * Time.UnscaledDeltaTime);
        var cameraAngleXt = Mathf.SmoothStep(0, 1, AngleStepSize * Time.UnscaledDeltaTime);
        var cameraAngleYt = Mathf.SmoothStep(0, 1, AngleStepSize * Time.UnscaledDeltaTime);

        CameraAngle.X = Mathf.UnwindDegrees(CameraAngle.X);
        CameraAngle.Y = Mathf.UnwindDegrees(CameraAngle.Y);
        cameraAngle.X = Mathf.LerpAngle(cameraAngle.X, CameraAngle.X, cameraAngleXt);
        cameraAngle.Y = Mathf.LerpAngle(cameraAngle.Y, CameraAngle.Y, cameraAngleYt);

        Quaternion rot = Quaternion.Euler(cameraAngle.X, cameraAngle.Y, 0);
        camera.Position = LastPosition - new Vector3(0, 0, distance) * rot;
        camera.Orientation = rot;
        LastPosition = Vector3.SmoothStep(LastPosition, Actor.Position, Time.UnscaledDeltaTime * FallowStepSize);
    }
}
