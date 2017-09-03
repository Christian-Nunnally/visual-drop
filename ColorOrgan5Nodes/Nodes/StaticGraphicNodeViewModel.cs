using ColorOrgan5Nodes.NodeTools;
using DiagramEditor.Model;
using DiagramEditor.ViewModel.Diagram;
using PropertyChanged;
using Stylet;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ColorOrgan5Nodes.Nodes
{
    [Serializable]
    [AddINotifyPropertyChangedInterface]
    public class StaticGraphicNodeViewModel : PluginNodeViewModel, IViewAware
    {
        private const string SwitchTriggeredDelegateKey = "SwitchTriggered";
        private StaticLedGraphic _graphic;
        private LedDisplay _led;
        private StaticLedGraphic _offGraphic;
        private OutputTerminalViewModel _outputTerminal;
        private Random _rnd = new Random();

        public override string Name => "Static Graphic Node";

        public SolidColorBrush SelectedColor { get; set; }

        public byte RedValue { get; set; }
        public byte GreenValue { get; set; }
        public byte BlueValue { get; set; }

        public bool IsSymmetryOptionChecked { get; set; }

        public bool IsColorPickerVisible { get; set; }

        public void ColorValueChanged()
        {
            SelectedColor = new SolidColorBrush(Color.FromRgb(RedValue, GreenValue, BlueValue));
        }

        public override void NodeLoaded()
        {
            var view = (StaticGraphicNodeView)View;
            _led = view.Display;
            _led.PixelClicked += DisplayOnPixelClicked;

            _graphic = new StaticLedGraphic();
            _offGraphic = new StaticLedGraphic();

            SelectedColor = new SolidColorBrush(Colors.Green);
        }

        public override void ConstructTerminals()
        {
            ConstructNewInputTerminal("On/Off", typeof(bool), Direction.North, SwitchTriggeredDelegateKey);
            _outputTerminal = ConstructNewOutputTerminal("Image", typeof(byte[]), Direction.South);
        }

        public override void SetupDelegates(DelegateMapper delegateMapper)
        {
            delegateMapper.AddMapping(SwitchTriggeredDelegateKey, SwitchTriggered);
        }

        private void DisplayOnPixelClicked(int pixelNumber, EventArgs eventArgs)
        {
            ColorPixel(pixelNumber, _graphic.IsPixelOff(pixelNumber) ? SelectedColor.Color : Colors.Black);
            _led.Set(_graphic.Graphic);
        }

        private void ColorPixel(int pixelNumber, Color color)
        {
            if (IsSymmetryOptionChecked)
            {
                var x = pixelNumber % 8;
                var y = pixelNumber / 8;

                var otherX = 7 - x;
                var otherY = 7 - y;

                var pixelNumber1 = x + 8 * y;
                var pixelNumber2 = x + 8 * otherY;
                var pixelNumber3 = otherX + 8 * y;
                var pixelNumber4 = otherX + 8 * otherY;

                _graphic.SetPixel(pixelNumber1, color);
                _graphic.SetPixel(pixelNumber2, color);
                _graphic.SetPixel(pixelNumber3, color);
                _graphic.SetPixel(pixelNumber4, color);
            }
            else
            {
                _graphic.SetPixel(pixelNumber, color);
            }
        }

        private IDictionary<OutputTerminal, object> SwitchTriggered(object o)
        {
            var switchValue = (bool)o;
            var result = new Dictionary<OutputTerminal, object>();
            var displayingGraphic = switchValue ? _graphic.Graphic : _offGraphic.Graphic;
            result.Add(_outputTerminal.OutputTerminal, displayingGraphic);
            _led.Set(displayingGraphic);
            return result;
        }

        public void MouseOverColorPickerTabChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            if (newValue) IsColorPickerVisible = true;
        }

        public void MouseLeavingColorPicker()
        {
            IsColorPickerVisible = false;
        }
    }
}