// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System.Windows;
using TestAppModels;

namespace TestAppCSharp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var initialSettings = new AudioSettings(volume: 0.0, pan: 0.0);
            var vm = new ViewModel(new Model(initialSettings));
            DataContext = vm;
        }
    }
}
