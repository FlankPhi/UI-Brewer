using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace UI_Brewer.Converters
{

    class ValueToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush retColor = new SolidColorBrush();
            retColor.Color = Windows.UI.Colors.Red;
            if (Math.Abs((int)value) <= 1)
            {
                retColor.Color = Windows.UI.Colors.LawnGreen;
            }
            else if (Math.Abs((int)value) <= 2)
            {
                retColor.Color = Windows.UI.Colors.YellowGreen;
            }else
            {
                retColor.Color = Windows.UI.Colors.Red;
            }
            return retColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
