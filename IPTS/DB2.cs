using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTS
{
    public class DB2 : INotifyPropertyChanged
    {
        public bool X_0_0_Traffic_Light { get; set; }

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
    

}
