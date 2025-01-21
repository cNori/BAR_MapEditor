using System;
using System.IO;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;
namespace Game;

[ExecuteInEditMode]
/// <summary>
/// CameraControll Script.
/// </summary>
public class MapEditor : Script
{
    public float MoveSpeed = 100;
    [Serialize] private float ArmDistance = 0;
    [Serialize] private float moveScroll = 0;
    [Serialize] private Float3 CameraAngleXY;
    [Serialize] private Float3 moveDirection = Float3.Zero;
    private Actor Camera;

    public MapGen map;
    private float BrushSize;
    private float BrushStrength;
    private float BrushFalloff;
    private float BrushMinDistance;
    private Float2 BrushPosition;

    private bool BuildUI;
    private Panel root;
    private VerticalPanel BrushSettingsRoot;
    private Panel InspectorRoot;
    private bool HasMenuOpen = false;
    private ContainerControl gamewindow;
    private AssetView assetView;

    /// <inheritdoc/>
    public override void OnStart()
    {

        BuildUI = true;
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

        bool hasGUIFocus = false;
        if (Input.GetKey(KeyboardKeys.Alt))
        {
            Input.MousePosition = new Float2(Mathf.Clamp(Input.MousePosition.X, 0.0f, Screen.Size.X - InspectorRoot.Size.X), Input.MousePosition.Y);
        }
        else
        {
            if (InspectorRoot != null)
            {
                hasGUIFocus = InspectorRoot.IsMouseOver;
            }
        }
        if (hasGUIFocus)
        {
            Screen.CursorVisible = true;
        }

        if (BuildUI)
        {
            BuildUI = false;

#if FLAX_EDITOR
            gamewindow = Editor.Instance.Windows.GameWin;
#else
            if (gamewindow == null)
                gamewindow = Scene.AddChild<UICanvas>().GUI;
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

            root = gamewindow.AddChild<Panel>();
            root.SetAnchorPreset(AnchorPresets.StretchAll, false);

            //inspector

            //setup panel
            InspectorRoot = root.AddChild<Panel>();
            InspectorRoot.SetAnchorPreset(AnchorPresets.VerticalStretchRight, false);
            InspectorRoot.Size = new Float2(400, InspectorRoot.Size.Y);
            InspectorRoot.Location = new Float2(InspectorRoot.Parent.Size.X - InspectorRoot.Size.X, InspectorRoot.Y);
            InspectorRoot.BackgroundColor = Style.Current.Background;

            var iprop = InspectorRoot.AddChild<VerticalPanel>();
            iprop.SetAnchorPreset(AnchorPresets.StretchAll, false);

            TitleProperty(iprop, "Import Settings");
            TextProperty(iprop, "Height Map Path", (string value) =>
            {
                if (Path.Exists(value))
                {
                    map.HeightMapPath = value;
                    return value;
                }
                else
                {
                    return map.HeightMapPath;
                }
            }, map.HeightMapPath);
            TextProperty(iprop, "Color Map Path", (string value) =>
            {
                if (Path.Exists(value))
                {
                    map.ColorMapPath = value;
                    return value;
                }
                else
                {
                    return map.ColorMapPath;
                }
            }, map.ColorMapPath);
            ButtonProperty(iprop, "Import Maps", () =>
            {
                map.Import();
            });
            TextProperty(iprop, "Asset Path", (string value) =>
            {
                if (Directory.Exists(value))
                {
                    map.AssetDirectory = value;
                    return value;
                }
                else
                {
                    return map.AssetDirectory;
                }
            }, map.AssetDirectory);

            ButtonProperty(iprop, "Load Asset's", () =>
            {
                map.LoadAssets((MapGen.Asset asset) =>
                {
                    var display = assetView.AddChild<AssetView.AssetDisplay>();
                    display.Name = Path.GetFileNameWithoutExtension(asset.Path).Replace('_',' ');
                    //display.Asset = asset;
                    display.RenderThumbnail();
                });
            });

            //object selector
            assetView = iprop.AddChild<AssetView>();
            assetView.Size = new Float2(400, 400);
            //assetView.ScrollBars = ScrollBars.Vertical;
            //assetView.AlwaysShowScrollbars = true;

            //brush settings
            BrushSettingsRoot = root.AddChild<VerticalPanel>();
            BrushSettingsRoot.Visible = false;
            BrushSettingsRoot.Size = new Float2(400, 200);

            BrushSettingsRoot.BackgroundColor = Style.Current.Background;
            TitleProperty(BrushSettingsRoot, "Brush Settings");
            FloatProperty(BrushSettingsRoot, "Size", (float value) => { BrushSize = value; }, BrushSize, true, 0, 500.0f);
            FloatProperty(BrushSettingsRoot, "Strength", (float value) => { BrushStrength = value; }, BrushStrength, true, 0.01f, 1.0f);
            FloatProperty(BrushSettingsRoot, "Falloff", (float value) => { BrushFalloff = value; }, BrushFalloff, true, 0.0f, 1.0f);
            FloatProperty(BrushSettingsRoot, "Min Distance", (float value) => { BrushMinDistance = value; }, BrushMinDistance, true, 0.0f, 100.0f);
            //spacer
            TitleProperty(BrushSettingsRoot, "");

        }

        if (Engine.HasGameViewportFocus && !hasGUIFocus)
        {
            Camera = Actor.GetChild(0);
            if (Input.MouseScrollDelta != 0)
            {
                moveScroll -= Input.MouseScrollDelta * 10;
            }
            if (Mathf.Abs(moveScroll) > 0.001f)
            {
                moveScroll += moveScroll * -10.0f * Time.UnscaledDeltaTime;
            }
            else
            {
                moveScroll = 0;
            }
            var mouseX = Mathf.Remap(Input.MousePosition.X, 0.0f, Screen.Size.X - InspectorRoot.Size.X, -2.0f, 2.0f);
            var mouseY = Mathf.Remap(Input.MousePosition.Y, 0.0f, Screen.Size.Y, -2.0f, 2.0f);
            var mouseMoveX = Mathf.Clamp(Mathf.Abs(mouseX) - 1.75f, 0, 1) * Mathf.Sign(mouseX) * 50.0f;
            var mouseMoveY = -Mathf.Clamp(Mathf.Abs(mouseY) - 1.75f, 0, 1) * Mathf.Sign(mouseY) * 50.0f;

            var speedmulty = ArmDistance / 1000.0f;
            var inputH = Input.GetAxis("Horizontal") * speedmulty;
            var inputV = Input.GetAxis("Vertical") * speedmulty;

            if (Input.GetKey(KeyboardKeys.Alt))
            {
                Screen.CursorVisible = false;
                Screen.CursorLock = CursorLockMode.Clipped;
                CameraAngleXY.X -= mouseMoveX * Time.UnscaledDeltaTime * 5.0f;
                CameraAngleXY.Y += mouseMoveY * Time.UnscaledDeltaTime * 5.0f;

                if (MOpenCords == Vector2.Zero)
                {
                    MOpenCords = Input.MousePosition;
                }
            }
            else
            {
                Screen.CursorVisible = HasMenuOpen;
                Screen.CursorLock = CursorLockMode.None;

                if (MOpenCords != Vector2.Zero && !HasMenuOpen)
                {
                    Input.MousePosition = MOpenCords;
                    MOpenCords = Vector2.Zero;
                }
                PaintSurface();
            }
            moveDirection.X += inputH * Time.UnscaledDeltaTime * MoveSpeed;
            moveDirection.Z += inputV * Time.UnscaledDeltaTime * MoveSpeed;
            float h = map.SampleHeightMapWorld(Actor.Position.X, Actor.Position.Z);
            moveDirection.Y = Mathf.Clamp(h - Actor.Position.Y, -100f, 100f) / 10.0f;


            //move dir gravity
            if (Mathf.Abs(moveDirection.X) > 0.001f)
            {
                moveDirection.X -= moveDirection.X * (MoveSpeed / 20) * Time.UnscaledDeltaTime;
            }
            else
            {
                moveDirection.X = 0;
            }
            if (Mathf.Abs(moveDirection.Z) > 0.001f)
            {
                moveDirection.Z -= moveDirection.Z * (MoveSpeed / 20) * Time.UnscaledDeltaTime;
            }
            else
            {
                moveDirection.Z = 0;
            }

            if (Mathf.Abs(moveDirection.Y) > 0.001f)
            {
                moveDirection.Y -= moveDirection.Y * Time.UnscaledDeltaTime;
            }
            else
            {
                moveDirection.Y = 0;
            }



            Actor.Position += moveDirection * Quaternion.Euler(0, CameraAngleXY.X, 0);


            ArmDistance += moveScroll;
            ArmDistance = Mathf.Clamp(ArmDistance, 20.0f, 10000.0f);
            Quaternion rot = Quaternion.Euler(CameraAngleXY.Y, CameraAngleXY.X, 0);

            Camera.Position = Actor.Position - new Vector3(0, 0, ArmDistance) * Quaternion.Euler(CameraAngleXY.Y, CameraAngleXY.X, 0);

            Camera.Orientation = rot;
        }


        //if(root != null)
        //    root.Location = Input.MousePosition;


    }
    private Vector2 MOpenCords;
    private void PaintSurface()
    {
        map.MaterialSetBrushParamiters(BrushSize, BrushStrength, BrushFalloff);
        if (BrushSettingsRoot != null)
        {
            if (Input.GetMouseButtonDown(MouseButton.Middle))
            {
                HasMenuOpen = !HasMenuOpen;
                if (HasMenuOpen)
                {
                    MOpenCords = Input.MousePosition;
                    BrushSettingsRoot.Location = Input.MousePosition;
                }
                else
                {
                    if (MOpenCords != Vector2.Zero)
                    {
                        Input.MousePosition = MOpenCords;
                        MOpenCords = Vector2.Zero;
                    }
                }

            }
            BrushSettingsRoot.Visible = HasMenuOpen;
            if (BrushSettingsRoot.Visible)
            {
                return;
            }
        }


        var ray = (Camera as Camera).ConvertMouseToRay(Input.MousePosition);
        if (Physics.RayCast(ray.Position, ray.Direction, out RayCastHit hitInfo))
        {
            //DebugDraw.DrawBox(new BoundingBox(-Vector3.One + hitInfo.Point, Vector3.One + hitInfo.Point), Color.Black);
            map.MaterialSetBrushPosition(hitInfo.Point.X, hitInfo.Point.Z);
            BrushPosition.X = hitInfo.Point.X;
            BrushPosition.Y = hitInfo.Point.Z;
            if (Input.GetMouseButton(MouseButton.Left))
            {
                var selected = assetView.GetSelectedAssets();
                if (selected.Count == 0)
                    return;

                var size = BrushSize * 0.5f;
                RNGState = (uint)(BrushPosition.X + BrushPosition.Y);


                int pid = 1;
                var space = (10 + (int)BrushMinDistance);
                Float2[] points = new Float2[(int)(BrushSize * BrushSize)];
                points[0] = new Float2(BrushPosition.X, BrushPosition.Y);

                //bool o = false;
                for (int x = Mathf.CeilToInt(BrushPosition.X - size); x < Mathf.CeilToInt(BrushPosition.X + size); x += space)
                {
                    for (int y = Mathf.CeilToInt(BrushPosition.Y - size); y < Mathf.CeilToInt(BrushPosition.Y + size); y += space)
                    {
                        PRNG_Next();
                        float rx = (((float)RNGState / uint.MaxValue) - 0.5f) * space;
                        PRNG_Next();
                        float ry = (((float)RNGState / uint.MaxValue) - 0.5f) * space;
                        var uv = new Float2(x + rx + (x - BrushPosition.X), y + ry + (y - BrushPosition.Y));
                        if (SampleBrushObjectPlacment(uv))
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
                    PRNG_Next();
                    var o = Mathf.FloorToInt((((float)RNGState / uint.MaxValue)) * selected.Count);

                    Float3 p = new Float3(points[i].X, map.SampleHeightMapWorld(points[i].X, points[i].Y), points[i].Y);
                    //map.PlaceAsset(selected[Mathf.Clamp(o,0, selected.Count-1)],p,0);
                    //DebugDraw.DrawBox(new BoundingBox(-Vector3.One + p, Vector3.One + p), Color.Green);
                }
            }
        }
    }
    public void TitleProperty(VerticalPanel parent, string Name)
    {
        var hp = parent.AddChild<HorizontalPanel>();
        hp.Size = new Float2(400, 24);
        var label = hp.AddChild<Label>();
        label.SetAnchorPreset(AnchorPresets.StretchAll, false);
        label.Text = Name;
    }
    public void FloatProperty(VerticalPanel parent, string Name, Action<float> OnValueChanged, float DefaultValue, bool ShowSlider, float min = 0, float max = 100)
    {
        var hp = parent.AddChild<HorizontalPanel>();
        hp.Size = new Float2(400, 24);
        var label = hp.AddChild<Label>();
        var value = hp.AddChild<TextBox>();
        label.Size = new Float2(100, 24);
        value.Size = new Float2(50, 24);
        label.SetAnchorPreset(AnchorPresets.MiddleLeft, true);
        value.SetAnchorPreset(AnchorPresets.MiddleRight, true);

        value.Text = DefaultValue.ToString();

        if (ShowSlider)
        {
            var slider = hp.AddChild<Slider>();
            slider.Value = DefaultValue;
            slider.Size = new Float2(250, 24);
            slider.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
            var of = -10;
            slider.Size = new Float2(slider.Size.X + of, slider.Size.Y);
            slider.X += (of * 0.5f);
            slider.ValueChanged += () =>
            {
                value.Text = slider.Value.ToString();
                OnValueChanged(slider.Value);
            };
            value.EditEnd += () =>
            {
                if (float.TryParse(value.Text, out float fvalue))
                {
                    slider.Value = fvalue;
                    OnValueChanged(slider.Value);
                }
                else
                {
                    value.Text = slider.Value.ToString();
                }
            };
            slider.Minimum = min;
            slider.Maximum = max;
        }
        else
        {
            value.EditEnd += () =>
            {
                if (float.TryParse(value.Text, out float fvalue))
                {
                    OnValueChanged(fvalue);
                }
                //else
                //{
                //    value.Text = slider.Value.ToString();
                //}
            };
        }
        label.Text = Name;
    }
    public void TextProperty(VerticalPanel parent, string Name, Func<string,string> OnValueChanged, string DefaultValue)
    {
        var hp = parent.AddChild<HorizontalPanel>();
        hp.Size = new Float2(400, 24);
        var label = hp.AddChild<Label>();
        var value = hp.AddChild<TextBox>();
        label.Size = new Float2(100, 24);
        label.SetAnchorPreset(AnchorPresets.MiddleLeft, true);

        value.Size = new Float2(300, 24);

        value.SetAnchorPreset(AnchorPresets.MiddleLeft, true);
        parent.PerformLayout();
        var l = value.Left;
        value.SetAnchorPreset(AnchorPresets.MiddleRight, true);
        parent.PerformLayout();
        value.SetAnchorPreset(AnchorPresets.StretchAll, true);
        var b = value.Bounds;
        var dif = b.Location.X - l;
        b.Location.X = l;
        b.Size.X += dif;
        value.Bounds = b;

        //var of = -(hp.Spacing) * 2;
        //value.Size = new Float2(value.Size.X + hp.Spacing, value.Size.Y);
        //value.X += (hp.Spacing * 0.5f);

        //value.Size = new Float2(50, 24);
        value.Text = DefaultValue;
        value.EditEnd += () =>
        {
            value.Text = OnValueChanged(value.Text);
        };
        label.Text = Name;
    }
    public void ButtonProperty(VerticalPanel parent, string Name, Action OnClicked)
    {
        var hp = parent.AddChild<HorizontalPanel>();
        hp.Size = new Float2(400, 24);
        var value = hp.AddChild<Button>();
        value.Size = new Float2(400, 24);
        value.SetAnchorPreset(AnchorPresets.MiddleLeft, true);
        parent.PerformLayout();
        var l = value.Left;
        value.SetAnchorPreset(AnchorPresets.MiddleRight, true);
        parent.PerformLayout();
        value.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
        var b = value.Bounds;
        var dif = b.Location.X - l;
        b.Location.X = l;
        b.Size.X += dif;
        value.Bounds = b;
        value.Text = Name;
        value.Clicked += () =>
        {
            OnClicked();
        };
    }

    uint RNGState = 1804289383;

    public bool SampleBrushObjectPlacment(Float2 cords)
    {
        var uv = BrushPosition - cords;
        float cg = 1 - (uv * 2).Length;
        float Falloff = BrushFalloff * BrushSize;
        float sample = Mathf.Clamp(((cg - (1 - (BrushSize - Falloff))) + Falloff) / Falloff, 0, 1) * BrushStrength;

        Color color = Color.Green;


        PRNG_Next();

        float canplace = ((float)RNGState / uint.MaxValue);

        if (canplace < sample)
        {
            return true;
        }
        return false;
    }
    public void PRNG_Next()
    {
        uint Number = RNGState;
        Number ^= Number << 13;
        Number ^= Number >> 17;
        Number ^= Number << 5;
        RNGState = Number;
    }
}
