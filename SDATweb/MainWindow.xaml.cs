using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace SDATweb
{
    public sealed partial class MainWindow : Window
    {
        private List<string> pagesContent = new List<string>();
        private List<string> pagesName = new List<string>();
        HttpClient client = new HttpClient();

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void NewPage(object sender, RoutedEventArgs e)
        {
            lb_pages.Items.Add(1);
            pagesContent.Add("""
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Sample Page</title>
                </head>
                <body>
                    <h1>Welcome to My HTML Page</h1>
                    <p>This is a sample paragraph with proper line breaks.</p>
                </body>
                </html>
                """);
            pagesName.Add("Sample Page");
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

            using (client)
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

        private void BuildWebsite(object sender, RoutedEventArgs e)
        {
            clearSite();

            string deployFolder = "site";
            try
            {
                System.IO.Directory.CreateDirectory(deployFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder {deployFolder}: {ex.Message}");
                return;
            }

            // a navigation menu listing all pages
            var navLinks = new System.Text.StringBuilder();
            navLinks.AppendLine("<nav>");
            for (int i = 0; i < pagesContent.Count; i++)
            {
                navLinks.AppendLine($"<a href='{pagesName[i].Replace(" ", "")}{i}.html'>{pagesName[i]}</a>");
            }
            navLinks.AppendLine("</nav>");
            string navHtml = navLinks.ToString();

            // write each page with the navigation menu injected
            for (int i = 0; i < pagesContent.Count; i++)
            {
                string pageHtml = pagesContent[i];
                // Look for the <body> tag to insert the nav
                int bodyIndex = pageHtml.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
                if (bodyIndex >= 0)
                {
                    bodyIndex += "<body>".Length;
                    pageHtml = pageHtml.Insert(bodyIndex, navHtml);
                }
                else
                {
                    // If no <body> tag was found, append the nav at the end
                    pageHtml += navHtml;
                }

                string pageFileName = System.IO.Path.Combine(deployFolder, $"{pagesName[i].Replace(" ", "")}{i}.html");
                try
                {
                    System.IO.File.WriteAllText(pageFileName, pageHtml);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving file {pageFileName}: {ex.Message}");
                }
            }

            // Create an index.html that serves as the landing page with a list of links
            var indexContent = new System.Text.StringBuilder();
            indexContent.AppendLine("<!DOCTYPE html>");
            indexContent.AppendLine("<html>");
            indexContent.AppendLine($"<head><title>{nameBox.Text} Home</title></head>");
            indexContent.AppendLine("<body>");
            indexContent.AppendLine(navHtml);
            indexContent.AppendLine($"<h1>Welcome to the {nameBox.Text} Home Page</h1>");
            indexContent.AppendLine("<ul>");
            for (int i = 0; i < pagesContent.Count; i++)
            {
                indexContent.AppendLine($"<li><a href='{pagesName[i].Replace(" ", "")}{i}.html'>{pagesName[i]}</a></li>");
            }
            indexContent.AppendLine("</ul>");
            indexContent.AppendLine("</body>");
            indexContent.AppendLine("</html>");

            string indexFileName = System.IO.Path.Combine(deployFolder, "index.html");
            try
            {
                System.IO.File.WriteAllText(indexFileName, indexContent.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file {indexFileName}: {ex.Message}");
            }

            // Create a 404.html page for handling not found routes
            var notFoundContent = new System.Text.StringBuilder();
            notFoundContent.AppendLine("<!DOCTYPE html>");
            notFoundContent.AppendLine("<html>");
            notFoundContent.AppendLine("<head><title>404 Not Found</title></head>");
            notFoundContent.AppendLine("<body>");
            notFoundContent.AppendLine(navHtml);
            notFoundContent.AppendLine("<h1>404 - Page Not Found</h1>");
            notFoundContent.AppendLine("<p>The page you are looking for does not exist.</p>");
            notFoundContent.AppendLine("</body>");
            notFoundContent.AppendLine("</html>");

            string notFoundFileName = System.IO.Path.Combine(deployFolder, "404.html");
            try
            {
                System.IO.File.WriteAllText(notFoundFileName, notFoundContent.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file {notFoundFileName}: {ex.Message}");
            }
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                DependencyObject parent = textBox;
                while (parent != null && parent is not ListBoxItem)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is ListBoxItem listBoxItem)
                {
                    ListBox listBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem) as ListBox;

                    if (listBox != null)
                    {
                        // real action here
                        int index = listBox.ItemContainerGenerator.IndexFromContainer(listBoxItem);
                        if (index < pagesContent.Count)
                        {
                            pagesContent[index] = textBox.Text;
                        }
                        else
                        {
                            pagesContent.Add(textBox.Text);
                        }
                    }
                }
            }
        }
        private void TextBox_NameChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                DependencyObject parent = textBox;
                while (parent != null && parent is not ListBoxItem)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is ListBoxItem listBoxItem)
                {
                    ListBox listBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem) as ListBox;

                    if (listBox != null)
                    {
                        // real action here
                        int index = listBox.ItemContainerGenerator.IndexFromContainer(listBoxItem);
                        if (index < pagesName.Count)
                        {
                            pagesName[index] = textBox.Text;
                        }
                        else
                        {
                            pagesName.Add(textBox.Text);
                        }
                    }
                }
            }
        }

        private void clearSite()
        {
            string deployFolder = "site";
            try
            {
                if (System.IO.Directory.Exists(deployFolder))
                {
                    System.IO.Directory.Delete(deployFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting folder {deployFolder}: {ex.Message}");
            }
        }

        private void ClearItems(object sender, RoutedEventArgs e)
        {
            clearSite();
            pagesContent.Clear();
            pagesName.Clear();
            lb_pages.Items.Clear();
        }
    }
}
