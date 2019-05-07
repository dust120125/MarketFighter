using MarketFighter.Engine.GameObject;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarketFighter.Engine.Manager
{
    public class ParticleManager
    {
        public readonly static int OriginSize = 5;

        private Canvas MainCanvas;
        private Animation.AnimationPlayer _AnimationPlayer;

        private Queue<IParticle> ParticlesCreatingLine = new Queue<IParticle>(OriginSize);

        private ConcurrentQueue<ParticleInfo> ImageRequests = new ConcurrentQueue<ParticleInfo>();
        private ConcurrentQueue<ParticleInfo> TextRequests = new ConcurrentQueue<ParticleInfo>();

        private Queue<ImageParticle> ImageParticlesQueue = new Queue<ImageParticle>(OriginSize);
        private Queue<TextParticle> TextParticlesQueue = new Queue<TextParticle>(OriginSize);

        private HashSet<IParticle> Particles = new HashSet<IParticle>();

        private Queue<IParticle> RemoveQueue = new Queue<IParticle>(OriginSize);

        public ParticleManager(Canvas canvas, Animation.AnimationPlayer animationPlayer)
        {
            MainCanvas = canvas;
            _AnimationPlayer = animationPlayer;

            for (int i = 0; i < OriginSize; i++)
            {
                ParticlesCreatingLine.Enqueue(new ImageParticle());
                ParticlesCreatingLine.Enqueue(new TextParticle());
            }
        }

        public void AddRequest(ParticleInfo info)
        {
            switch (info.Type)
            {
                case ParticleType.Image:
                    ImageRequests.Enqueue(info);
                    break;
                case ParticleType.Text:
                    TextRequests.Enqueue(info);
                    break;
            }
        }

        public void RegisterAnimation(IGameObject requester, Animation.AnimationProvider provider)
        {
            _AnimationPlayer.AddTask(requester, provider);
        }

        public void Processing()
        {
            foreach (IParticle p in Particles)
            {
                p.SpeedX += p.AccelerationX;
                p.SpeedY += p.AccelerationY;

                if (p.Gravity) p.SpeedY += GameEngine.G;
                if (p.AirResistance)
                {
                    if (p.SpeedX > 0) p.SpeedX -= GameEngine.AirResistance;
                    else if (p.SpeedX < 0) p.SpeedX += GameEngine.AirResistance;
                }

                if (p.MaxSpeedX != 0 && p.SpeedX > p.MaxSpeedX) p.SpeedX = p.MaxSpeedX;
                else if (p.MinSpeedX != 0 && p.SpeedX < p.MinSpeedX) p.SpeedX = p.MinSpeedX;
                if (p.MaxSpeedY != 0 && p.SpeedY > p.MaxSpeedY) p.SpeedY = p.MaxSpeedY;
                else if (p.MinSpeedY != 0 && p.SpeedY < p.MinSpeedY) p.SpeedX = p.MinSpeedY;

                p.RotateDegree += p.RotateAcceleration;
                if (p.MaxRotateDegree != 0 && p.RotateDegree > p.MaxRotateDegree) p.RotateDegree = p.MaxRotateDegree;
                else if (p.MinRotateDegree != 0 && p.RotateDegree < p.MinRotateDegree) p.RotateDegree = p.MinRotateDegree;
                p.Direction += p.RotateDegree;

                p.Move(p.SpeedX / GameManager.ProcessingRate, p.SpeedY / GameManager.ProcessingRate);

                if (p.GetAge() > p.Life)
                {
                    if (p.WaitAnimationEnd)
                    {
                        if (p.AnimationProvider?.State != Animation.AnimationProvider.PlayState.Playing)
                        {
                            Die(p);
                        }
                    }
                    else
                    {
                        Die(p);
                    }
                }

                UpdateDisplay(p);
            }

            CheckRemoveQueue();
            CheckCreatingLine();
            DealRequests();
        }

        private void AddParticle(ParticleInfo info, IParticle particle)
        {
            particle.SetPosition(info.X, info.Y);
            particle.SpeedX = info.SpeedX;
            particle.SpeedY = info.SpeedY;
            particle.Direction = info.Direction;
            particle.Opacity = info.Opacity;

            particle.Life = info.Life;
            particle.MaxSpeedX = info.MaxSpeedX;
            particle.MaxSpeedY = info.MaxSpeedY;
            particle.MaxRotateDegree = info.MaxRotateDegree;
            particle.RotateAcceleration = info.RotateAcceleration;
            particle.MinRotateDegree = info.MinRotateDegree;
            particle.MinSpeedY = info.MinSpeedY;
            particle.MinSpeedX = info.MinSpeedX;
            particle.RotateDegree = info.RotateDegree;
            particle.AccelerationX = info.AccelerationX;
            particle.AccelerationY = info.AccelerationY;
            particle.Gravity = info.Gravity;
            particle.AirResistance = info.AirResistance;
            particle.AnimationProvider = info.AnimationProvider;
            particle.WaitAnimationEnd = info.WaitAnimationEnd;

            particle.Birth = GameEngine.GetMillisecond();
            particle.Visibility = System.Windows.Visibility.Visible;

            if (particle.AnimationProvider != null)
            {
                particle.AnimationProvider.Reset(false);
                RegisterAnimation(particle, particle.AnimationProvider);
                particle.AnimationProvider.Play();
            }

            if (info.ZIndex != 0)
            {
                GameEngine.SetZIndex(particle.GetSprite(), info.ZIndex);
            }

            switch (particle.Type)
            {
                case ParticleType.Image:
                    ImageParticle ip = particle as ImageParticle;
                    ip.SetImage(info.Visual as ImageSource);
                    Particles.Add(ip);
                    break;
                case ParticleType.Text:
                    TextParticle tp = particle as TextParticle;
                    tp.Text = info.Visual as string;
                    if (info.FontColor != null)
                        tp.FontColor = info.FontColor;
                    if (info.FontSize != 0)
                        tp.FontSize = info.FontSize;
                    if (info.FontStyle != null)
                        tp.FontStyle = info.FontStyle;
                    if (info.FontWeight != null)
                        tp.FontWeight = info.FontWeight;
                    Particles.Add(tp);
                    break;
            }
        }

        private void DealRequests()
        {
            int times = ImageRequests.Count;
            for (int i = 0; i < times; i++)
            {
                bool res = ImageRequests.TryDequeue(out ParticleInfo info);
                if (!res) continue;
                if (!ImageParticlesQueue.Any())
                {
                    ImageRequests.Enqueue(info);
                    CreateNewInstance(ParticleType.Image);
                    continue;
                }
                else
                {
                    AddParticle(info, ImageParticlesQueue.Dequeue());
                }
            }

            times = TextRequests.Count;
            for (int i = 0; i < times; i++)
            {
                bool res = TextRequests.TryDequeue(out ParticleInfo info);
                if (!res) continue;
                if (!TextParticlesQueue.Any())
                {
                    TextRequests.Enqueue(info);
                    CreateNewInstance(ParticleType.Text);
                    continue;
                }
                else
                {
                    AddParticle(info, TextParticlesQueue.Dequeue());
                }
            }
        }

        private void CreateNewInstance(ParticleType type)
        {
            switch (type)
            {
                case ParticleType.Image:
                    ParticlesCreatingLine.Enqueue(new ImageParticle());
                    break;
                case ParticleType.Text:
                    ParticlesCreatingLine.Enqueue(new TextParticle());
                    break;
            }
        }

        private void CheckCreatingLine()
        {
            int times = ParticlesCreatingLine.Count;
            for (int i = 0; i < times; i++)
            {
                IParticle particle = ParticlesCreatingLine.Dequeue();
                if (particle.GetSprite() == null)
                {
                    ParticlesCreatingLine.Enqueue(particle);
                    continue;
                }
                particle.OnInitialize();
                particle.Initialized = true;
                GameEngine.RunOnUIThread(GameManager.AddSprite, particle);

                switch (particle.Type)
                {
                    case ParticleType.Image:
                        ImageParticlesQueue.Enqueue(particle as ImageParticle);
                        break;
                    case ParticleType.Text:
                        TextParticlesQueue.Enqueue(particle as TextParticle);
                        break;
                }
            }
        }

        private void Die(IParticle particle)
        {
            particle.AnimationProvider?.Suspend();
            particle.Visibility = System.Windows.Visibility.Hidden;
            CleanParticle(particle);
            RemoveQueue.Enqueue(particle);

        }

        private void CleanParticle(IParticle particle)
        {
            particle.Life = -1;
            particle.MaxRotateDegree = 0;
            particle.MaxSpeedX = 0;
            particle.MaxSpeedY = 0;
            particle.MaxRotateDegree = 0;
            particle.RotateAcceleration = 0;
            particle.MinRotateDegree = 0;
            particle.MinSpeedY = 0;
            particle.MinSpeedX = 0;
            particle.RotateDegree = 0;
            particle.AccelerationX = 0;
            particle.AccelerationY = 0;
            particle.Gravity = false;
            particle.AirResistance = false;
            particle.AnimationProvider = null;
            particle.WaitAnimationEnd = false;
        }

        private void CheckRemoveQueue()
        {
            while (RemoveQueue.Any())
            {
                IParticle particle = RemoveQueue.Dequeue();
                switch (particle.Type)
                {
                    case ParticleType.Image:
                        ImageParticle ip = particle as ImageParticle;
                        Particles.Remove(ip);
                        ImageParticlesQueue.Enqueue(ip);
                        break;
                    case ParticleType.Text:
                        TextParticle tp = particle as TextParticle;
                        Particles.Remove(tp);
                        TextParticlesQueue.Enqueue(tp);
                        break;
                }
            }
        }

        private void UpdateDisplay(IGameObject gameObject)
        {
            GameManager.UpdateDisplay(gameObject);
        }
    }
}
