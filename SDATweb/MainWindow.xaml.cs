using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using System.Text;

namespace SDATweb
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void NewPage(object sender, RoutedEventArgs e)
        {
            lb_pages.Items.Add(1);
        }

        private void SendRequest(object sender, RoutedEventArgs e)
        {
            Button sendButton = sender as Button;

            if (sendButton != null)
            {
                StackPanel parentPanel = sendButton.Parent as StackPanel;

                if (parentPanel != null)
                {
                    TextBox smallerTextBox = parentPanel.Children[0] as TextBox;
                    StackPanel outerPanel = parentPanel.Parent as StackPanel;
                    TextBox largerTextBox = outerPanel.Children[1] as TextBox;

                    if (smallerTextBox != null && largerTextBox != null)
                    {
                        largerTextBox.Text = "Waiting for response...";
                        sendHTTP(smallerTextBox.Text, largerTextBox);
                    }
                }
            }
        }
        
        private async void sendHTTP(string query, TextBox htmlBox)
        {
            string url = urlBox.Text;
            string inputData = query;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent(inputData, Encoding.UTF8, "text/plain");

                    HttpResponseMessage response = await client.PostAsync(url, content);

                    string responseString = await response.Content.ReadAsStringAsync();
                    htmlBox.Text = responseString;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
