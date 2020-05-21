using HTML_Parser_Framework.VIEWMODEL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HTML_Parser_Framework.VIEW
{
    /// <summary>
    /// Interaction logic for LinkCounterWindow.xaml
    /// </summary>
    public partial class LinkCounterWindow : Window
    {
        public LinkCounterWindow()
        {
            InitializeComponent();
            DataContext = new LinkCounterWindowViewModel();
        }
    }
}
