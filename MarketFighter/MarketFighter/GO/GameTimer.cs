using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;

namespace MarketFighter.GO
{
    public class GameTimer : TextGameObject
    {
        public event Action CountdownEnds;

        public long MaxTime { get; set; }

        public readonly bool Countdown;

        private bool CountdownEnded;

        public bool IsRunning { get { return Watch.IsRunning; } }

        private Stopwatch Watch = new Stopwatch();
        private long latestCheck;
        private bool fstNotify, secNotify;

        private Animation NotifyAnime = new Animation(null,
            null, 0d, 0d, 0d, 2, 2, null, 30, 1,
            null, 0d, 0d, 0d, 1.85, 1.85, null, 30, 2,
            null, 0d, 0d, 0d, 1.7, 1.7, null, 30, 3,
            null, 0d, 0d, 0d, 1.45, 1.45, null, 30, 4,
            null, 0d, 0d, 0d, 1.3, 1.3, null, 30, 5,
            null, 0d, 0d, 0d, 1.15, 1.15, null, 30, 6,
            null, 0d, 0d, 0d, 0.95, 0.95, null, 30, 7,
            null, 0d, 0d, 0d, 1, 1, null, 30, -1);

        public GameTimer() : this(false)
        {

        }

        public GameTimer(bool countdown) : this(countdown, null)
        {

        }

        public GameTimer(bool countdown, string id) : base(id)
        {
            Countdown = countdown;
        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            long textTime = Countdown ? MaxTime : 0;
            string text = GetTimeText(textTime);

            RunOnUIThread(new Action(() =>
            {
                Panel.SetZIndex(_SpriteView, 999999999);
                _SpriteView.Effect = new DropShadowEffect();
                _SpriteView.Foreground = Brushes.Red;
                _SpriteView.FontSize = 70;
                _SpriteView.FontWeight = FontWeights.ExtraBlack;
                _SpriteView.Text = text;
            }));
        }

        public override void OnLoad()
        {

        }

        public override void Run()
        {
            if (Watch.IsRunning)
            {
                long pass = Watch.ElapsedMilliseconds;
                long interval = pass - latestCheck;
                if (interval < 1000) return;
                latestCheck = pass;
                long textTime = Countdown ? MaxTime - pass : pass;
                if (textTime < 0) textTime = 0;
                string text = GetTimeText(textTime);
                Text = text;

                if (Countdown)
                {
                    if (!fstNotify && textTime < 21000)
                    {
                        fstNotify = true;
                        RegisterAnimation(new AnimationProvider(NotifyAnime, true));
                    }
                    if (!secNotify && textTime < 11000)
                    {
                        secNotify = true;
                        RegisterAnimation(new AnimationProvider(NotifyAnime, true));
                    }
                    else if (textTime == 0)
                    {
                        CountdownEnded = true;
                        CountdownEnds?.Invoke();
                        Watch.Stop();
                    }
                }
            }
        }

        public void Restart()
        {
            Stop();
            Reset();
            Start();
        }

        public void Reset()
        {
            Watch.Reset();
            CountdownEnded = false;
        }

        public void Start()
        {
            if (!CountdownEnded) Watch.Start();
        }

        public void Stop()
        {
            Watch.Stop();
        }

        private string GetTimeText(long milisecond)
        {
            long sec = milisecond / 1000;
            long min = sec / 60;
            sec %= 60;

            return string.Format("{0:00}:{1:00}", min, sec);
        }
    }
}
