using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace SDATweb
{
    public sealed partial class MainWindow : Window
    {
        private const string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
        private WebsiteDataModel websiteDataModel = new WebsiteDataModel();

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void NewPage(object sender, RoutedEventArgs e)
        {
            lb_pages.Items.Add(1);
            string contentOfThePage = """
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
                """;
            string nameOfThePage = "Sample Page";
            websiteDataModel.AddNewPage(contentOfThePage, nameOfThePage);
        }

        private void SendRequest(object sender, RoutedEventArgs e)
        {
            Button sendButton = (Button)sender;

            if (sendButton != null)
            {
                StackPanel parentPanel = (StackPanel)sendButton.Parent;

                if (parentPanel != null)
                {
                    TextBox smallerTextBox = (TextBox)parentPanel.Children[0];
                    StackPanel outerPanel = (StackPanel)parentPanel.Parent;
                    TextBox largerTextBox = (TextBox)outerPanel.Children[2];

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
                    htmlBox.Text = "Received response";
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
            copyAssets();

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
            for (int i = 0; i < websiteDataModel.PagesContent.Count; i++)
            {
                navLinks.AppendLine($"<a href='{websiteDataModel.PagesName[i].Replace(" ", "")}{i}.html'>{websiteDataModel.PagesName[i]}</a>");
            }
            navLinks.AppendLine("</nav>");
            string navHtml = navLinks.ToString();

            // write each page with the navigation menu injected
            for (int i = 0; i < websiteDataModel.PagesContent.Count; i++)
            {
                string pageHtml = websiteDataModel.PagesContent[i];

                // Look for the <head> to insert favicon
                int headIndex = pageHtml.IndexOf("<head>", StringComparison.OrdinalIgnoreCase);
                if (headIndex >= 0)
                {
                    headIndex += "<head>".Length;
                    pageHtml = pageHtml.Insert(headIndex, "<link rel='icon' type='image/png' href='assets/icon.png'>");
                }
                else
                {
                    // If no <head> tag was found, append the favicon at the end
                    pageHtml += "<link rel='icon' type='image/png' href='assets/icon.png'>";
                }

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

                string pageFileName = System.IO.Path.Combine(deployFolder, $"{websiteDataModel.PagesName[i].Replace(" ", "")}{i}.html");
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
            if (indexToggle.IsChecked == false || websiteDataModel.PagesContent.Count == 0)
            {
                indexContent.AppendLine("<!DOCTYPE html>");
                indexContent.AppendLine("<html>");
                indexContent.AppendLine($"<head><title>{nameBox.Text} Home</title><link rel='icon' type='image/png' href='assets/icon.png'></head>");
                indexContent.AppendLine("<body>");
                indexContent.AppendLine(navHtml);
                indexContent.AppendLine($"<h1>Welcome to the {nameBox.Text} Home Page</h1>");
                indexContent.AppendLine("<ul>");
                for (int i = 0; i < websiteDataModel.PagesContent.Count; i++)
                {
                    indexContent.AppendLine($"<li><a href='{websiteDataModel.PagesName[i].Replace(" ", "")}{i}.html'>{websiteDataModel.PagesName[i]}</a></li>");
                }
                indexContent.AppendLine("</ul>");
                indexContent.AppendLine("</body>");
                indexContent.AppendLine("</html>");
            }
            else
            {
                indexContent.AppendLine("<!DOCTYPE html>");
                indexContent.AppendLine("<html>");
                indexContent.AppendLine($"<head><title>{nameBox.Text} Home</title><link rel='icon' type='image/png' href='assets/icon.png'><meta http-equiv='refresh' content=\"0; url='{websiteDataModel.PagesName[0].Replace(" ", "")}0.html'\" /></head>");
                indexContent.AppendLine("<body>");
                indexContent.AppendLine(navHtml);
                indexContent.AppendLine($"<h1>Welcome to the {nameBox.Text} Home Page</h1>");
                indexContent.AppendLine("</body>");
                indexContent.AppendLine("</html>");
            }

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
            notFoundContent.AppendLine("<head><title>404 Not Found</title><link rel='icon' type='image/png' href='assets/icon.png'></head>");
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

        private void copyAssets() 
        {
            string deployFolder = "site";
            string assetsFolder = "assets";
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(deployFolder, assetsFolder));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder {deployFolder}/{assetsFolder}: {ex.Message}");
                return;
            }
            foreach (var asset in websiteDataModel.Assets)
            {
                string destPath = System.IO.Path.Combine(deployFolder, assetsFolder, asset.Name);
                try
                {
                    File.Copy(asset.Path, destPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {asset.Name}: {ex.Message}");
                }
            }
            // copy icon
            try
            {
                File.Copy(iconBox.Text, System.IO.Path.Combine(deployFolder, assetsFolder, "icon.png"), true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file {iconBox.Text}: {ex.Message}");
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
                    ListBox listBox = (ListBox)ItemsControl.ItemsControlFromItemContainer(listBoxItem);

                    if (listBox != null)
                    {
                        // real action here
                        int index = listBox.ItemContainerGenerator.IndexFromContainer(listBoxItem);
                        if (index < websiteDataModel.PagesContent.Count)
                        {
                            websiteDataModel.PagesContent[index] = textBox.Text;
                        }
                        else
                        {
                            websiteDataModel.PagesContent.Add(textBox.Text);
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
                    ListBox listBox = (ListBox)ItemsControl.ItemsControlFromItemContainer(listBoxItem);

                    if (listBox != null)
                    {
                        // real action here
                        int index = listBox.ItemContainerGenerator.IndexFromContainer(listBoxItem);
                        if (index < websiteDataModel.PagesName.Count)
                        {
                            websiteDataModel.PagesName[index] = textBox.Text;
                        }
                        else
                        {
                            websiteDataModel.PagesName.Add(textBox.Text);
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
            websiteDataModel.PagesContent.Clear();
            websiteDataModel.PagesName.Clear();
            lb_pages.Items.Clear();
        }

        private void OpenDirectory(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"c:\windows\explorer.exe";
            psi.Arguments = "site";
            Process.Start(psi);
        }

        private void OpenIndex(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = edgePath;
            psi.Arguments = System.Environment.CurrentDirectory + "/site/index.html";
            Process.Start(psi);
        }

        private async void SelectIcon(object sender, RoutedEventArgs e)
        {
            StorageFile file = await selectFile([".png"]);
            if (file != null)
            {
                iconBox.Text=file.Path;
            }
        }

        private void ClearAssets(object sender, RoutedEventArgs e)
        {
            lb_assets.Items.Clear();
            websiteDataModel.Assets.Clear();
        }

        private async void AddAsset(object sender, RoutedEventArgs e)
        {
            StorageFile file = await selectFile(["*"]);
            if (file != null)
            {
                lb_assets.Items.Add(file.Name);
                websiteDataModel.Assets.Add(file);
            }
        }

        private async Task<StorageFile> selectFile(List<string> filterFormats)
        {
            var picker = new FileOpenPicker();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            foreach (string format in filterFormats)
            {
                picker.FileTypeFilter.Add(format);
            }

            StorageFile file = await picker.PickSingleFileAsync();
            return file;
        }
    }
}
