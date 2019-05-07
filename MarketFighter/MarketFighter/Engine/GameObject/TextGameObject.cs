using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MarketFighter.Engine;

namespace MarketFighter.Engine.GameObject
{
    public abstract class TextGameObject : BaseGameObject<TextBlock>
    {

        private string _Text;

        private Action SetFontFamilyAct;
        private Action SetFontColorAct;
        private Action SetFontSizeAct;
        private Action SetFontWeightAct;
        private Action SetFontStyleAct;

        public double Width { get; private set; }
        public double Height { get; private set; }

        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text == value) return;
                _Text = value;
                if (_SpriteView != null)
                    RunOnUIThread(new Action(() => _SpriteView.Text = _Text));
            }
        }

        private FontFamily _FontFamily;
        public FontFamily FontFamily
        {
            get { return _FontFamily; }
            set
            {
                _FontFamily = value;
                if (_SpriteView != null)
                    RunOnUIThread(SetFontFamilyAct);
            }
        }

        private Brush _FontColor;
        public Brush FontColor
        {
            get { return _FontColor; }
            set
            {
                _FontColor = value;
                if (_SpriteView != null)
                    RunOnUIThread(SetFontColorAct);
            }
        }

        private double _FontSize;
        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                _FontSize = value;
                if (_SpriteView != null)
                    RunOnUIThread(SetFontSizeAct);
            }
        }

        private FontWeight _FontWeight;
        public FontWeight FontWeight
        {
            get { return _FontWeight; }
            set
            {
                _FontWeight = value;
                if (_SpriteView != null)
                    RunOnUIThread(SetFontWeightAct);
            }
        }

        private FontStyle _FontStyle;
        public FontStyle FontStyle
        {
            get { return _FontStyle; }
            set
            {
                _FontStyle = value;
                if (_SpriteView != null)
                    RunOnUIThread(SetFontStyleAct);
            }
        }

        public TextGameObject(string id) : base()
        {
            SetFontFamilyAct = new Action(SetFontFamily);
            SetFontColorAct = new Action(SetFontColor);
            SetFontSizeAct = new Action(SetFontSize);
            SetFontWeightAct = new Action(SetFontWeight);
            SetFontStyleAct = new Action(SetFontStyle);

            SpriteInitialized += () => 
            {
                if (Text != null)
                    _SpriteView.Text = Text;
                if (FontFamily != null)
                    _SpriteView.FontFamily = FontFamily;
                if (FontColor != null)
                    _SpriteView.Foreground = FontColor;
                if (FontSize != 0)
                    _SpriteView.FontSize = FontSize;
                if (FontWeight != null)
                    _SpriteView.FontWeight = FontWeight;
                if (FontStyle != null)
                    _SpriteView.FontStyle = FontStyle;

                _SpriteView.SizeChanged += _SpriteView_SizeChanged;
            };
        }

        private void _SpriteView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = e.NewSize.Width;
            Height = e.NewSize.Height;
            CenterToSprite();
        }

        public TextGameObject() : this(null)
        {

        }

        public override void CenterToSprite()
        {
            if (_SpriteView == null) return;
            CenterX = _SpriteView.ActualWidth * ScaleX / 2;
            CenterY = _SpriteView.ActualHeight * ScaleY / 2;
        }

        private void SetFontFamily()
        {
            _SpriteView.Foreground = _FontColor;
            CenterToSprite();
        }

        private void SetFontColor()
        {
            _SpriteView.Foreground = _FontColor;
            CenterToSprite();
        }

        private void SetFontSize()
        {
            _SpriteView.FontSize = _FontSize;
            CenterToSprite();
        }

        private void SetFontWeight()
        {
            _SpriteView.FontWeight = _FontWeight;
            CenterToSprite();
        }

        private void SetFontStyle()
        {
            _SpriteView.FontStyle = _FontStyle;
            CenterToSprite();
        }

    }
}
