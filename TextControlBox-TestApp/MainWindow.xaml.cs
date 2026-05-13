using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            //textbox.LoadLines(Enumerable.Range(0, 5_000_000).Select(x => "Line " + x + " is cool right?"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.NumberOfSpacesForTab = 8;
            textbox.UseSpacesInsteadTabs = false;
            textbox.ShowWhitespaceCharacters = true;

            SetWindowTheme(this, ElementTheme.Default);

            textbox.LinkClicked += Textbox_LinkClicked;
            textbox.DispatcherQueue.TryEnqueue(() =>
            {
                textbox.RequestedTheme = ElementTheme.Default;
            });
            foreach(var Language in Enum.GetValues<SyntaxHighlightID>().Cast<SyntaxHighlightID>().OrderBy(x => x.ToString()))
            {
                LanguageComboBox.Items.Add(new ComboBoxItem { Content = Language.ToString(), IsSelected = Language == SyntaxHighlightID.CSharp });
            }
            //ThemeComboBox.ItemsSource = Enum.GetValues<ElementTheme>().OrderBy(x => x.ToString()).ToArray();
            //LanguageComboBox.ItemsSource = Enum.GetValues<SyntaxHighlightID>().OrderBy(x => x.ToString()).ToArray();
        }

        private void Textbox_LinkClicked(TextControlBox sender, string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static void SetWindowTheme(Window window, ElementTheme theme)
        {
            if (window.Content is FrameworkElement frame)
                frame.RequestedTheme = theme;
        }


        public void RewriteTabsSpaces(int spaces)
        {
            bool useSpaces = spaces != -1;
            this.textbox.RewriteTabsSpaces(spaces == -1 ? 4 : spaces, useSpaces);
        }

        private void Format1_Click(object sender, RoutedEventArgs e)
        {
            RewriteTabsSpaces(-1);
        }
        private void Format2_Click(object sender, RoutedEventArgs e)
        {
            RewriteTabsSpaces(2);

        }
        private void Format3_Click(object sender, RoutedEventArgs e)
        {
            RewriteTabsSpaces(4);

        }
        private void Format4_Click(object sender, RoutedEventArgs e)
        {
            RewriteTabsSpaces(8);

        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textbox == null)
                return;
            var theme = Enum.Parse<ElementTheme>(((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString());
            SetWindowTheme(this, theme);
            textbox.DispatcherQueue.TryEnqueue(() =>
            {
                textbox.RequestedTheme = theme;
            });
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textbox == null) return;
            var language = Enum.Parse<SyntaxHighlightID>(((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString());
            textbox.SelectSyntaxHighlightingById(language);
        }
    }
}
