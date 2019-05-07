using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MarketFighter.Engine.GameObject;
using static MarketFighter.Engine.Animation.AnimationProvider;

namespace MarketFighter.Engine.Animation
{
    public class AnimationPlayer
    {

        private struct AnimationTask
        {
            public IGameObject Requester;
            public AnimationProvider Provider;
        }

        private const int CleanInterval = 2000;

        private Dictionary<IGameObject, AnimationTask> Tasks;

        private long latestCleanTime;

        public AnimationPlayer()
        {
            Tasks = new Dictionary<IGameObject, AnimationTask>(25);
        }

        public bool AddTask(IGameObject gameObject, AnimationProvider provider)
        {
            if (gameObject == null || provider == null)
                return false;
            if (provider.State == PlayState.Ended || provider.State == PlayState.Dead)
                return false;

            lock (Tasks)
            {
                if (Tasks.ContainsKey(gameObject)) StopTask(gameObject);

                AnimationTask t = new AnimationTask()
                { Requester = gameObject, Provider = provider };

                Tasks[gameObject] = t;
                if (provider.State != PlayState.Playing)
                    provider.State = PlayState.Ready;
            }
            return true;
        }

        public void NextFrame(long now)
        {
            lock (Tasks)
            {
                foreach (AnimationTask task in Tasks.Values)
                {
                    if (task.Provider.State != PlayState.Playing ||
                        task.Provider.State == PlayState.Ended) continue;

                    AnimationFrame frame = task.Provider.GetFrame(now);
                    if (task.Provider.FrameChanged)
                    {
                        task.Requester.AnimationOffsetX = frame.AnimationOffsetX;
                        task.Requester.AnimationOffsetY = frame.AnimationOffsetY;
                        task.Requester.AnimationRotation = frame.AnimationRotation;
                        if (frame.ScaleX != null)
                            task.Requester.ScaleX = frame.ScaleX.Value;
                        if (frame.ScaleY != null)
                            task.Requester.ScaleY = frame.ScaleY.Value;
                        if (frame.Opacity != null)
                            task.Requester.Opacity = frame.Opacity.Value;
                        if (frame.Visual != null)
                        {
                            if (frame.Visual is string && task.Requester is TextGameObject)
                                (task.Requester as TextGameObject).Text = frame.Visual as string;
                            else if (frame.Visual is ImageSource && task.Requester is GameObject.GameObject)
                                (task.Requester as GameObject.GameObject).SetImage(frame.Visual as ImageSource);
                        }
                    }

                    if (!task.Provider.PlayedFirstFrame)
                    {
                        task.Provider.PlayedFirstFrame = true;
                    }
                }
            }

            if (now - latestCleanTime > CleanInterval)
            {
                CleanEndedTask();
                latestCleanTime = now;
            }
        }

        private void CleanEndedTask()
        {
            lock (Tasks)
            {
                var tasks = Tasks.Values;
                for (int i = 0; i < tasks.Count;)
                {
                    AnimationTask task = tasks.ElementAt(i);
                    if (task.Provider.State == PlayState.Ended)
                    {
                        RemoveTask(task);
                    }
                    else i++;
                }
            }
        }

        public void RemoveTask(IGameObject gameObject)
        {
            lock (Tasks)
            {
                Tasks[gameObject].Provider.State = PlayState.Dead;
                Tasks.Remove(gameObject);
            }
        }

        private void RemoveTask(AnimationTask task)
        {
            RemoveTask(task.Requester);
        }

        public void StopTask(IGameObject gameObject)
        {
            Tasks.TryGetValue(gameObject, out AnimationTask task);
            StopTask(task);
        }

        private void StopTask(AnimationTask task)
        {
            AnimationFrame frame = task.Provider.GetStopFrame();

            task.Requester.AnimationOffsetX = frame.AnimationOffsetX;
            task.Requester.AnimationOffsetY = frame.AnimationOffsetY;
            task.Requester.AnimationRotation = frame.AnimationRotation;
            if (frame.ScaleX != null)
                task.Requester.ScaleX = frame.ScaleX.Value;
            if (frame.ScaleY != null)
                task.Requester.ScaleY = frame.ScaleY.Value;
            if (frame.Opacity != null)
                task.Requester.Opacity = frame.Opacity.Value;
            if (frame.Visual != null)
            {
                if (frame.Visual is string && task.Requester is TextGameObject)
                    (task.Requester as TextGameObject).Text = frame.Visual as string;
                else if (frame.Visual is ImageSource && task.Requester is GameObject.GameObject)
                    (task.Requester as GameObject.GameObject).SetImage(frame.Visual as ImageSource);
            }

            task.Provider.Stop();
        }

    }
}
