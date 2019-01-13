using PropertyChanged;
using System;
using System.Runtime.Serialization;

namespace VisualDrop
{
    [AddINotifyPropertyChangedInterface]
    [Serializable]
    public class AudioDeviceInformation
    {
        private string _name;

        [DataMember]
        public string Name
        {
            get => _name;

            set
            {
                _name = value;
                if (_name.Contains("("))
                {
                    DisplayName = _name.Substring(0, _name.LastIndexOf('('));
                }
                if (DisplayName.Contains(" - "))
                {
                    DisplayName = DisplayName.Substring(DisplayName.IndexOf(" - ") + 3);
                }
            }
        }

        public string DisplayName { get; set; }
    }
}
