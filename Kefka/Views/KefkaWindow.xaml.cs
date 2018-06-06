using ff14bot;
using Kefka.Models;
using Kefka.Utilities;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using TrelloNet;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Kefka.Views
{
    public partial class KefkaWindow
    {
        public KefkaWindow()
        {
            InitializeComponent();
            SelectTheme();

            Kefka.windowInitialized = true;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #region Theme Switch

        private void SelectTheme()
        {
            switch (MainSettingsModel.Instance.Theme)
            {
                case SelectedTheme.Blue:
                    ChangeThemeColor("Blue");
                    break;

                case SelectedTheme.Pink:
                    ChangeThemeColor("Pink");
                    break;

                case SelectedTheme.Green:
                    ChangeThemeColor("Green");
                    break;

                case SelectedTheme.Red:
                    ChangeThemeColor("Red");
                    break;

                case SelectedTheme.Yellow:
                    ChangeThemeColor("Yellow");
                    break;

                case SelectedTheme.Purple:
                    ChangeThemeColor("Purple");
                    break;

                case SelectedTheme.Orange:
                    ChangeThemeColor("Orange");
                    break;

                case SelectedTheme.Lime:
                    ChangeThemeColor("Lime");
                    break;

                case SelectedTheme.Emerald:
                    ChangeThemeColor("Emerald");
                    break;

                case SelectedTheme.Teal:
                    ChangeThemeColor("Teal");
                    break;

                case SelectedTheme.Cyan:
                    ChangeThemeColor("Cyan");
                    break;

                case SelectedTheme.Cobalt:
                    ChangeThemeColor("Cobalt");
                    break;

                case SelectedTheme.Indigo:
                    ChangeThemeColor("Indigo");
                    break;

                case SelectedTheme.Violet:
                    ChangeThemeColor("Violet");
                    break;

                case SelectedTheme.Magenta:
                    ChangeThemeColor("Magenta");
                    break;

                case SelectedTheme.Crimson:
                    ChangeThemeColor("Crimson");
                    break;

                case SelectedTheme.Amber:
                    ChangeThemeColor("Amber");
                    break;

                case SelectedTheme.Brown:
                    ChangeThemeColor("Brown");
                    break;

                case SelectedTheme.Olive:
                    ChangeThemeColor("Olive");
                    break;

                case SelectedTheme.Steel:
                    ChangeThemeColor("Steel");
                    break;

                case SelectedTheme.Mauve:
                    ChangeThemeColor("Mauve");
                    break;

                case SelectedTheme.Taupe:
                    ChangeThemeColor("Taupe");
                    break;

                case SelectedTheme.Sienna:
                    ChangeThemeColor("Sienna");
                    break;

                default:
                    ChangeThemeColor("Steel");
                    break;
            }
        }

        private void ChangeThemeColor(string color)
        {
            try
            {
                if (Resources.MergedDictionaries.Count > 0)
                {
                    Resources.MergedDictionaries.Clear();
                }

                AddResourceDictionary("/KefkaUI.Metro;component/Styles/Controls.xaml");
                AddResourceDictionary("/KefkaUI.Metro;component/Styles/Fonts.xaml");
                AddResourceDictionary("/KefkaUI.Metro;component/Styles/Colors.xaml");
                AddResourceDictionary("/KefkaUI.Metro.IconPacks;component/Themes/IconPacks.xaml");
                AddResourceDictionary($"/KefkaUI.Metro;component/Styles/Accents/{color}.xaml");
                AddResourceDictionary("/KefkaUI.Metro;component/Styles/Accents/BaseDark.xaml");
            }
            catch (Exception e)
            {
                Logger.KefkaLog(e.ToString());
                throw;
            }
        }

        private void AddResourceDictionary(string source)
        {
            var resourceDictionary = Application.LoadComponent(new Uri(source, UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        #endregion Theme Switch

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            InterruptManager.ResetInterrupts();
            TankBusterManager.ResetTankBusters();
            FormManager.SaveFormInstances();
            Hide();
        }

        private void CmbSwitchTheme(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            SelectTheme();
        }

        #region Export Settings Region

        private void TrelloLink(object sender, RoutedEventArgs e)
        {
            Process.Start("https://trello.com/b/KrJDSjp8");
        }

        private void Trello(object sender, RoutedEventArgs routedEventArgs)
        {
            BeginAgain:
            var priorAuth = false;

            var routineName = MainSettingsModel.Instance.CurrentRoutine;
            var discordUsername = Interaction.InputBox("Please enter your Discord Username. (So I can ping you in the Discord)");

            if (discordUsername.Any(char.IsWhiteSpace))
                discordUsername = Regex.Replace(discordUsername, @"\s+", "");
            if (discordUsername == "")
            {
                var result = MessageBox.Show("You didn't enter anything for the Trello Key. Abandon Report?", "Trello Auth Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return;

                    case MessageBoxResult.Yes:
                        return;

                    case MessageBoxResult.No:
                        goto BeginAgain;
                }
            }
            var initPath = Path.Combine(Environment.CurrentDirectory, @"Logs");

            var rbLogLocation = new OpenFileDialog
            {
                Title = @"Please select the log file(s) where the issue exists. Usually \RB\Logs",
                Multiselect = true,
                InitialDirectory = initPath
            };

            ITrello trello = new Trello("b09ce954a206f0165506513795959840");

            if (MainSettingsModel.Instance.TrelloToken == "" || MainSettingsModel.Instance.TrelloTokenData == null || DateTime.Now.Date > MainSettingsModel.Instance.TrelloTokenData.DateCreated.Date + TimeSpan.FromDays(29))
            {
                var url = trello.GetAuthorizationUrl("Kefka", Scope.ReadWrite, Expiration.ThirtyDays);
                Process.Start(url.ToString());

                BeginTrelloAuth:
                var token = Interaction.InputBox("Please enter the Trello Access Token you recieved.");
                if (token.Any(char.IsWhiteSpace))
                    token = Regex.Replace(token, @"\s+", "");
                if (token == "")
                {
                    var result = MessageBox.Show("You didn't enter anything for the Trello Key. Abandon Report?", "Trello Auth Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                            return;

                        case MessageBoxResult.Yes:
                            return;

                        case MessageBoxResult.No:
                            goto BeginTrelloAuth;
                    }
                }

                var tokenData = trello.Tokens.WithToken(token);
                MainSettingsModel.Instance.TrelloToken = token;
                MainSettingsModel.Instance.TrelloTokenData = tokenData;
                FormManager.SaveFormInstances();

                trello.Authorize(token);
                Process.Start("https://trello.com/invite/b/KrJDSjp8/0e335c0fad7fdea3708ee73d291e35c7/kefka");
                MessageBox.Show("Please open your web browser and authorize your account, then click Ok. If you do not authorize, you will have errors reporting issues.", "Please authorize your account.", MessageBoxButton.OK);
                MessageBox.Show("Thank you for authorizing your account!", "Thanks!", MessageBoxButton.OK);
                goto SkipRegularTokenCheck;
            }

            if (MainSettingsModel.Instance.TrelloToken != "")
                trello.Authorize(MainSettingsModel.Instance.TrelloToken);
            priorAuth = true;

            SkipRegularTokenCheck:

            var bugReportList = new ListId("584020ad8f3cb7d794f3dbcd");

            try
            {
                var newBugCard = trello.Cards.Add(new NewCard("New Bug Report", bugReportList));

                CardName:
                newBugCard.Name = Interaction.InputBox("What was the general Bug type? Include the rotation name! (Cyan: Not using Hakaze, Exception, Crash, ect)");
                if (newBugCard.Name == "")
                {
                    var tryAgain = MessageBox.Show("You didn't enter anything. Abandon Report?", "Trello Name Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (tryAgain)
                    {
                        case MessageBoxResult.Cancel:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.Yes:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.No:
                            goto CardName;
                    }
                }

                CardDescription:
                newBugCard.Desc = Interaction.InputBox("Please describe the Bug in as much detail as possible.");
                if (newBugCard.Desc == "")
                {
                    var tryAgain = MessageBox.Show("You didn't enter anything. Abandon Report?", "Trello Description Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (tryAgain)
                    {
                        case MessageBoxResult.Cancel:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.Yes:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.No:
                            goto CardDescription;
                    }
                }

                trello.Cards.Update(newBugCard);

                try
                {
                    var file = new StreamWriter("Bug Report Description");
                    file.WriteLine(newBugCard.Desc);
                    file.Close();

                    rbLogLocation.ShowDialog();
                    string[] myFile;
                    if (!string.IsNullOrEmpty(rbLogLocation.FileName))
                        myFile = rbLogLocation.FileNames;
                    else goto cleanup;

                    using (var archive = ZipFile.Open($"{routineName} Bug Report.zip", ZipArchiveMode.Create))
                    {
                        foreach (var rbLog in myFile)
                        {
                            var filename = rbLog;
                            archive.CreateEntryFromFile(rbLog, Path.GetFileName(filename));
                        }
                        if (File.Exists("Bug Report Description"))
                            archive.CreateEntryFromFile("Bug Report Description", "Bug Report Description.txt");
                        if (File.Exists(@"Settings/" + Core.Me.Name + "/Kefka/Main_Settings.json"))
                            archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + "/Kefka/Main_Settings.json", "Main_Settings.json");
                        if (File.Exists(@"Settings/" + Core.Me.Name + $"/Kefka/Routine Settings/{routineName}/{routineName}_Settings.json"))
                            archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + $"/Kefka/Routine Settings/{routineName}/{routineName}_Settings.json", $"{routineName}_Settings.json");
                        if (File.Exists(@"Settings/" + Core.Me.Name + $"/Kefka/Hotkeys/{routineName}_Hotkeys.json"))
                            archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + $"/Kefka/Hotkeys/{routineName}_Hotkeys.json", $"{routineName}_Hotkeys.json");
                        if (File.Exists(@"Settings/" + Core.Me.Name + $"/Kefka/Openers/{routineName}_Opener.json"))
                            archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + $"/Kefka/Openers/{routineName}_Opener.json", $"{routineName}_Opener.json");
                    }
                }
                catch (Exception e)
                {
                    var errorWindow = MessageBox.Show(e.ToString(), "There was an error, pleasee read the error in the RB Log and try again.", MessageBoxButton.OK);
                    Logger.KefkaLog(e.ToString());

                    if (errorWindow == MessageBoxResult.OK || errorWindow == MessageBoxResult.Cancel)
                    {
                        goto CardName;
                    }
                }

                trello.Cards.AddAttachment(newBugCard, new FileAttachment($"{routineName} Bug Report.zip", ($"{routineName} Bug Report")));

                var omninewb = trello.Members.WithId("58402025bbbfb2e3afe186fe");
                var fare = trello.Members.WithId("5927059480eccdcdf2bda9ec");

                //switch (MainSettingsModel.Instance.CurrentRoutine)
                //{
                //    case "Barret":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Beatrix":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Cecil":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Cyan":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Edward":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Eiko":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Elayne":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Freya":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Mikoto":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Paine":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Remiel":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Sabin":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Shadow":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;

                //    case "Surito":
                //        trello.Cards.AddMember(newBugCard, fare);
                //        break;

                //    case "Vivi":
                //        trello.Cards.AddMember(newBugCard, omninewb);
                //        break;
                //}

                trello.Cards.AddMember(newBugCard, omninewb);
                trello.Cards.AddMember(newBugCard, fare);
                trello.Cards.AddMember(newBugCard, trello.Members.Me());
                trello.Cards.AddComment(newBugCard, $"Discord Username: {discordUsername}");
            }
            catch (Exception e)
            {
                var errorWindow = MessageBox.Show(e + "\n\n Chances are, you either closed the authorization window before you actually authorized Kefka to post, or your Trello token data is just... bad. :D. Try to re-submit the error report.", "There was an error", MessageBoxButton.OK);
                Logger.KefkaLog(e.ToString());
                Logger.KefkaLog("Chances are, you either closed the authorization window before you actually authorized Kefka to post, or your Trello token data is just... bad. :D. Try to re-submit the error report.");
                Core.OverlayManager.AddToast(() => "Error: Re-Submit Error Report.", TimeSpan.FromMilliseconds(3000), MainSettingsModel.Instance.ToastColor(true), Colors.White, new FontFamily("High Tower Text Italic"), new FontWeight(), 52);
                MainSettingsModel.Instance.TrelloToken = "";
                MainSettingsModel.Instance.TrelloTokenData = null;
            }

            cleanup:
            if (File.Exists("Bug Report Description"))
                File.Delete("Bug Report Description");
            if (File.Exists($"{routineName} Bug Report.zip"))
                File.Delete($"{routineName} Bug Report.zip");
            if (File.Exists($"{routineName} Bug Report"))
                File.Delete($"{routineName} Bug Report");

            if (priorAuth)
                Process.Start("https://trello.com/b/KrJDSjp8");
        }

        #endregion Export Settings Region
    }
}