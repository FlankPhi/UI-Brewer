using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace UI_Brewer.Converters
{
    class FromStateToColor : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush retColor = new SolidColorBrush();
            retColor.Color = Windows.UI.Colors.Red;
            if ((bool)value)
            {
                retColor.Color = Windows.UI.Colors.Red;
            }
            else
            {
                retColor.Color = Windows.UI.Colors.Gray;
            }
            return retColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
