using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MeteoPanel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HambMenu : Page
    {
        public static bool button1_selected, button2_selected, button3_selected, button4_selected, button5_selected;

        public HambMenu()
        {
            this.InitializeComponent();

            MenuButtonSet("Button 1");
            Menu_Button1.IsChecked = true;
            MySplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            Frame_Pantalla.Navigate(typeof(Pantalla1));
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (MySplitView.IsPaneOpen)
            {
                Menu_Label.Text = "Menu";
                Menu_Button1.FontSize = 18;
                Menu_Button2.FontSize = 18;
                Menu_Button3.FontSize = 18;
                Menu_Button4.FontSize = 18;
                Menu_Button5.FontSize = 18;
                Menu_Button1.Content = " \u2AD8  Conexión";
                Menu_Button2.Content = " \uE2C8  Monitor de datos";
                Menu_Button3.Content = " \uE2AD  Registro";
                Menu_Button4.Content = " \uE115  Configuración";
                Menu_Button5.Content = "  \u003F    Información";
            }
            else
            {
                Menu_Label.Text = " ";
                Menu_Button1.FontSize = 24;
                Menu_Button2.FontSize = 24;
                Menu_Button3.FontSize = 24;
                Menu_Button4.FontSize = 24;
                Menu_Button5.FontSize = 24;
                Menu_Button1.Content = "  \u2AD8";
                Menu_Button2.Content = "  \uE2C8";
                Menu_Button3.Content = "  \uE2AD";
                Menu_Button4.Content = "  \uE115";
                Menu_Button5.Content = "   \u003F";
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void MenuButton1_clicked(object sender, RoutedEventArgs e)
        {
            if (Menu_Button2.IsChecked == true)
                Menu_Button2.IsChecked = false;
            if (Menu_Button3.IsChecked == true)
                Menu_Button3.IsChecked = false;
            if (Menu_Button4.IsChecked == true)
                Menu_Button4.IsChecked = false;
            if (Menu_Button5.IsChecked == true)
                Menu_Button5.IsChecked = false;

            if (MenuButtonGet("Button 1") == false)
            {
                Frame_Pantalla.Navigate(typeof(Pantalla1));
                MenuButtonSet("Button 1");
            }
        }

        private void MenuButton2_clicked(object sender, RoutedEventArgs e)
        {
            if (Menu_Button1.IsChecked == true)
                Menu_Button1.IsChecked = false;
            if (Menu_Button3.IsChecked == true)
                Menu_Button3.IsChecked = false;
            if (Menu_Button4.IsChecked == true)
                Menu_Button4.IsChecked = false;
            if (Menu_Button5.IsChecked == true)
                Menu_Button5.IsChecked = false;

            if (MenuButtonGet("Button 2") == false)
            {
                Frame_Pantalla.Navigate(typeof(Pantalla2));
                MenuButtonSet("Button 2");
            }
        }


        private void MenuButton3_clicked(object sender, RoutedEventArgs e)
        {
            if (Menu_Button1.IsChecked == true)
                Menu_Button1.IsChecked = false;
            if (Menu_Button2.IsChecked == true)
                Menu_Button2.IsChecked = false;
            if (Menu_Button4.IsChecked == true)
                Menu_Button4.IsChecked = false;
            if (Menu_Button5.IsChecked == true)
                Menu_Button5.IsChecked = false;

            if (MenuButtonGet("Button 3") == false)
            {
                Frame_Pantalla.Navigate(typeof(Pantalla3));
                MenuButtonSet("Button 3");
            }
        }

        private void MenuButton4_clicked(object sender, RoutedEventArgs e)
        {
            if (Menu_Button1.IsChecked == true)
                Menu_Button1.IsChecked = false;
            if (Menu_Button2.IsChecked == true)
                Menu_Button2.IsChecked = false;
            if (Menu_Button3.IsChecked == true)
                Menu_Button3.IsChecked = false;
            if (Menu_Button5.IsChecked == true)
                Menu_Button5.IsChecked = false;

            if (MenuButtonGet("Button 4") == false)
            {
                Frame_Pantalla.Navigate(typeof(Pantalla4));
                MenuButtonSet("Button 4");
            }
        }

        private void MenuButton5_clicked(object sender, RoutedEventArgs e)
        {
            if (Menu_Button1.IsChecked == true)
                Menu_Button1.IsChecked = false;
            if (Menu_Button2.IsChecked == true)
                Menu_Button2.IsChecked = false;
            if (Menu_Button3.IsChecked == true)
                Menu_Button3.IsChecked = false;
            if (Menu_Button4.IsChecked == true)
                Menu_Button4.IsChecked = false;

            if (MenuButtonGet("Button 5") == false)
            {
                Frame_Pantalla.Navigate(typeof(Pantalla5));
                MenuButtonSet("Button 5");
            }
        }

        private void MenuButtonSet(String nameButton)
        {
            if (nameButton == "Button 1")
            {
                button1_selected = true;
                button2_selected = false;
                button3_selected = false;
                button4_selected = false;
                button5_selected = false;
            }
            else if (nameButton == "Button 2")
            {
                button1_selected = false;
                button2_selected = true;
                button3_selected = false;
                button4_selected = false;
                button5_selected = false;
            }
            else if (nameButton == "Button 3")
            {
                button1_selected = false;
                button2_selected = false;
                button3_selected = true;
                button4_selected = false;
                button5_selected = false;
            }
            else if (nameButton == "Button 4")
            {
                button1_selected = false;
                button2_selected = false;
                button3_selected = false;
                button4_selected = true;
                button5_selected = false;
            }
            else if (nameButton == "Button 5")
            {
                button1_selected = false;
                button2_selected = false;
                button3_selected = false;
                button4_selected = false;
                button5_selected = true;
            }
        }

        private bool MenuButtonGet(String nameButton)
        {
            if (nameButton == "Button 1")
                return button1_selected;
            else if (nameButton == "Button 2")
                return button2_selected;
            else if (nameButton == "Button 3")
                return button3_selected;
            else if (nameButton == "Button 4")
                return button4_selected;
            else if (nameButton == "Button 5")
                return button5_selected;
            else
                return false; // Default
        }
    }
}