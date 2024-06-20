using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace Game
{
    public partial class MainWindow : Window
    {
        private Horse[] _horses;
        private Horse[] _horseOnScreen;
        private CancellationTokenSource _cancellationTokenSource;
        private int BankAccount { get; set; }
        private int Reserve = 20;
        private string HorseBetName { get; set; }
        private int horseIndex = 1;
        public MainWindow()
        {
            _horses = new Horse[5]
            {
                new Horse("Кондор", Brushes.LimeGreen),
                new Horse("Сніг", Brushes.White),
                new Horse("Тінь", Brushes.Navy),
                new Horse("Вишня", Brushes.LightPink),
                new Horse("Ворон", Brushes.Firebrick)
            };

            _horseOnScreen = (Horse[])_horses.Clone();

            BankAccount = 250;

            InitializeComponent();
            this.Closing += MainWindow_Closing;

        }
        private void MainWindow_Closing(object sender,System.ComponentModel.CancelEventArgs e)
        {
             Process.GetCurrentProcess().Kill();
        }
        public void StopProcess()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void SetHorses(Horse[] horses)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateHorseInformation(horses[0], firstColor, firstName, firstCoefficient, firstTime, firstPosition);
                UpdateHorseInformation(horses[1], secondColor, secondName, secondCoefficient, secondTime, secondPosition);
                UpdateHorseInformation(horses[2], thirdColor, thirdName, thirdCoefficient, thirdTime, thirdPosition);
                UpdateHorseInformation(horses[3], fourthColor, fourthName, fourthCoefficient, fourthTime, fourthPosition);
                UpdateHorseInformation(horses[4], fifthColor, fifthName, fifthCoefficient, fifthTime, fifthPosition);
            });
        }

        public void UpdateHorseInformation(Horse horse, Rectangle color, Label name, Label acceration, Label time, Label position)
        {
            color.Fill = horse.Color;
            name.Content = horse.Name;
            acceration.Content = horse.Accelaration;
            time.Content = horse.Timer.Elapsed;
            position.Content = horse.Position;
        }

        public async Task LaunchHorses(Horse[] horses)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < horses.Length; i++)
            {
                tasks.Add(horses[i].RunAsync());
            }

            await Task.WhenAll(tasks);
        }

        private List<Task> RenderHorseAnimation()
        {
            List<List<ImageSource>> horsesAnimation = new List<List<ImageSource>>();

            Color[] colors = new Color[5] { Colors.LimeGreen, Colors.White, Colors.Navy, Colors.LightPink, Colors.Firebrick };
            
            foreach (var color in colors)
            {
                horsesAnimation.Add(GetHorseAnimation(color));
            }

            List<Task> horsesTask = new List<Task>();
            horsesTask.Add(PlayAnimation(horsesAnimation[0], horse_1));
            horsesTask.Add(PlayAnimation(horsesAnimation[1], horse_2));
            horsesTask.Add(PlayAnimation(horsesAnimation[2], horse_3));
            horsesTask.Add(PlayAnimation(horsesAnimation[3], horse_4));
            horsesTask.Add(PlayAnimation(horsesAnimation[4], horse_5));

            return horsesTask;
        }
       private async void RunProgram(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            List<Task> horsesAnimation = RenderHorseAnimation();

            Task updateRatingPositionHorses = UpdateRatingPositionHorses();
            Task launchHorses = LaunchHorses(_horses);
            Task changePositionHorses = ChangePositionHorses();

            await Task.WhenAll(launchHorses, updateRatingPositionHorses, changePositionHorses);
            await Task.WhenAll(horsesAnimation);

            MessageBox.Show("Гонка закінчилась!");
        
        }

        private async Task PlayAnimation(List<ImageSource> animationFrames, Image targetImage)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                foreach (var frame in animationFrames)
                {
                    await Task.Run(() =>
                    {
                        targetImage.Dispatcher.Invoke(() =>
                        {
                            targetImage.Source = frame;
                        });
                    });

                    await Task.Delay(TimeSpan.FromSeconds(0.1));

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                }
            }
        }
        private async Task ChangePositionHorses()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PositionChanges(horse_1, _horses[0].Position);
                        PositionChanges(horse_2, _horses[1].Position);
                        PositionChanges(horse_3, _horses[2].Position);
                        PositionChanges(horse_4, _horses[3].Position);
                        PositionChanges(horse_5, _horses[4].Position);
                    });

                    Task.Delay(10).Wait();

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                }
            });
        }
        private void PositionChanges(Image horse, double position)
        {
            double horseChangePositionValue = (position + 100) % 900;
            horse.Margin = new Thickness(horseChangePositionValue, 0, 0, 0);
        }
        public List<ImageSource> GetHorseAnimation(Color color)
        {
            const int count = 12;
            var bitmap_image_list = ReadImageList(@"Images\Horses", "WithOutBorder_", ".png", count);
            var mask_image_list = ReadImageList(@"Images\HorsesMask", "mask_", ".png", count);

            return bitmap_image_list.Select((item, index) => GetImageWithColor(item, mask_image_list[index], color)).ToList();
        }

        private List<BitmapImage> ReadImageList(string path, string name, string format, int count)
        {
            path = $@"C:\AllFiles\cnulabs\cnulabs\OOP\Images\{path}\{name}";
            List<BitmapImage> list = new List<BitmapImage>();
            for(int i = 0; i < count; i++)
            {
                var uri = path + string.Format("{0:0000}", i) + format;
                var img = new BitmapImage(new Uri(uri));
                list.Add(img);
            }

            return list;
        }

        private ImageSource GetImageWithColor(BitmapImage image, BitmapImage mask, Color color)
        {
            WriteableBitmap image_bmp = new WriteableBitmap(image);
            WriteableBitmap mask_bmp = new WriteableBitmap(mask);
            WriteableBitmap output_bmp = BitmapFactory.New(image.PixelWidth, image.PixelHeight);
            output_bmp.ForEach((x, y, z) =>
            {
                return MultiplyColors(image_bmp.GetPixel(x, y), color, mask_bmp.GetPixel(x, y).A);
            });

            return output_bmp;
        }
         
        private Color MultiplyColors(Color color_1, Color color_2, byte alpha)
        {
            var amount = alpha / 255.0;
            byte r = (byte)(color_2.R * amount + color_1.R * (1 - amount));
            byte g = (byte)(color_2.G * amount + color_1.G * (1 - amount));
            byte b = (byte)(color_2.B * amount + color_1.B * (1 - amount));
            return Color.FromArgb(color_1.A, r, g, b);
        }

        public async Task UpdateRatingPositionHorses()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!_horses[0].Timer.IsRunning && !_horses[1].Timer.IsRunning && !_horses[2].Timer.IsRunning && !_horses[3].Timer.IsRunning && !_horses[4].Timer.IsRunning)
                    {
                        _horses = Horse.ChangePositionRaiting(_horses);

                        if (!string.IsNullOrEmpty(HorseBetName) && HorseBetName.Contains(_horses[0].Name))
                        {
                            BankAccount += Reserve * 2;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            BalanceContent.Content = $"Баланс: {BankAccount}$";
                        });

                        StopProcess();             
                    }

                    Dispatcher.Invoke(() =>
                    {
                        SetHorses(_horses);
                    });

                    _horses = Horse.ChangePlace(_horses);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            horseIndex %= 5;
            HorsesNameContent.Content = $"{horseIndex + 1}. " +_horses[horseIndex].Name;
            horseIndex++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (horseIndex == 0)
            {
                horseIndex = _horses.Length - 1;
            }
            else
            {
                horseIndex--;
            }
            
            HorsesNameContent.Content = $"{horseIndex + 1}. " + _horses[horseIndex].Name;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Reserve += 5;
            MoneyThatPayed.Content = Reserve.ToString() + "$";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(Reserve > 0)
            {
                Reserve -= 5;
            }
            MoneyThatPayed.Content = Reserve.ToString() + "$";
        }
        
        private void Bet(object sender, RoutedEventArgs e)
        {
            if(BankAccount - Reserve >= 0)
            {
                BankAccount -= Reserve;
                BalanceContent.Content = $"Баланс: {BankAccount}$";
                MessageBox.Show($"Ви зробили ставку на {HorsesNameContent.Content} {Reserve}$");
                HorseBetName = HorsesNameContent.Content.ToString();
            }
            else
            {
                MessageBox.Show($"Не вистачає коштів. Потрібно {Reserve - BankAccount}");
            }
        }
    }
}