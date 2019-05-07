using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarketFighter.GO
{
    public class EnemyHand : Hand
    {

        private static ImageSource red_hand;
        private static ImageSource red_hand_grip;
        private static Animation HandGripAnim;

        private long latestGripRelease;
        private bool CatchOnece;

        public override void OnInitialize()
        {
            SetImage(image_Hand);
            GameEngine.RunOnUIThread(
                new Action<System.Windows.UIElement, int>(System.Windows.Controls.Panel.SetZIndex),
                GetSprite(), 9999);
        }

        public override void OnLoad()
        {
            if (red_hand == null || red_hand_grip == null)
            {
                red_hand = ImageFactory.GetColorFilteredBitmap(
                    GameEngine.GetImage("./Sprite/hand.png"), Colors.Red, 0.5);
                red_hand.Freeze();

                red_hand_grip = ImageFactory.GetColorFilteredBitmap(
                    GameEngine.GetImage("./Sprite/hand_grip.png"), Colors.Red, 0.5);
                red_hand_grip.Freeze();
            }

            image_Hand = red_hand;
            image_Hand_Grip = red_hand_grip;
        }

        public override void Run()
        {
            if (!CatchOnece)
            {
                long now = GameEngine.GetMillisecond();
                if (now - latestGripRelease > 170)
                {
                    latestGripRelease = now;
                    if (Gripped)
                    {
                        Release();
                    }
                    else
                    {
                        if (Catch())
                        {
                            CatchOnece = true;
                        }
                    }
                }                
            }

            if (!Catched &&
                X < -100 || X > GameEngine.FieldWidth + 100)
            {
                GameEngine._GameManager.RemoveGameObject(this);
            }
        }
    }
}
