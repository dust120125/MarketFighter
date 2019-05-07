using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.Manager;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;

namespace MarketFighter.Engine
{
    public class GameEngine
    {        
        public readonly static double RADIAN = Math.PI / 180;
        public readonly static int ProcessingRate = 72;
        public readonly static double G = 980 / ProcessingRate;
        public readonly static double AirResistance = 100 / ProcessingRate;

        private static Action<UIElement, int> SetZIndexAct = new Action<UIElement, int>(Panel.SetZIndex);

        public static Random Randomer = new Random();

        private static GameEngine _Engine;
        private static Canvas _Canvas;
        private static Stopwatch TimeCounter;

        public static double FieldWidth { get; private set; }
        public static double FieldHeight { get; private set; }

        public static GameManager _GameManager { get; private set; }
        public static ParticleManager _ParticleManager { get; private set; }
        public static AnimationPlayer _AnimationPlayer { get; private set; }

        private readonly static Dictionary<string, BitmapImage> LoadedBitmapImages
            = new Dictionary<string, BitmapImage>(10);

        public static GameEngine Create(Canvas canvas)
        {            
            return _Engine = new GameEngine(canvas);
        }

        public static GameEngine GetInstance()
        {
            return _Engine;
        }

        private GameEngine(Canvas canvas)
        {
            TimeCounter = new Stopwatch();
            TimeCounter.Start();

            _Canvas = canvas;
            FieldWidth = canvas.ActualWidth;
            FieldHeight = canvas.ActualHeight;

            _AnimationPlayer = new AnimationPlayer();
            _ParticleManager = new ParticleManager(_Canvas, _AnimationPlayer);
            _GameManager = new GameManager(_Canvas, _AnimationPlayer, _ParticleManager);   
        }

        public static DispatcherOperation RunOnUIThread(Delegate action, params object[] args)
        {
            return _Canvas.Dispatcher.BeginInvoke(action, args);
        }

        public static BitmapImage GetImage(string filePath)
        {
            if (LoadedBitmapImages.ContainsKey(filePath))
            {
                return LoadedBitmapImages[filePath];
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = fs;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public static void SetZIndex(UIElement element, int z)
        {
            RunOnUIThread(SetZIndexAct, element, z);
        }

        public static long GetMillisecond()
        {
            return TimeCounter.ElapsedMilliseconds;
        }

        public void Stop()
        {
            _GameManager.Stop();
        }

    }
}
