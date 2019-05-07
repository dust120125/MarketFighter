using MarketFighter.Engine.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace MarketFighter.Engine.GameObject
{
    public abstract class BaseGameObject<T> : IGameObject where T : UIElement
    {

        protected T _SpriteView;
        protected Action<Visibility> SetVisibility;

        private Action SetEffectAct;

        public event Action SpriteInitialized;
        public event Action EnterGame;
        public event Action LeftGame;

        private long Birth;
        public bool AlignCenter = true;
        protected DateTime latestMoveTime, latestMoveTimeY;

        protected readonly string _ID;

        public string ID { get { return _ID; } }

        public string Name { get; set; }

        public bool Loaded { get; set; }

        public bool Initialized { get; set; }

        protected Visibility _Visibility = Visibility.Visible;

        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                if (_Visibility == value) return;

                _Visibility = value;
                VisibilityChanged = true;
            }
        }

        public bool VisibilityChanged { get; private set; }

        private double _Opacity;

        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                _Opacity = value;
                OpacityChanged = true;
            }
        }

        public bool OpacityChanged { get; private set; }

        protected double latestX, latestY;

        protected double _ScaleX = 1;
        public double ScaleX
        {
            get { return _ScaleX; }
            set
            {
                if (value == _ScaleX) return;
                _ScaleX = value;
                ScaleChanged = true;
                if (AlignCenter) CenterToSprite();
            }
        }

        protected double _ScaleY = 1;
        public double ScaleY
        {
            get { return _ScaleY; }
            set
            {
                if (value == _ScaleY) return;
                _ScaleY = value;
                ScaleChanged = true;
                if (AlignCenter) CenterToSprite();
            }
        }

        public bool ScaleChanged { get; protected set; }

        protected double _X;
        public double X
        {
            get { return _X; }
        }

        protected double _Y;
        public double Y
        {
            get { return _Y; }
        }

        public void SetPosition(double x, double y)
        {
            latestMoveTime = DateTime.Now;
            latestX = _X;
            latestY = _Y;
            _X = x;
            _Y = y;
        }

        public double ActualSpeedX
        {
            get
            {
                double interval = (DateTime.Now - latestMoveTime).Milliseconds;
                interval = interval > 0 ? interval : 1;
                double multiple = GameManager.ProcessingRate / interval;
                return (X - latestX) * multiple;
            }
        }

        public double ActualSpeedY
        {
            get
            {
                int interval = (DateTime.Now - latestMoveTime).Milliseconds;
                interval = interval > 0 ? interval : 1;
                double multiple = GameManager.ProcessingRate / interval;
                return (Y - latestY) * multiple;
            }
        }

        public double CenterX { get; set; }

        public double CenterY { get; set; }

        public double AnimationOffsetX { get; set; }

        public double AnimationOffsetY { get; set; }

        public double AnimationRotation { get; set; }

        public double Speed
        {
            get
            {
                return Math.Sqrt(Math.Pow(SpeedX, 2) + Math.Pow(SpeedY, 2));
            }
        }

        public double SpeedX { get; set; }

        public double SpeedY { get; set; }

        public MoveTask MoveTask { get; set; }

        public double ActualDirection { get; set; }

        protected double _Direction;
        public double Direction
        {
            get { return _Direction; }
            set
            {
                if (value < 0) _Direction = value + 360;
                else _Direction = value % 360;
                DirectionChanged = true;
            }
        }

        public bool DirectionChanged { get; protected set; }

        public bool RotatableSprite { get; set; }
        public bool Disposed { get; set; }

        public int Waiting { get; set; }

        public long WaitCheckpoint { get; set; }

        public bool InGame { get; set; }

        private Effect _Effect;
        public Effect Effect
        {
            get { return _Effect; }
            set
            {
                _Effect = value;
                EffectChanged = true;
            }
        }

        public bool EffectChanged { get; private set; }

        protected RotateTransform _RotateTransform;

        protected ScaleTransform _ScaleTransform;

        //########## Methods ##########

        public BaseGameObject() : this(null)
        {

        }

        public BaseGameObject(string id)
        {
            _ID = id;
            GameEngine.RunOnUIThread(new Action(InitSprite));
            Birth = GameEngine.GetMillisecond();

            SetVisibility = new Action<Visibility>(
                delegate (Visibility visibility)
                {
                    _SpriteView.Visibility = Visibility;
                });
        }

        private void InitSprite()
        {
            _ScaleTransform = new ScaleTransform(ScaleX, ScaleY);
            _RotateTransform = new RotateTransform(Direction);

            TransformGroup group = new TransformGroup();
            group.Children.Add(_ScaleTransform);
            group.Children.Add(_RotateTransform);

            _SpriteView = CreateSprite();
            _SpriteView.RenderTransform = group;
            _SpriteView.Visibility = _Visibility;

            SpriteInitialized?.Invoke();
        }

        protected virtual T CreateSprite()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public abstract void OnLoad();
        public abstract void OnInitialize();
        public abstract void Dispose();
        public abstract void Run();

        public void RegisterAnimation(Animation.AnimationProvider provider)
        {
            GameEngine._GameManager.RegisterAnimation(this, provider);
        }

        public UIElement GetSprite()
        {
            return _SpriteView;
        }

        public virtual void CenterToSprite()
        {
            CenterX = _SpriteView.RenderSize.Width / 2;
            CenterY = _SpriteView.RenderSize.Height / 2;
        }

        public void Move(double x, double y)
        {
            SetPosition(_X + x, _Y + y);
        }

        public virtual long GetAge()
        {
            return GameEngine.GetMillisecond() - Birth;
        }

        public void SetSpeed(double speed, double direction)
        {
            double objectDirection;
            if (speed != 0)
            {
                objectDirection = direction * GameEngine.RADIAN;
                SpeedX = Math.Cos(objectDirection) * speed;
                SpeedY = Math.Sin(objectDirection) * speed;
            }
            else
            {
                SpeedX = 0;
                SpeedY = 0;
            }
        }

        public virtual void SetActualScale(double scaleX, double scaleY)
        {
            _ScaleTransform.ScaleX = scaleX;
            _ScaleTransform.ScaleY = scaleY;
            if (AlignCenter) CenterToSprite();
            ScaleChanged = false;
        }

        public void SetActualRotate(double angle)
        {
            ActualDirection = angle;
            _RotateTransform.CenterX = CenterX;
            _RotateTransform.CenterY = CenterY;
            _RotateTransform.Angle = angle;
            DirectionChanged = false;
        }

        public void SetActualOpacity(double opacity)
        {
            _SpriteView.Opacity = opacity;
            OpacityChanged = false;
        }

        public void SetActualVisibility(Visibility visibility)
        {
            _SpriteView.Visibility = Visibility;
            VisibilityChanged = false;
        }

        public void SetActualEffect(Effect effect)
        {
            _SpriteView.Effect = effect;
            EffectChanged = false;
        }

        public double Distance(IGameObject go)
        {
            return Math.Sqrt(Math.Pow(X - go.X, 2) + Math.Pow(Y - go.Y, 2));
        }

        public double Distance(double X, double X2, double Y, double Y2)
        {
            return Math.Sqrt(Math.Pow(X - X2, 2) + Math.Pow(Y - Y2, 2));
        }

        public static System.Windows.Threading.DispatcherOperation RunOnUIThread(Delegate action, params object[] args)
        {
            return GameEngine.RunOnUIThread(action, args);
        }

        public virtual void Enter()
        {
            InGame = true;
            EnterGame?.Invoke();
        }

        public virtual void Left()
        {
            InGame = false;
            LeftGame?.Invoke();
        }
    }
}
