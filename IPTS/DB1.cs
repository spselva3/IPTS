using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTS
{
    public class DB1 : INotifyPropertyChanged
    {
        public bool X_0_0_Weight_1_Entry_Sensor { get; set; }
        public bool X_0_1_Weight_1_Exitt_Sensor { get; set; }
        public bool X_0_2_Weight_2_Entry_Sensor { get; set; }
        public bool X_0_3_Weight_2_Exitt_Sensor { get; set; }
        public bool X_0_4_Weight_3_Entry_Sensor { get; set; }
        public bool X_0_5_Weight_3_Exitt_Sensor { get; set; }
        public bool X_0_6_Dummy { get; set; }
        public bool X_0_7_Dummy { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataChangedEventArgs> DataChanged;
        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            if (DataChanged != null)
            {
                DataChanged.Invoke(this, new DataChangedEventArgs()
                {
                    NewValue = bool.Parse(after.ToString()),
                    OldValue = bool.Parse(before.ToString()),
                    PropertyName = propertyName
                });
            }
        }
    }

    public class DataChangedEventArgs
    {
        public bool NewValue { get; set; }
        public bool OldValue { get; set; }
        public string PropertyName { get; set; }
    }

}
