using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;
namespace Game;

public class AssetView : ContainerControl
{
    public class AssetDisplay : Control
    {
        public bool IsSelected;
        public string Name;
        public int AssetID;
        internal GPUTexture Thumbnail;
        public override void Draw()
        {
            if(Thumbnail == null)
                RenderThumbnail();

            var style = Style.Current;
            var size = Size;
            var clientRect = new Rectangle(Float2.Zero, size);

            var textHeight = DefaultTextHeight * size.X / DefaultWidth;
            var textRect = new Rectangle(0, size.Y - textHeight, size.X, textHeight);

            // Small shadow
            var shadowRect = new Rectangle(2, 2, clientRect.Width + 1, clientRect.Height + 1);
            var color = Color.Black.AlphaMultiplied(0.2f);
            Render2D.FillRectangle(shadowRect, color);

            Render2D.FillRectangle(clientRect, style.Background.RGBMultiplied(1.25f));
            Render2D.FillRectangle(textRect, style.LightBackground);
            var accentHeight = 2;
            var barRect = new Rectangle(0, 4 + AssetView.DefaultThumbnailSize, clientRect.Width, accentHeight);
            Render2D.FillRectangle(barRect, IsSelected ? style.BackgroundSelected : Color.DimGray);

            if(Thumbnail != null)
            {
                Render2D.DrawTexture(Thumbnail, new Rectangle(4, 4, AssetView.DefaultThumbnailSize, AssetView.DefaultThumbnailSize));
            }
            if (IsSelected)
            {
                Render2D.FillRectangle(textRect, Parent.ContainsFocus ? style.BackgroundSelected : style.LightBackground);
                Render2D.DrawRectangle(clientRect, Parent.ContainsFocus ? style.BackgroundSelected : style.LightBackground);
            }
            else if (IsMouseOver)
            {
                Render2D.FillRectangle(textRect, style.BackgroundHighlighted);
                Render2D.DrawRectangle(clientRect, style.BackgroundHighlighted);
            }

            Render2D.PushClip(ref textRect);
            Render2D.DrawText(style.FontMedium, Name, textRect, style.Foreground, TextAlignment.Center, TextAlignment.Center, TextWrapping.WrapWords, 1f, 0.95f);
            Render2D.PopClip();
        }
        
