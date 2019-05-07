using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MarketFighter.Engine.GameObject
{
    public interface IGameObject : IDisposable
    {

        event Action SpriteInitialized;

        void OnLoad();

        void OnInitialize();

        void Run();

        System.Windows.UIElement GetSprite();

        //void SetImage(ImageSource image);

        string ID { get; }

        string Name { get; set; }

        bool Loaded { get; set; }

        bool Initialized { get; set; }

        System.Windows.Media.Effects.Effect Effect { get; set; }

        bool EffectChanged { get; }

        System.Windows.Visibility Visibility { get; set; }

        bool VisibilityChanged { get; }

        double Opacity { get; set; }

        bool OpacityChanged { get; }

        double ScaleX { get; set; }

        double ScaleY { get; set; }

        bool ScaleChanged { get; }

        double X { get; }

        double Y { get; }

        void SetPosition(double x, double y);

        double CenterX { get; set; }

        double CenterY { get; set; }

        double AnimationOffsetX { get; set; }

        double AnimationOffsetY { get; set; }

        double AnimationRotation { get; set; }

        double Speed { get; }

        double SpeedX { get; set; }

        double SpeedY { get; set; }

        double ActualSpeedX { get; }

        double ActualSpeedY { get; }

        double ActualDirection { get; }

        double Direction { get; set; }

        bool DirectionChanged { get; }

        void Move(double x, double y);

        MoveTask MoveTask {get; set;}

        bool RotatableSprite { get; set; }

        long GetAge();

        int Waiting { get; set; }

        long WaitCheckpoint { get; set; }

        bool InGame { get; set; } //If this object is in GameManager

        void Enter(); //When this object added into GameManager

        void Left(); //When this object removed from GameManager

        void SetActualScale(double scaleX, double scaleY);

        void SetActualRotate(double angle);

        void SetActualOpacity(double opacity);

        void SetActualVisibility(System.Windows.Visibility visibility);

        void SetActualEffect(System.Windows.Media.Effects.Effect effect);

        bool Disposed { get; set; }
    }
}
