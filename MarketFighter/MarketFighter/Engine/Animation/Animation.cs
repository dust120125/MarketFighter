using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarketFighter.Engine.Animation
{

    public class ImageAnimationFrame : AnimationFrame
    {
        public ImageSource Image { get; private set; }
        public override object Visual { get { return Image; } }

        public ImageAnimationFrame(ImageSource image, double animationOffsetX, double animationOffsetY,
            double animationRotation, double? scaleX, double? scaleY, double? opacity)
            : base(animationOffsetX, animationOffsetY, animationRotation, scaleX, scaleY, opacity)
        {
            Image = image;
        }
    }

    public class TextAnimationFrame : AnimationFrame
    {
        public string Text { get; private set; }
        public override object Visual { get { return Text; } }

        public TextAnimationFrame(string text, double animationOffsetX, double animationOffsetY,
            double animationRotation, double? scaleX, double? scaleY, double? opacity)
            : base(animationOffsetX, animationOffsetY, animationRotation, scaleX, scaleY, opacity)
        {
            Text = text;
        }
    }

    public class AnimationFrameInfo
    {
        public AnimationFrame Frame;
        public int interval;
        public int next;

        public AnimationFrameInfo(AnimationFrame frame, int interval, int next)
        {
            Frame = frame;
            this.interval = interval;
            this.next = next;
        }
    }

    public class Animation
    {

        private List<AnimationFrameInfo> FrameInfos;

        private AnimationFrame _StopFrame;
        public AnimationFrame StopFrame
        {
            get
            {
                if (_StopFrame == null) return FrameInfos.Last().Frame;
                else return _StopFrame;
            }

            private set
            {
                _StopFrame = value;
            }
        }

        public Animation(AnimationFrame stopFrame, params object[] frames)
        {
            if (frames.Length % 9 != 0) throw new AnimationInitializeException(
                "There is something error in frame nodes parameters.");

            StopFrame = stopFrame;
            FrameInfos = new List<AnimationFrameInfo>();

            try
            {
                AnimationFrame frame;
                for (int i = 0; i < frames.Length;)
                {
                    object visual = frames[i++]; //could be null (same image with previous frame)
                    double offsetX = (double)frames[i++];
                    double offsetY = (double)frames[i++];
                    double rotation = (double)frames[i++];
                    double? scaleX = frames[i++] as double?;
                    double? scaleY = frames[i++] as double?;
                    double? opactiy = frames[i++] as double?;

                    int interval = (int)frames[i++];
                    int next = (int)frames[i++];

                    if (visual == null)
                    {
                        frame = new TextAnimationFrame(null, offsetX, offsetY, rotation, scaleX, scaleY, opactiy);
                    }
                    else if (visual is string)
                    {
                        frame = new TextAnimationFrame(visual as string, offsetX, offsetY, rotation, scaleX, scaleY, opactiy);
                    }
                    else if (visual is ImageSource)
                    {
                        frame = new ImageAnimationFrame(visual as ImageSource, offsetX, offsetY, rotation, scaleX, scaleY, opactiy);
                    }
                    else
                    {
                        throw new AnimationInitializeException("'Visual' must be a 'string' or 'ImageSource' !");
                    }

                    FrameInfos.Add(new AnimationFrameInfo(frame, interval, next));
                }
            }
            catch (Exception e)
            {
                throw new AnimationInitializeException(
                    "There is something error in frame nodes parameters.", e);
            }
        }

        public Animation(AnimationFrame stopFrame, List<AnimationFrameInfo> frameInfos)
        {
            FrameInfos = frameInfos;
            StopFrame = stopFrame;
        }

        public AnimationFrameInfo GetFrame(int index)
        {
            return FrameInfos[index];
        }

    }


    [Serializable]
    public class AnimationInitializeException : Exception
    {
        public AnimationInitializeException() { }
        public AnimationInitializeException(string message) : base(message) { }
        public AnimationInitializeException(string message, Exception inner) : base(message, inner) { }
        protected AnimationInitializeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
