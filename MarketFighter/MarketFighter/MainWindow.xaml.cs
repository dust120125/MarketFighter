using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;

using MarketFighter.GO;

namespace MarketFighter
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        private GameEngine Engine;
        private GameRunner GameRunner;
        private Hand PlayerHand, Player2Hand;

        public MainWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            this.MouseMove += MainWindow_MouseMove;
            this.MouseDown += MainWindow_MouseDown;
            this.MouseUp += MainWindow_MouseUp;

            this.KeyDown += MainWindow_KeyDown;

            BackgroundBrush.ImageSource = GameEngine.GetImage("./bg/bg1.jpg");
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Player2Hand != null)
            {
                int move = 20;
                switch (e.Key)
                {
                    case Key.W:
                        Player2Hand.SetPosition(Player2Hand.X, Player2Hand.Y - move);
                        break;
                    case Key.S:
                        Player2Hand.SetPosition(Player2Hand.X, Player2Hand.Y + move);
                        break;
                    case Key.A:
                        Player2Hand.SetPosition(Player2Hand.X - move, Player2Hand.Y);
                        break;
                    case Key.D:
                        Player2Hand.SetPosition(Player2Hand.X + move, Player2Hand.Y);
                        break;
                    case Key.E:
                        if (!e.IsRepeat)
                            if (!Player2Hand.Gripped) Player2Hand.Catch();
                            else Player2Hand.Release();
                        break;
                }
            }
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PlayerHand != null)
            {
                PlayerHand.Release();
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PlayerHand != null)
            {
                PlayerHand.Catch();
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (PlayerHand != null)
            {
                Point pos = e.GetPosition(mainCanvas);
                PlayerHand.SetPosition(pos.X, pos.Y);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GameRunner.End();
            Engine.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Engine = GameEngine.Create(mainCanvas);
            GameEngine._GameManager.Start();

            PlayerHand = new Hand("ph1");
            PlayerHand.SetPosition(mainCanvas.ActualWidth * 0.75, 0);
            GameEngine._GameManager.AddGameObject(PlayerHand);            

            Player2Hand = new Hand("ph2");
            Player2Hand.SetPosition(mainCanvas.ActualWidth * 0.25, 0);
            GameEngine._GameManager.AddGameObject(Player2Hand);

            GameRunner.Hands.Add(PlayerHand);
            GameRunner.Hands.Add(Player2Hand);

            GameRunner = new GameRunner();
            GameRunner.Start();

            //RunTest();
        }

        private void RunTest()
        {
            System.Threading.Thread ss = new System.Threading.Thread(Test);
            ss.Start();
        }

        private void Test()
        {
            Animation HandGripAnim = new Animation(null,
                GameEngine.GetImage("./Sprite/hand.png"), 0d, 0d, 0d, null, null, null, 45, 1,
                GameEngine.GetImage("./Sprite/hand_grip.png"), 0d, 0d, 0d, null, null, null, 45, 0
                );

            Random random = new Random();
            for (int i = 0; i < 80; i++)
            {
                System.Threading.Thread.Sleep(100);
                Hand hh = new Hand()
                {
                    Direction = 90
                };
                hh.SetPosition(random.Next(20, (int)GameEngine.FieldWidth - 20), 0);
                hh.SetSpeed(300, hh.Direction);
                hh.RegisterAnimation(new AnimationProvider(HandGripAnim, true));

                GameEngine._GameManager.AddGameObject(hh);
            }
        }

    }
}
