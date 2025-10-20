using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;
using WindowsInput;

namespace TaxiBot
{
    internal class Program
    {
        private static int pollDelay = 1;
        private static Color targetColor = Color.FromArgb(0x30, 0x43, 0x68);
        private static int tolerance = 5;
        private static Rectangle region = new Rectangle(1120, 350, 230, 550);
        private const string soundFile = "sound.mp3";

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "GTA5RP TaxiBot";

            var sim = new InputSimulator();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== GTA5RP TaxiBot ===");
                Console.WriteLine("1. Запустить бота");
                Console.WriteLine("2. Выйти");
                Console.Write("\n> ");
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
                {
                    Console.Clear();
                    Console.WriteLine("Бот запущен. Ожидание вызова...");
                    RunBot(sim);
                    Console.WriteLine("\nВызов найден. Нажмите любую клавишу, чтобы вернуться в меню.");
                    Console.ReadKey(true);
                }
                else if (key == ConsoleKey.D2 || key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }

        static void RunBot(InputSimulator sim)
        {
            while (true)
            {
                if (FindColor(region, targetColor, tolerance, out Point p))
                {
                    MoveAndClick(sim, p.X, p.Y);
                    PlaySound(soundFile);
                    break;
                }
                Thread.Sleep(pollDelay);
            }
        }

        static bool FindColor(Rectangle region, Color target, int tol, out Point point)
        {
            using var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(region.X, region.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (Math.Abs(c.R - target.R) < tol &&
                        Math.Abs(c.G - target.G) < tol &&
                        Math.Abs(c.B - target.B) < tol)
                    {
                        point = new Point(region.X + x, region.Y + y);
                        return true;
                    }
                }

            point = Point.Empty;
            return false;
        }

        static void MoveAndClick(InputSimulator sim, int x, int y)
        {
            int w = GetSystemMetrics(0);
            int h = GetSystemMetrics(1);
            double nx = x * 65535.0 / Math.Max(w - 1, 1);
            double ny = y * 65535.0 / Math.Max(h - 1, 1);
            sim.Mouse.MoveMouseTo(nx, ny);
            sim.Mouse.LeftButtonClick();
        }

        static void PlaySound(string path)
        {
            if (!File.Exists(path)) return;
            using var reader = new AudioFileReader(path);
            using var output = new WaveOutEvent();
            output.Init(reader);
            output.Play();
            while (output.PlaybackState == PlaybackState.Playing)
                Thread.Sleep(10);
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
    }
}
