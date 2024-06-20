using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Voronoi_firs
{
    public partial class Form1 : Form
    {
        private Point? _draggedPoint = null;
        private Random random = new Random();
        private readonly List<Point> _points = new List<Point>();
        private readonly Dictionary<Point, Color> _cellColors = new Dictionary<Point, Color>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _points.Clear();
            _cellColors.Clear();
            int numPoints = 2;

            var rand = new Random();
            for (var i = 0; i < numPoints; i++)
            {
                var point = new Point(rand.Next(pictureBox1.Width), rand.Next(pictureBox1.Height));
                _points.Add(point);

                var color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                _cellColors[point] = color;
            }

            MultiThreadVoronoi();
        }

        private void MultiThreadVoronoi() // малювання діаграми Вороного
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            int numThreads = 5;
            int regionWidth = bmp.Width / numThreads;
            List<Rectangle> regions = new List<Rectangle>();
            for (int i = 0; i < numThreads; i++)
            {
                regions.Add(new Rectangle(i * regionWidth, 0, regionWidth, bmp.Height));
            }

            var tasks = new List<Task>();
            foreach (var region in regions)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (var x = region.X; x < region.X + region.Width; x++)
                    {
                        for (var y = region.Y; y < region.Y + region.Height; y++)
                        {
                            var closestPoint = FindPoint(new Point(x, y));

                            if (closestPoint != null)
                            {
                                lock (_cellColors)
                                {
                                    bmp.SetPixel(x, y, _cellColors[closestPoint.Value]);
                                }
                            }
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            DrawPoints(bmp);
            pictureBox1.Image = bmp;
        }

        private void DrawPoints(Bitmap bmp)
        {
            using (var g = Graphics.FromImage(bmp))
            {
                var pointSize = 5;
                foreach (var point in _points)
                {
                    var brush = new SolidBrush(Color.Black);
                    g.FillEllipse(brush, point.X - pointSize / 2, point.Y - pointSize / 2, pointSize, pointSize);
                }
            }

            pictureBox1.Image = bmp;
        }

        private Point? FindPoint(Point p)
        {
            var minDistance = double.MaxValue;
            Point? closestPoint = null;

            foreach (var point in _points)
            {
                var distance = Math.Sqrt(Math.Pow(p.X - point.X, 2) + Math.Pow(p.Y - point.Y, 2));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var clickedPoint = _points.FirstOrDefault(p => IsPointClicked(p, e.Location));
            if (clickedPoint != default(Point))
            {
                _draggedPoint = clickedPoint;
            }
            else
            {
                _points.Add(new Point(e.X, e.Y));
                _cellColors.Add(new Point(e.X, e.Y), Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                MultiThreadVoronoi();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedPoint.HasValue)
            {
                _points[_points.IndexOf(_draggedPoint.Value)] = new Point(e.X, e.Y);

                MultiThreadVoronoi();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _draggedPoint = null;
        }

        private bool IsPointClicked(Point point, Point location)
        {
            const int tolerance = 5;
            return Math.Abs(point.X - location.X) <= tolerance && Math.Abs(point.Y - location.Y) <= tolerance;
        }
    }
}