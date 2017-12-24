using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class SqaureLedDisplayViewModel : PluginNode
    {
        private readonly DisplayInfo _displayInfo = new DisplayInfo();

        private readonly List<Rectangle> _ledRectangles = new List<Rectangle>();
        private bool _diminsionsLabelTextVisible;
        private long _lastDisplayUpdateTimeInMilliseconds;

        private Action _layoutLedGridAction;
        private int _xLedCount;
        private int _yLedCount;
        private SqaureLedDisplayView SqaureLedView { get; set; }

        private int XLedCount
        {
            get => _xLedCount;
            set
            {
                if (value <= 0 || value > 100) return;
                _xLedCount = value;
                LayoutLedGrid();
            }
        }

        private int YLedCount
        {
            get => _yLedCount;
            set
            {
                if (value <= 0 || value > 100) return;
                _yLedCount = value;
                LayoutLedGrid();
            }
        }

        public string DiminsionsLabelText => XLedCount + " x " + YLedCount;

        public bool DiminsionsLabelTextVisible
        {
            get { return _diminsionsLabelTextVisible; }
            set
            {
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    DiminsionsLabelTextVisible = false;
                });
                _diminsionsLabelTextVisible = value;
            }
        }

        public Terminal<IVisualEffect> VisualEffectInputTerminal { get; set; }
        public Terminal<DisplayInfo> DisplayOutputTerminal { get; set; }
        public Terminal<DisplayInfo> SeriesDisplayInputTerminal { get; set; }

        public bool ButtonsVisible { get; set; }
        public int ButtonSpace => ButtonsVisible ? 10 : 0;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(120, 120);
            setup.NodeName("LedDisplay");
            setup.EnableResize();
            SeriesDisplayInputTerminal = setup.InputTerminal<DisplayInfo>("Series Display Input", Direction.North);
            DisplayOutputTerminal = setup.OutputTerminal<DisplayInfo>("Display Info Output", Direction.South);
            VisualEffectInputTerminal = setup.InputTerminal<IVisualEffect>("Visual Effect", Direction.East);

            SeriesDisplayInputTerminal.DataChanged += SeriesDisplayInputTerminalOnDataChanged;
            VisualEffectInputTerminal.DataChanged += VisualEffectInputTerminalOnDataChanged;
            _xLedCount = 8;
            _yLedCount = 8;
            LayoutLedGrid();

            DisplayOutputTerminal.Data = _displayInfo;
        }

        private void VisualEffectInputTerminalOnDataChanged(IVisualEffect data)
        {
            _displayInfo.VisualEffect = data;
        }

        private void SeriesDisplayInputTerminalOnDataChanged(DisplayInfo data)
        {
            _displayInfo.ChainedDisplay = data;
        }

        private void SerialDataInOnDataChanged(byte[] data)
        {
            if (data == null) return;
            const int headerSize = 12;

            if (data.Length < headerSize + XLedCount * YLedCount * 3) return;
            if (data[5] != 0) return;

            if (DateTime.Now.Ticks / 10000 - _lastDisplayUpdateTimeInMilliseconds <= 33) return; 
            _lastDisplayUpdateTimeInMilliseconds = DateTime.Now.Ticks / 10000;

            var ledNumber = 0;
            for (var i = 0; i < XLedCount * YLedCount * 3; i += 3)
            {
                var rIndex = headerSize + i;
                var gIndex = headerSize + i + 1;
                var bIndex = headerSize + i + 2;

                _ledRectangles[ledNumber].Fill = new SolidColorBrush(Color.FromArgb(255, data[rIndex], data[gIndex], data[bIndex]));
                ledNumber++;
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            SqaureLedView = (SqaureLedDisplayView) View;
            _layoutLedGridAction?.Invoke();
        }

        private void LayoutLedGrid()
        {
            _layoutLedGridAction = null;
            if (View == null)
            {
                _layoutLedGridAction = LayoutLedGrid;
                return;
            }

            var ledGrid = SqaureLedView.LedGrid;
            ledGrid.Children.Clear();
            ledGrid.ColumnDefinitions.Clear();
            ledGrid.RowDefinitions.Clear();
            _ledRectangles.Clear();

            for (var x = 0; x < XLedCount; x++)
                ledGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (var y = 0; y < YLedCount; y++)
                ledGrid.RowDefinitions.Add(new RowDefinition());

            for (var x = 0; x < XLedCount; x++)
            for (var y = 0; y < YLedCount; y++)
            {
                var rectangle = new Rectangle();
                rectangle.Margin = new Thickness(1);
                rectangle.Fill = Brushes.Black;
                Grid.SetColumn(rectangle, x);
                Grid.SetRow(rectangle, y);
                ledGrid.Children.Add(rectangle);
                _ledRectangles.Add(rectangle);
            }

            _displayInfo.DisplaySize = (byte) (XLedCount * YLedCount);
        }

        public void AddXLed()
        {
            XLedCount++;
            DiminsionsLabelTextVisible = true;
        }

        public void RemoveXLed()
        {
            XLedCount--;
            DiminsionsLabelTextVisible = true;
        }

        public void AddYLed()
        {
            YLedCount++;
            DiminsionsLabelTextVisible = true;
        }

        public void RemoveYLed()
        {
            YLedCount--;
            DiminsionsLabelTextVisible = true;
        }

        public void MouseEnteredNode()
        {
            _displayEffectEnabled = true;
            new Thread(DisplayEffect).Start();
        }

        public void MouseLeftNode()
        {
            ButtonsVisible = false;
            _displayEffectEnabled = false;
        }

        public void PreviewLeftMouseDownOnNode()
        {
            ButtonsVisible = true;
        }

        private bool _displayEffectEnabled;

        private void DisplayEffect()
        {
            while (_displayEffectEnabled)
            {
                Thread.Sleep(33);

                if (_displayInfo.VisualEffect == null) return;
                var graphic = _displayInfo.VisualEffect.GetEffect();
                if (graphic == null) return;
                if (graphic.Length > 3 * _displayInfo.DisplaySize) continue;
                _ledRectangles[0].Dispatcher.BeginInvoke(new Action(() =>
                {
                    var ledNumber = 0;
                    for (var i = 0; i < XLedCount * YLedCount * 3; i += 3)
                    {
                        _ledRectangles[ledNumber++].Fill = new SolidColorBrush(Color.FromArgb(255, graphic[i], graphic[i + 1], graphic[i + 2]));
                    }
                }));
            }
        }
    }
}