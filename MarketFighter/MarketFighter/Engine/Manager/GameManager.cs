using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

using MarketFighter.Engine.GameObject;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace MarketFighter.Engine.Manager
{
    public class GameManager
    {
        public static readonly double RADIAN = GameEngine.RADIAN;

        private static readonly bool SHOW_FPS = false;

        private Canvas MainCanvas;
        private Animation.AnimationPlayer _AnimationPlayer;
        private ParticleManager _ParticleManager;

        public readonly static int ProcessingRate = GameEngine.ProcessingRate;
        private static int SingleProcessTime = (int)Math.Round(1000.0 / ProcessingRate);
        private int waitForNext;

        private Thread ProcessingThread;
        public bool Running { get; private set; }

        private Action<long> PlayAnimation;

        private static Action<IGameObject, UIElement,
            double, double,
            bool, double,
            bool, double,
            bool, double, double> UpdateDisplayFunc
            = new Action<IGameObject, UIElement,
                double, double,
                bool, double,
                bool, double,
                bool, double, double>(UpdateDisplayDele);

        public static Action<UIElement> RemoveSprite { get; private set; }
        public static Action<IGameObject> AddSprite { get; private set; }

        public HashSet<IGameObject> GameObjects { get; private set; }
        public Dictionary<string, IGameObject> GOIDs { get; private set; }
        private ConcurrentQueue<IGameObject> ObjectsEnqueue;
        private ConcurrentQueue<IGameObject> ObjectsDequeue;

        public GameManager(Canvas canvas, Animation.AnimationPlayer animationPlayer, ParticleManager particleManager)
        {
            MainCanvas = canvas;
            _AnimationPlayer = animationPlayer;
            _ParticleManager = particleManager;

            PlayAnimation = new Action<long>(_AnimationPlayer.NextFrame);

            RemoveSprite = new Action<UIElement>(MainCanvas.Children.Remove);
            AddSprite = new Action<IGameObject>(AddSpriteFunc);

            GameObjects = new HashSet<IGameObject>();
            GOIDs = new Dictionary<string, IGameObject>(20);
            ObjectsEnqueue = new ConcurrentQueue<IGameObject>();
            ObjectsDequeue = new ConcurrentQueue<IGameObject>();
        }

        public void AddGameObject(IGameObject gameObject)
        {
            if (HasSameGOID(gameObject.ID))
                throw new DuplicateObjectIDException("This object ID is already exist.");
                        
            Console.WriteLine("OPEQ: " + gameObject);

            if (!gameObject.Loaded)
            {
                gameObject.OnLoad();
                gameObject.Loaded = true;
            }

            ObjectsEnqueue.Enqueue(gameObject);
        }

        public void RemoveGameObject(IGameObject gameObject)
        {
            gameObject.Dispose();
            ObjectsDequeue.Enqueue(gameObject);
        }

        public bool HasSameGOID(string goid)
        {
            if (goid == null) return false;
            return GOIDs.ContainsKey(goid);
        }

        public IGameObject GetGameObjectByID(string goid)
        {
            GOIDs.TryGetValue(goid, out IGameObject go);
            return go;
        }

        public IEnumerable<IGameObject> GetGameObjectsByName(string name)
        {
            var gos = GameObjects.Where(g => g.Name == name);
            return gos;
        }

        public IEnumerable<IGameObject> GetGameObjectsByNames(params string[] name)
        {
            var gos = GameObjects.Where(g => name.Contains(g.Name));
            return gos;
        }

        public IEnumerable<IGameObject> GetGameObjects()
        {
            return GameObjects;
        }

        public void RegisterAnimation(IGameObject requester, Animation.AnimationProvider provider)
        {
            _AnimationPlayer.AddTask(requester, provider);
        }

        public static DispatcherOperation RunOnUIThread(Delegate action, params object[] args)
        {
            return GameEngine.RunOnUIThread(action, args);
        }

        public void Start()
        {
            ProcessingThread = new Thread(Run);
            Running = true;
            ProcessingThread.Start();
        }

        public void Stop()
        {
            Running = false;
            ProcessingThread.Abort();
        }

        private void Run()
        {
            try
            {
                while (Running)
                {
                    Processing();
                }
            }
            catch (ThreadAbortException) { }
        }

        int frameCount;
        long latestSec;
        private void Processing()
        {
            long now = GameEngine.GetMillisecond();

            if (now - latestSec > 1000)
            {
                if (SHOW_FPS) Console.WriteLine(frameCount);
                latestSec = now;
                frameCount = 0;
            }

            _ParticleManager.Processing();

            _AnimationPlayer.NextFrame(GameEngine.GetMillisecond());

            foreach (IGameObject go in GameObjects)
            {
                if (go.Waiting < 0) continue;
                else if (go.Waiting > 0)
                {
                    if (go.WaitCheckpoint != 0)
                    {
                        go.Waiting -= (int)(now - go.WaitCheckpoint);
                        if (go.Waiting < 0) go.Waiting = 0;
                    }
                    go.WaitCheckpoint = now;
                    continue;
                }

                MoveObject(go);

                if (go is ICatchableGameObject)
                {
                    ICatchableGameObject cago = go as ICatchableGameObject;
                    if (cago.Catched && cago.FollowCatcher)
                    {
                        go.SetPosition(
                            cago.Catcher.X + cago.CatchOffsetX,
                            cago.Catcher.Y + cago.CatchOffsetY);
                    }
                }

                go.Run();

                UpdateDisplay(go);
            }

            CheckObjectQueues();

            int spend = (int)(GameEngine.GetMillisecond() - now);
            waitForNext += SingleProcessTime - spend;
            if (waitForNext > SingleProcessTime * 0.7)
            {
                if (waitForNext > SingleProcessTime)
                {
                    Thread.Sleep(SingleProcessTime);
                    waitForNext -= SingleProcessTime;
                }
                else
                {
                    Thread.Sleep(waitForNext - 1);
                    waitForNext = 0;
                }
            }

            frameCount++;
        }

        private void CheckObjectQueues()
        {
            int times = ObjectsDequeue.Count;
            for(int i = 0; i < times; i++)
            {
                bool result = ObjectsDequeue.TryDequeue(out IGameObject go);

                if (!result) continue;

                if (go.ID != null)
                {
                    GOIDs.Remove(go.ID);
                }
                RunOnUIThread(RemoveSprite, go.GetSprite());
                GameObjects.Remove(go);
                go.Left();
            }

            times = ObjectsEnqueue.Count;
            for (int i = 0; i < times; i++)
            {
                bool result = ObjectsEnqueue.TryDequeue(out IGameObject go);

                if (!result) continue;

                if (go == null) continue;

                if (go.GetSprite() == null)
                {
                    ObjectsEnqueue.Enqueue(go);
                    continue;
                }

                Console.WriteLine("OEQ: " + go);

                if (!go.Initialized)
                {
                    go.OnInitialize();
                    go.Initialized = true;
                }

                GameObjects.Add(go);
                if (go.ID != null)
                {
                    GOIDs.Add(go.ID, go);
                }
                RunOnUIThread(AddSprite, go);
                go.Enter();
            }

        }

        private void MoveObject(IGameObject go)
        {
            var task = go.MoveTask;
            if (task != null)
            {
                if (go is ICatchableGameObject)
                {
                    var icgo = go as ICatchableGameObject;
                    if (icgo.Catched)
                    {
                        if (!task.Force) return;
                        icgo.Catcher.Release();
                    }
                }
                double disX = task.DestX - go.X;
                double disY = task.DestY - go.Y;
                double angle = (Math.Atan(disY / disX));
                if (disX < 0) angle += 180 * GameEngine.RADIAN;

                double speed = task.Speed / GameEngine.ProcessingRate;

                double mx = Math.Cos(angle) * speed;
                double my = Math.Sin(angle) * speed;
                if (disX / mx <= 1) mx = disX;
                if (disY / my <= 1) my = disY;
                go.Move(mx, my);

                if (go.X == task.DestX && go.Y == task.DestY)
                {
                    go.MoveTask = null;
                    task.Callback?.Invoke();                    
                }
                Console.WriteLine("TX: " + task.DestX + ", TY: " + task.DestY + ", GX: " + go.X + ", GY: " + go.Y);
            }
            else if (go.SpeedX != 0 || go.SpeedY != 0)
            {
                go.Move(go.SpeedX / ProcessingRate, go.SpeedY / ProcessingRate);
            }            
        }

        private void AddSpriteFunc(IGameObject gameObject)
        {
            UpdateDisplay(gameObject);
            MainCanvas.Children.Add(gameObject.GetSprite());
        }

        public static void UpdateDisplay(IGameObject gameObject)
        {
            RunOnUIThread(UpdateDisplayFunc,
                    gameObject,
                    gameObject.GetSprite(),
                    gameObject.X - gameObject.CenterX - gameObject.AnimationOffsetX,
                    gameObject.Y - gameObject.CenterY - gameObject.AnimationOffsetY,
                    gameObject.OpacityChanged,
                    gameObject.Opacity,
                    gameObject.RotatableSprite && gameObject.DirectionChanged,
                    gameObject.Direction + gameObject.AnimationRotation,
                    gameObject.ScaleChanged,
                    gameObject.ScaleX,
                    gameObject.ScaleY
                );
        }

        private static void UpdateDisplayDele(IGameObject gameObject, UIElement uiElement,
            double x, double y,
            bool opacityC, double opacity,
            bool rotate, double direction,
            bool scaled, double scaleX, double scaleY)
        {
            Canvas.SetLeft(uiElement, x);
            Canvas.SetTop(uiElement, y);
            if (rotate)
            {
                gameObject.SetActualRotate(direction);
            }
            if (scaled)
            {
                gameObject.SetActualScale(scaleX, scaleY);
            }
            if (opacityC)
            {
                gameObject.SetActualOpacity(opacity);
            }
            if (gameObject.VisibilityChanged)
            {
                gameObject.SetActualVisibility(gameObject.Visibility);
            }
            if (gameObject.EffectChanged)
            {
                gameObject.SetActualEffect(gameObject.Effect);
            }
        }

    }



    [Serializable]
    public class DuplicateObjectIDException : Exception
    {
        public DuplicateObjectIDException() { }
        public DuplicateObjectIDException(string message) : base(message) { }
        public DuplicateObjectIDException(string message, Exception inner) : base(message, inner) { }
        protected DuplicateObjectIDException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
