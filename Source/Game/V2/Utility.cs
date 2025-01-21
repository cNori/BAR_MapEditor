using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;
public static class Utility
{
    public static class UI
    {
        public const float PropertyHeight = 24f;

        public static void TitleProperty(VerticalPanel parent, string Name)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Size = new Float2(400, 24);
            var label = hp.AddChild<Label>();
            label.SetAnchorPreset(AnchorPresets.StretchAll, false);
            label.Text = Name;
        }
        public static void FloatProperty(VerticalPanel parent, string Name, Action<float> OnValueChanged, float DefaultValue, bool ShowSlider, float min = 0, float max = 100)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Width = parent.Width;
            hp.Height = PropertyHeight;
            var label = hp.AddChild<Label>();
            hp.Width = 100;
            hp.Height = PropertyHeight;
            label.HorizontalAlignment = TextAlignment.Near;
            var value = hp.AddChild<TextBox>();
            value.Height = PropertyHeight;
            value.X += label.Width;
            value.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
            value.Text = DefaultValue.ToString();


            if (ShowSlider)
            {
                var slider = hp.AddChild<Slider>();
                slider.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
                value.Width = hp.Width - slider.Width - label.Width - slider.ThumbSize.X;
                value.X = slider.X + slider.Width + slider.ThumbSize.X;
                slider.Value = DefaultValue;

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
                value.Width = hp.Width - label.Width;

                value.EditEnd += () =>
                {
                    if (float.TryParse(value.Text, out float fvalue))
                    {
                        OnValueChanged(fvalue);
                    }
                };
            }
            label.Text = Name;
        }

        public static void IntProperty(VerticalPanel parent, string Name, Action<int> OnValueChanged, float DefaultValue, bool ShowSlider, int min = 0, int max = 100)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Width = parent.Width;
            hp.Height = PropertyHeight;
            var label = hp.AddChild<Label>();
            hp.Width = 100;
            hp.Height = PropertyHeight;
            label.HorizontalAlignment = TextAlignment.Near;
            var value = hp.AddChild<TextBox>();
            value.Height = PropertyHeight;
            value.X += label.Width;
            value.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
            value.Text = DefaultValue.ToString();


            if (ShowSlider)
            {
                var slider = hp.AddChild<Slider>();
                slider.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
                value.Width = hp.Width - slider.Width - label.Width - slider.ThumbSize.X;
                value.X = slider.X + slider.Width + slider.ThumbSize.X;
                slider.Value = DefaultValue;
                slider.WholeNumbers = true;
                slider.ValueChanged += () =>
                {
                    value.Text = ((int)slider.Value).ToString();
                    OnValueChanged((int)slider.Value);
                };
                value.EditEnd += () =>
                {
                    if (int.TryParse(value.Text, out int fvalue))
                    {
                        slider.Value = fvalue;
                        OnValueChanged((int)slider.Value);
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
                value.Width = hp.Width - label.Width;

                value.EditEnd += () =>
                {
                    if (int.TryParse(value.Text, out int fvalue))
                    {
                        OnValueChanged(fvalue);
                    }
                };
            }
            label.Text = Name;
        }
        public static void TextProperty(VerticalPanel parent, string Name, Func<string, string> OnValueChanged, string DefaultValue)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Width = parent.Width;
            hp.Height = PropertyHeight;
            var label = hp.AddChild<Label>();
            hp.Width = 100;
            hp.Height = PropertyHeight;
            label.HorizontalAlignment = TextAlignment.Near;
            var value = hp.AddChild<TextBox>();
            value.Width = hp.Width - label.Width;
            value.Height = PropertyHeight;
            value.X += label.Width;
            value.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
            value.Text = DefaultValue;
            value.EditEnd += () =>
            {
                value.Text = OnValueChanged(value.Text);
            };
            label.Text = Name;
        }
        public static void ButtonProperty(VerticalPanel parent, string Name, Action OnClicked)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Width = parent.Width;
            hp.Height = PropertyHeight;
            var value = hp.AddChild<Button>();
            value.Width = hp.Width;
            value.Height = hp.Height;

            value.Text = Name;
            value.SetAnchorPreset(AnchorPresets.HorizontalStretchMiddle, true);
            value.Clicked += () =>
            {
                OnClicked();
            };
        }


        public static void CheakBoxProperty(VerticalPanel parent, string Name, Action<bool> StateChanged)
        {
            var hp = parent.AddChild<HorizontalPanel>();
            hp.Width = parent.Width;
            hp.Height = PropertyHeight;
            var label = hp.AddChild<Label>();
            hp.Width = 100;
            hp.Height = PropertyHeight;
            label.HorizontalAlignment = TextAlignment.Near;
            label.Text = Name;
            var value = hp.AddChild<CheckBox>();
            value.Height = PropertyHeight;
            value.X = hp.Width - value.BoxSize;
            value.Width = value.BoxSize;
            value.SetAnchorPreset(AnchorPresets.MiddleRight, true);

            value.StateChanged += (CheckBox cb) => 
            {
                StateChanged(cb.Checked);
            };

            //value.Text = DefaultValue.ToString();
        }
    }


    public class Random
    {
        private uint RNGState = 1804289383;

        public Random(uint State)
        {
            RNGState = State;
        }

        public void Next()
        {
            uint Number = RNGState;
            Number ^= Number << 13;
            Number ^= Number >> 17;
            Number ^= Number << 5;
            RNGState = Number;
        }

        public float Range(float min, float max)
        {
            return Mathf.Remap(Get01(), 0, 1, min, max);
        }
        public float Get01()
        {
            return (float)RNGState / uint.MaxValue;
        }
    }

    public static int Index2D(int x, int y, int SizeX, int SizeY)
    {
        return (y % SizeY) * SizeX + (x % SizeX);
    }

    public static void DrawPoint(Vector3 point,Color color,float size = 1.0f)
    {
        DebugDraw.DrawBox(new BoundingBox(point - Vector3.One * size, point + Vector3.One * size), color);
    }
}
