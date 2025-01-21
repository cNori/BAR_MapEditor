using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
[ExecuteInEditMode]
public class CameraControler : Script
{
    public float MoveSpeed = 100;
    public SpringArm arm;
    public override void OnStart()
    {
    }
    public override void OnUpdate()
    {
        if (Terrain.Instance == null || arm == null || !Engine.HasGameViewportFocus)
            return;

        if(ViewportPanel.Instance != null)
        {
            if (!ViewportPanel.Instance.IsMouseOver || UIRoot.TerainBrush.IsMouseOver)
                return;
        }

        var speedmulty = arm.Distance / arm.MaxDistance;
        var movespeed = (MoveSpeed * arm.MaxDistance) * speedmulty * Time.UnscaledDeltaTime;

        var inputH = Input.GetAxis("Horizontal");
        var inputV = Input.GetAxis("Vertical");
        var mmb = Input.GetMouseButton(MouseButton.Middle);
        var shift = Input.GetKey(KeyboardKeys.Shift);
        if (mmb)
        {
            if (shift)
            {
                inputH = Input.MousePositionDelta.X * 0.1f;
                inputV -= Input.MousePositionDelta.Y * 0.1f;
            }
            else
            {
                var a = Input.MousePositionDelta * Mathf.Pi * Time.UnscaledDeltaTime;
                arm.CameraAngle.X -= a.Y;
                arm.CameraAngle.Y -= a.X;
            }
            Input.MousePosition -= Input.MousePositionDelta;
            ViewportPanel.Instance.MouseCapture(true);
        }
        else
        {

            if(Input.GetKey(KeyboardKeys.Numpad1))
                arm.CameraAngle = new Float2(0, Input.GetKey(KeyboardKeys.Control) ? 180 : 0);
            if (Input.GetKey(KeyboardKeys.Numpad3))
                arm.CameraAngle = new Float2(0, Input.GetKey(KeyboardKeys.Control) ? -90 : 90);
            if (Input.GetKey(KeyboardKeys.Numpad7))
                arm.CameraAngle = new Float2(90, 0);
            if (Input.GetKey(KeyboardKeys.Numpad9))
                arm.CameraAngle = new Float2(-90, 0);

            if (Input.GetKey(KeyboardKeys.Numpad4))
                arm.CameraAngle.Y += Mathf.Pi * 10 * Time.UnscaledDeltaTime;
            if (Input.GetKey(KeyboardKeys.Numpad6))
                arm.CameraAngle.Y -= Mathf.Pi * 10 * Time.UnscaledDeltaTime;
            if (Input.GetKey(KeyboardKeys.Numpad8))
                arm.CameraAngle.X += Mathf.Pi * 10 * Time.UnscaledDeltaTime;
            if (Input.GetKey(KeyboardKeys.Numpad2))
                arm.CameraAngle.X -= Mathf.Pi * 10 * Time.UnscaledDeltaTime;

            ViewportPanel.Instance.MouseCapture(false);



        }

        if (Input.GetMouseButtonDown(MouseButton.Right))
        {
            UIRoot.TerainBrush.Visible = !UIRoot.TerainBrush.Visible;
        }
        var ray = arm.Camera.ConvertMouseToRay(Input.MousePosition);
        bool CanPaint = false;
        if (Physics.RayCast(ray.Position, ray.Direction, out RayCastHit hitInfo))
        {
            Terrain.Instance.BrushPosition.X = hitInfo.Point.X;
            Terrain.Instance.BrushPosition.Y = hitInfo.Point.Z;
            CanPaint = true;
        }
        if (Input.GetMouseButton(MouseButton.Left) && CanPaint)
        {
            Terrain.Instance.Paint();
        }
        Screen.CursorVisible = !mmb;
        Screen.CursorLock = mmb ? CursorLockMode.Clipped : CursorLockMode.None;
        arm.Distance -= Input.MouseScrollDelta * 10;
        Actor.Position += new Vector3(inputH, 0, inputV).Normalized * movespeed * Quaternion.Euler(0, arm.CameraAngle.Y, 0);
        var h = Terrain.Instance.WorldGetHeight(Actor.Position.X, Actor.Position.Z);
        Actor.Position = new Vector3(Actor.Position.X, h, Actor.Position.Z);
    }
}