        public override bool OnMouseDown(Float2 location, MouseButton button)
        {
            IsSelected = !IsSelected;
            return base.OnMouseDown(location, button);
        }
        public void RenderThumbnail()
        {
            if (Thumbnail == null)
                Thumbnail = new GPUTexture();
            var desc = GPUTextureDescription.New2D(
                (int)AssetView.DefaultThumbnailSize,
                (int)AssetView.DefaultThumbnailSize,
                PixelFormat.R8G8B8A8_UNorm);
            Thumbnail.Init(ref desc);
            Quaternion rot = new Quaternion(-0.08f, -0.92f, 0.31f, -0.23f);
            float orbitRadius = 50.0f;
            
            SceneRenderTask task = new()
            {
                Output = Thumbnail,
                //IsCustomRendering = false,
                ActorsSource = ActorsSources.CustomActors,
                Camera = new Camera
                {
                    Orientation = rot,
                    Position = (Vector3.Forward * rot) * (-1f * orbitRadius)
                }
            };
            StaticModel staticmodel = new()
            {
                Model = Import.Assets[AssetID].ModelDisplay
            };
            
            float targetSize = 50.0f;
            BoundingBox box = staticmodel.Model.GetBox();
            float maxSize = Mathf.Max(0.001f, (float)box.Size.MaxValue);
            float scale = targetSize / maxSize;
            staticmodel.Scale = new Float3(scale);
            staticmodel.Position = box.Center * (-0.5f * scale) + new Vector3(0, -10, 0);

            // Setup preview scene
            var previewLight = new DirectionalLight
            {
                Brightness = 8.0f,
                ShadowsMode = ShadowsCastingMode.None,
                Orientation = Quaternion.Euler(new Vector3(52.1477f, -109.109f, -111.739f))
            };
            var envProbe = new EnvironmentProbe
            {
                //CustomProbe = FlaxEngine.Content.LoadAsyncInternal<CubeTexture>(Engine.)
            };
            var sky = new Sky
            {
                SunLight = previewLight,
                SunPower = 9.0f
            };
            var skyLight = new SkyLight
            {
                Mode = SkyLight.Modes.CustomTexture,
                Brightness = 2.1f,
                CustomTexture = envProbe.CustomProbe
            };

            task.AddCustomActor(previewLight);
            task.AddCustomActor(envProbe);
            task.AddCustomActor(sky);
            task.AddCustomActor(skyLight);
            task.AddCustomActor(staticmodel);
            task.AddCustomActor(staticmodel);
            task.Enabled = true;

            void Task_PostRender(GPUContext arg0, ref RenderContext arg1)
            {
                task.Enabled = false;
                FlaxEngine.Object.Destroy(ref task);
            }

            task.PostRender += Task_PostRender;

            //SceneRenderTask _task;
            //GPUTexture _output;

            //// Create render task but disabled for now
            //_output = GPUDevice.Instance.CreateTexture();
            //var desc = GPUTextureDescription.New2D(AssetView.DefaultThumbnailSize, AssetView.DefaultThumbnailSize, PixelFormat.R8G8B8A8_UNorm);
            //_output.Init(ref desc);
            //_task = FlaxEngine.Object.New<SceneRenderTask>();
            //_task.Order = 50; // Render this task later
            //_task.Enabled = false;
            //_task.Output = _output;

            //void OnRender(RenderTask task, GPUContext context)
            //{
            //    Debug.Log("im good");
            //    SceneRenderTask Task = (SceneRenderTask)task;
            //    float orbitRadius = 200.0f;
            //    Task.Camera.Orientation = new Quaternion(-0.08f, -0.92f, 0.31f, -0.23f);
            //    Task.Camera.Position = Task.Camera.Rotation.Forward * (-1f * orbitRadius);
            //    StaticModel staticmodel = new StaticModel();
            //    staticmodel.Model = Asset.model;
            //    Task.AddCustomActor(staticmodel);

            //}

            //_task.Render += OnRender;
            //_task.End += (RenderTask task, GPUContext context) =>
            //{
            //    SceneRenderTask Task = (SceneRenderTask)task;
            //    Thumbnail = Task.Output;
            //};
            //Renderer.Render(task);
        }
    }


    public const int DefaultMarginSize = 4;
    public const int DefaultTextHeight = 42;
    public const int DefaultThumbnailSize = 64;
    public const int DefaultWidth = (DefaultThumbnailSize + 2 * DefaultMarginSize);
    public const int DefaultHeight = (DefaultThumbnailSize + 2 * DefaultMarginSize + DefaultTextHeight);

    protected override void PerformLayoutBeforeChildren()
    {
        float width = GetClientArea().Width;
        float x = 0, y = 1;
        float ItemWidth  = DefaultWidth * 1f;
        float ItemHeight = DefaultHeight * 1f;
        int itemsToFit = Mathf.FloorToInt(width / ItemWidth) - 1;
        if (itemsToFit < 1)
            itemsToFit = 1;
        x = DefaultMarginSize;

        for (int i = 0; i < _children.Count; i++)
        {
            var c = _children[i];
            c.Bounds = new Rectangle(x, y, ItemWidth, ItemHeight);

            x += ItemWidth + DefaultMarginSize;
            if (x + ItemWidth > width)
            {
                x = DefaultMarginSize;
                y += ItemHeight + DefaultMarginSize;
            }
        }
        if (HasParent)
            Width = Parent.GetClientArea().Width;
        Height = y;
        base.PerformLayoutBeforeChildren();
    }


    public List<int> GetSelectedAssets()
    {
        List<int> selectedAssets = [];
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is AssetView.AssetDisplay asset)
            {
                if (asset.IsSelected)
                    selectedAssets.Add(asset.AssetID);
            }
        }
        return selectedAssets;
    }
}