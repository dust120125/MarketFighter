using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Manager;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Effects;
using System.Threading.Tasks;

namespace MarketFighter.GO
{
    public class Clothes : CatchableGameObject, ICommodity
    {

        private const bool DebugMode = false;


        public enum ClothesType
        {
            Blouse, Cardigan, Coat, Gloves,
            Jacket, Jeans, Scarf, Shirt,
            Shoes, Shorts, Skirt, Socks,
            Trousers, Vest
        }

        public readonly static int Types = 14;
        private static Random Randomer = new Random();

        private static Typeface DefaultTypeface = new Typeface(
            new FontFamily("微軟正黑體"),
            FontStyles.Normal,
            FontWeights.Bold,
            FontStretches.Normal
            );

        public static void PreLoadImages()
        {
            foreach (ClothesType c in Enum.GetValues(typeof(ClothesType)))
            {

            }
        }

        public static BitmapImage GetClothesImage(ClothesType type)
        {
            return GameEngine.GetImage("./sprite/" + type.ToString() + ".png");
        }

        private static ImageSource GetImageSource(BitmapImage bitmapImage, int price, int discount, bool off)
        {
            BitmapSource bitmapSource = bitmapImage;
            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, bitmapImage.Width, bitmapImage.Height));
                drawingContext.DrawText(new FormattedText(
                    price.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    DefaultTypeface,
                    32,
                    Brushes.Green), new Point(bitmapImage.Width / 2 - 38, bitmapImage.Height - 70));

