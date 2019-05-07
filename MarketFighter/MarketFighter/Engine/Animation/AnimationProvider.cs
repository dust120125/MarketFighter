using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using static MarketFighter.Engine.Animation.Animation;

namespace MarketFighter.Engine.Animation
{
    public class AnimationProvider
    {

        public enum PlayState { Initialized, Ready, Playing, Paused, Stopped, Ended, Dead }

        public PlayState State;

        private Animation _Animation;

        public delegate void AnimationPlayEventHandler();
        public event AnimationPlayEventHandler PlayStart;
        public event AnimationPlayEventHandler PlayFinished;
        public event AnimationPlayEventHandler PlaySuspend;

        private AnimationFrameInfo CurrentFrameInfo;
        private int CurrentFrameIndex;
        private long latestTime;
        public bool FrameChanged { get; private set; }
        private bool FirstFrame;

        private bool _PlayedFirstFrame;
        public bool PlayedFirstFrame
        {
            get { return _PlayedFirstFrame; }
            set
            {
                _PlayedFirstFrame = value;
                if (value) PlayStart?.Invoke();
            }
        }

        public AnimationProvider(Animation animation, bool autoPlay)
        {
            _Animation = animation;
            Reset(autoPlay);
        }

        public void Reset(bool autoPlay)
        {
            if (State == PlayState.Dead || State == PlayState.Initialized)
                State = PlayState.Initialized;
            else
                State = PlayState.Ready;

            CurrentFrameIndex = -1;
            latestTime = -1;
            if (autoPlay) Play();
        }

        public AnimationFrame Play()
        {
            if (State != PlayState.Initialized && State != PlayState.Ready) return null;
            FirstFrame = true;
            CurrentFrameIndex = 0;
            CurrentFrameInfo = _Animation.GetFrame(CurrentFrameIndex);
            latestTime = GameEngine.GetMillisecond();
            State = PlayState.Playing;
            return CurrentFrameInfo.Frame;
        }

        public void Pause()
        {
            if (State == PlayState.Playing)
                State = PlayState.Paused;
        }

        public void Resume()
        {
            if (State == PlayState.Paused)
            {
                latestTime = GameEngine.GetMillisecond();
                State = PlayState.Playing;
            }
        }

        /// <summary>
        /// Play Stop-Frame, then stop animation
        /// </summary>
        public void Stop()
        {
            if (State == PlayState.Ended || State == PlayState.Dead) return;
            State = PlayState.Stopped;
        }

        /// <summary>
        /// Stop animation without play Stop-Frame
        /// </summary>
        public void Suspend()
        {
            State = PlayState.Ended;
            PlaySuspend?.Invoke();
            PlayFinished?.Invoke();
        }

        public AnimationFrame GetStopFrame()
        {
            return _Animation.StopFrame;
        }

        public AnimationFrame GetFrame(long now)
        {
            if (State != PlayState.Playing) throw new AnimationNotPlayingException(
                "Animation isn't playing, maybe call 'Play()' first?");

            if (State == PlayState.Stopped)
            {
                State = PlayState.Ended;
                PlayFinished?.Invoke();
                return _Animation.StopFrame;
            }

            if (State == PlayState.Ended) throw new AnimationNotPlayingException(
               "This animation was ended, it should be removed by AnimationPlayer!");

            long pass = now - latestTime;

            if (FirstFrame)
            {
                FirstFrame = false;
                FrameChanged = true;
            }
            else FrameChanged = false;

            while (pass > CurrentFrameInfo.interval)
            {
                FrameChanged = true;
                latestTime = now;
                pass -= CurrentFrameInfo.interval;

                if (CurrentFrameInfo.next == -1)
                {
                    State = PlayState.Ended;
                    PlayFinished?.Invoke();
                    return _Animation.StopFrame;
                }

                CurrentFrameIndex = CurrentFrameInfo.next;
                CurrentFrameInfo = _Animation.GetFrame(CurrentFrameIndex);
            }

            return CurrentFrameInfo.Frame;
        }

    }


    [Serializable]
    public class AnimationNotPlayingException : Exception
    {
        public AnimationNotPlayingException() { }
        public AnimationNotPlayingException(string message) : base(message) { }
        public AnimationNotPlayingException(string message, Exception inner) : base(message, inner) { }
        protected AnimationNotPlayingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