                if (off)
                {
                    drawingContext.DrawText(new FormattedText(
                    discount + "% off",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    DefaultTypeface,
                    32,
                    Brushes.Red), new Point(bitmapImage.Width / 2 - 56, bitmapImage.Height - 40));
                }
                else
                {
                    drawingContext.DrawText(new FormattedText(
                    (100 - discount) + "%",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    DefaultTypeface,
                    32,
                    Brushes.Red), new Point(bitmapImage.Width / 2 - 36, bitmapImage.Height - 40));
                }
            }
            var image = new DrawingImage(visual.Drawing);
            image.Freeze();
            return image;
        }

        private static ImageSource[] GetSplitClothesImages(ClothesType type)
        {
            BitmapImage bmp = GetClothesImage(type);
            ImageSource[] images = new ImageSource[2];

            Int32Rect[] half = new Int32Rect[2];
            if (Randomer.Next(0, 2) > 0)
            {
                half[0] = new Int32Rect(0, 0, bmp.PixelWidth / 2, bmp.PixelHeight);
                half[1] = new Int32Rect(bmp.PixelWidth / 2, 0, bmp.PixelWidth / 2 - 1, bmp.PixelHeight);
            }
            else
            {
                half[0] = new Int32Rect(0, 0, bmp.PixelWidth, (bmp.PixelHeight - 55) / 2);
                half[1] = new Int32Rect(0, (bmp.PixelHeight - 55) / 2, bmp.PixelWidth, (bmp.PixelHeight - 55) / 2 - 1);
            }

            CroppedBitmap cbmp = new CroppedBitmap(bmp, half[0]);
            CroppedBitmap cbmp2 = new CroppedBitmap(bmp, half[1]);
            cbmp.Freeze();
            cbmp2.Freeze();

            images[0] = cbmp;
            images[1] = cbmp2;

            return images;
        }

        private static ImageSource GetStainedImage(ClothesType type)
        {
            if (!StainedClothes.ContainsKey(type))
            {
                BitmapSource tmp = ImageFactory.GetColorFilteredBitmap(
                    GetClothesImage(type).Clone(), Colors.SaddleBrown, 0.5);

                tmp.Freeze();
                StainedClothes[type] = tmp;
            }
            return StainedClothes[type];
        }

        //##################################################################

        public static readonly Dictionary<ClothesType, ImageSource> StainedClothes
            = new Dictionary<ClothesType, ImageSource>();

        public event Action<int> OnPriceCheckout;

        private ClothesType myType;
        private ImageSource Image;
        public int Price { get; private set; }
        public int Discount { get; private set; }

        private ICatcherGameObject Catcher2;
        private double CatchOffsetX2, CatchOffsetY2;

        private double originalX, originalY;
        private double originalScaleX, originalScaleY;

        private bool reversalScaleX, reversalScaleY;
        private double COW, COH;

        public bool NeverCatched { get; private set; }
        public bool InCart { get; private set; }
        public bool Stained { get; private set; }
        public bool Checkout { get; set; }
        private bool CheckoutTextShowed;

        private bool Splitted;
        private bool SplitAnimationPlaying;

        private static ParticleInfo CheckoutText = new ParticleInfo()
        {
            Type = ParticleType.Text,
            Gravity = false,
            AirResistance = false,
            SpeedY = -350,
            AccelerationY = 8,
            MaxSpeedY = -1,
            ZIndex = 9999,
            Life = 1400,
            Opacity = 1,
            FontColor = Brushes.Green,
            FontSize = 36,
            FontWeight = FontWeights.UltraBold
        };

        private static int CheckoutTextAnimeInterval = 80;
        private static Animation CheckoutTextAnime = new Animation(null,
            null, 0d, 0d, 0d, 1, 1, 1, CheckoutTextAnimeInterval, 1,
            null, 0d, 0d, 0d, 1, 1, 0.95, CheckoutTextAnimeInterval, 2,
            null, 0d, 0d, 0d, 1, 1, 0.9, CheckoutTextAnimeInterval, 3,
            null, 0d, 0d, 0d, 1, 1, 0.85, CheckoutTextAnimeInterval, 4,
            null, 0d, 0d, 0d, 1, 1, 0.8, CheckoutTextAnimeInterval, 5,
            null, 0d, 0d, 0d, 1, 1, 0.75, CheckoutTextAnimeInterval, 6,
            null, 0d, 0d, 0d, 1, 1, 0.7, CheckoutTextAnimeInterval, 7,
            null, 0d, 0d, 0d, 1, 1, 0.65, CheckoutTextAnimeInterval, 8,
            null, 0d, 0d, 0d, 1, 1, 0.6, CheckoutTextAnimeInterval, 9,
            null, 0d, 0d, 0d, 1, 1, 0.5, CheckoutTextAnimeInterval, 10,
            null, 0d, 0d, 0d, 1, 1, 0.4, CheckoutTextAnimeInterval, 11,
            null, 0d, 0d, 0d, 1, 1, 0.3, CheckoutTextAnimeInterval, 12,
            null, 0d, 0d, 0d, 1, 1, 0.2, CheckoutTextAnimeInterval, 13,
            null, 0d, 0d, 0d, 1, 1, 0.1, CheckoutTextAnimeInterval, -1);

        private AnimationProvider animationProvider;


        private Clothes(ClothesType type) : this()
        {
            myType = type;
        }

        public Clothes() : this(null)
        {

        }

        public Clothes(string id) : base(id)
        {
            DuplicateCatch = true;
            NeverCatched = true;

            int index = Randomer.Next(Types);
            myType = (ClothesType)Enum.GetValues(typeof(ClothesType)).GetValue(index);
            int priceRnd = Randomer.Next(10);

            if (Price <= 5)
            {
                Price = Randomer.Next(50, 2000);
            }
            else if (priceRnd <= 7)
            {
                Price = Randomer.Next(1000, 5000);
            }
            else
            {
                Price = Randomer.Next(5000, 50000);
            }


            Discount = Randomer.Next(91);
            if (Discount % 5.0 != 0) Discount = (int)(5 * Math.Round(Discount / 5.0));
        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            ScaleX = 0.7;
            ScaleY = 0.7;
            if (Splitted)
            {
                Price = 0;
                return;
            }
            Image = GetImageSource(
                GetClothesImage(myType),
                Price,
                Discount,
                Randomer.Next(2) == 0);

            SetImage(Image);
            CenterToSprite();
        }

        public override void OnLoad()
        {

        }

        public override bool InRange(IGameObject gameObject, double range)
        {
            return Distance(gameObject) < 55 + range;
        }

        public override ICatchableGameObject Catch(ICatcherGameObject catcher)
        {
            if (!Catchable) return null;

            if (Catched && !DuplicateCatch) return null;

            if (Catcher != null)
            {
                if (Catcher is EnemyHand)
                {
                    Catcher.Release();
                    Catcher = catcher;
                    CatchOffsetX = X - catcher.X;
                    CatchOffsetY = Y - catcher.Y;

                    SetSpeed(0, 0);
                    Catched = true;
                    return this;
                }

                if (Catcher2 != null)
                {
                    return null;
                }

                FollowCatcher = false;

                originalX = X;
                originalY = Y;
                originalScaleX = ScaleX;
                originalScaleY = ScaleY;

                Catcher2 = catcher;
                COW = ImageWidth * ScaleX;
                COH = ImageHeight * ScaleY;
                CatchOffsetX2 = X - catcher.X;
                CatchOffsetY2 = Y - catcher.Y;

                double xx = Catcher.X - Catcher2.X;
                double yy = Catcher.Y - Catcher2.Y;
                double slope = yy / xx;

                double angle = (Math.Atan(slope) / RADIAN);
                if (xx < 0) angle += 180;
                angle -= ActualDirection;

                if (angle < 0) angle += 360;
                else if (angle > 360) angle %= 360;

                reversalScaleX = (angle > 90 && angle < 180) || angle > 270;
                reversalScaleY = angle < 180;
            }
            else
            {
                if (Catcher2 != null) Catcher2.Release();
                Catcher = catcher;
                CatchOffsetX = X - catcher.X;
                CatchOffsetY = Y - catcher.Y;
            }

            SetSpeed(0, 0);
            Catched = true;
            return this;
        }

        public override void Release(ICatcherGameObject catcher)
        {
            if (Catcher2 != null)
            {
                if (!catcher.Equals(Catcher2))
                {
                    Catcher = Catcher2;
                    CatchOffsetX = CatchOffsetX2;
                    CatchOffsetY = CatchOffsetY2;
                }

                Catcher2 = null;

                int frames = (int)(Math.Max(ScaleX, ScaleY) / Math.Min(originalScaleX, originalScaleY) * 10);
                if (frames < 1) frames = 1;
                int interval = 10;
                double intervalX = (X - (Catcher.X + CatchOffsetX)) / frames;
                double intervalY = (Y - (Catcher.Y + CatchOffsetY)) / frames;
                double scaleIntervalX = (ScaleX - originalScaleX) / frames;
                double scaleIntervalY = (ScaleY - originalScaleY) / frames;
                Debug.Print("XIn: {0}, YIn:{1}", X - Catcher.X, Y - Catcher.Y);

                List<AnimationFrameInfo> infos = new List<AnimationFrameInfo>(frames);
                for (int i = 1; i <= frames; i++)
                {
                    int next = i == frames ? -1 : i;
                    infos.Add(new AnimationFrameInfo(new ImageAnimationFrame(
                        i == 0 ? Image : null,
                        Catcher.X + CatchOffsetX - (X - intervalX * i),
                        Catcher.Y + CatchOffsetY - (Y - intervalY * i),
                        0,
                        ScaleX - scaleIntervalX * i,
                        ScaleY - scaleIntervalY * i,
                        null),
                        interval, next));
                    Debug.Print("X: {0}, Y:{1}", intervalX * i, intervalY * i);
                }

                Animation anim = new Animation(
                    new ImageAnimationFrame(null, 0, 0, 0, originalScaleX, originalScaleY, null), infos);

                animationProvider = new AnimationProvider(anim, true);
                animationProvider.PlayStart += delegate ()
                {
                    AnimationOffsetX = Catcher.X + CatchOffsetX - (X - intervalX);
                    AnimationOffsetY = Catcher.Y + CatchOffsetY - (Y - intervalY);
                    FollowCatcher = true;
                };
                animationProvider.PlayFinished += delegate () { SplitAnimationPlaying = false; };

                SplitAnimationPlaying = true;
                GameEngine._AnimationPlayer.AddTask(this, animationProvider);

                return;
            }

            Catched = false;
            if (Inertia)
            {
                double angle = (Math.Atan(ActualSpeedY / ActualSpeedX) / RADIAN);
                if (ActualSpeedX < 0) angle += 180;
                if (Double.IsNaN(angle)) angle = 0;
                Direction = angle;
                ReleaseSpeed = Math.Sqrt(Math.Pow(ActualSpeedX, 2) + Math.Pow(ActualSpeedY, 2));
                Debug.Print(angle + ", RS: " + ReleaseSpeed);
            }

            if (!SplitAnimationPlaying)
            {
                if (animationProvider != null &&
                    animationProvider.State == AnimationProvider.PlayState.Playing)
                    animationProvider.Stop();
            }
            else
            {
                SetPosition((X + Catcher.X + CatchOffsetX) / 2, (Y + Catcher.Y + CatchOffsetY) / 2);
                SetPosition((X + Catcher.X + CatchOffsetX) / 2, (Y + Catcher.Y + CatchOffsetY) / 2);
            }

            NeverCatched = false;
            SetSpeed(ReleaseSpeed, Direction);
            Catcher = null;
        }

        private void Split()
        {
            if (Catcher == null || Catcher2 == null) return;

            Splitted = true;
            SplitParticle();

            var images = GetSplitClothesImages(myType);
            Clothes parts = new Clothes(myType)
            {
                Name = "commodity",
                Splitted = true,
                DuplicateCatch = false
            };
            parts.SetPosition(Catcher2.X, Catcher2.Y);
            parts.SpriteInitialized += () => parts.SetImage(images[1]);

            SetImage(images[0]);
            ScaleX = originalScaleX;
            ScaleY = originalScaleY;
            SetPosition(originalX, originalY);
            DuplicateCatch = false;
            FollowCatcher = true;

            GameEngine._GameManager.AddGameObject(parts);

            if (!(Catcher2 is EnemyHand)) { Catcher2.Catch(parts); }
            parts.NeverCatched = false;
            Catcher2 = null;
        }

        private void SplitParticle()
        {
            ParticleInfo info = new ParticleInfo()
            {
                Type = ParticleType.Text,
                X = X,
                Y = Y,
                Visual = "▩",
                Gravity = true,
                AirResistance = true,
                Opacity = 1,
                FontColor = Brushes.Gray,
                FontSize = 30,
                FontStyle = FontStyles.Oblique,
                WaitAnimationEnd = false
            };

            for (int i = 0; i < 15; i++)
            {
                info.RotateDegree = GameEngine.Randomer.Next(-5, 5);
                info.SpeedX = GameEngine.Randomer.Next(-250, 250);
                info.SpeedY = GameEngine.Randomer.Next(-250, 0);
                info.Life = GameEngine.Randomer.Next(1500, 3000);
                GameEngine._ParticleManager.AddRequest(info);
            }
        }

        public override void Run()
        {
            if (Checkout)
            {
                if (!CheckoutTextShowed && X < GameEngine.FieldWidth / 2)
                {
                    ParticleInfo info = CheckoutText;
                    info.X = X;
                    info.Y = Y + 250;
                    info.AnimationProvider = new AnimationProvider(CheckoutTextAnime, false);

                    int value;
                    if (Stained || Splitted)
                    {
                        value = 0;
                        info.FontColor = Brushes.Red;
                    }
                    else
                    {
                        value = Price;
                        info.FontColor = Brushes.Green;
                    }
                    info.Visual = "+" + value;
                    GameEngine._ParticleManager.AddRequest(info);

                    OnPriceCheckout?.Invoke(value);
                    CheckoutTextShowed = true;
                    Waiting = 400;
                }

                if (X < -250)
                {
                    GameEngine._GameManager.RemoveGameObject(this);
                }
            }
            else if (Catcher != null && Catcher2 != null)
            {
                double cX = Catcher.X;
                double cY = Catcher.Y;
                double cX2 = Catcher2.X;
                double cY2 = Catcher2.Y;

                double centerX = (cX + cX2) / 2;
                double centerY = (cY + cY2) / 2;

                double rWidth = COW;
                double rHeight = COH;

                double disX = cX - cX2;
                double disY = cY - cY2;
                if (disX != 0 || disY != 0)
                {
                    double dis = Math.Sqrt(Math.Pow(disX, 2) + Math.Pow(disY, 2));

                    if (dis > 650)
                    {
                        Split();
                        return;
                    }

                    double angle = (Math.Atan(disY / disX) / RADIAN);
                    if (disX > 0) angle += 180;
                    angle -= ActualDirection;
                    if (reversalScaleY) angle += 180;

                    if (angle < 0) angle += 360;
                    else if (angle > 360) angle %= 360;

                    double angleRad = angle * RADIAN;
                    rWidth = Math.Cos(angleRad) * dis;
                    rHeight = Math.Sin(angleRad) * dis;

                    if (reversalScaleX) rWidth = -rWidth;
                }

                double scaleX = rWidth / ImageWidth;
                double scaleY = rHeight / ImageHeight;
                ScaleX = scaleX;
                ScaleY = scaleY;
                SetPosition(centerX, centerY);
            }
            else if (!Catched && !NeverCatched)
            {
                SpeedX *= 0.99;
                SpeedY += 980 / GameManager.ProcessingRate;
            }

            if (!InCart)
            {
                if (Y > GameEngine.FieldHeight + 250)
                {
                    if (Splitted)
                    {
                        InCart = true;
                        GameRunner.GotClothes.Add(this);                        
                    }

                    GameEngine._GameManager.RemoveGameObject(this);
                }
            }
        }

        public bool PutIntoCart(ShoppingCart cart)
        {
            if (InCart) return false;

            InCart = true;
            GameEngine._GameManager.RemoveGameObject(this);
            return true;
        }

        public void Staine()
        {
            Stained = true;
            SetImage(GetStainedImage(myType));
        }
    }
}
