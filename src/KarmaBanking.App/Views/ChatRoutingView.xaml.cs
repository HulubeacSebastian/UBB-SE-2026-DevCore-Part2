using System;
using System.IO;
using System.Threading.Tasks;
using KarmaBanking.App.Services;
using KarmaBanking.App.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace KarmaBanking.App.Views
{
    public sealed partial class ChatRoutingView : Page
    {
        private const int NoRatingSelected = 0;
        private const int LowestRating = 1;
        private const int SecondRating = 2;
        private const int ThirdRating = 3;
        private const int FourthRating = 4;
        private const int HighestRating = 5;
        private const int RatingDialogSpacing = 12;
        private const int RatingButtonsSpacing = 8;
        private const int RatingTitleFontSize = 18;
        private const int AttachmentSizeLimitBytes = 10 * 1024 * 1024;
        private const int UploadDelayMilliseconds = 1000;
        private const int DefaultSessionId = 1;
        private const string NoRatingText = "No rating selected.";
        private const string SelectedRatingPrefix = "Selected rating: ";
        private const string OneStarLabel = "one star";
        private const string TwoStarsLabel = "two stars";
        private const string ThreeStarsLabel = "three stars";
        private const string FourStarsLabel = "four stars";
        private const string FiveStarsLabel = "five stars";
        private const string OneStarButtonText = "⭐ One";
        private const string TwoStarButtonText = "⭐ Two";
        private const string ThreeStarButtonText = "⭐ Three";
        private const string FourStarButtonText = "⭐ Four";
        private const string FiveStarButtonText = "⭐ Five";
        private const int FeedbackTextBoxHeight = 100;

        private readonly ChatViewModel viewModel = ChatViewModel.Instance;
        private int selectedRating = NoRatingSelected;

        public ChatRoutingView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SessionTitleTextBlock.Text = viewModel.CurrentSession == null
                ? "No active chat selected."
                : $"{viewModel.CurrentSession.Title} ({viewModel.CurrentSession.SessionModeLabel})";

            TranscriptTextBox.Text = viewModel.BuildCurrentTranscript();

            AttachmentInfoTextBlock.Text = viewModel.SelectedAttachment == null
                ? "No file attached."
                : $"Attached file: {viewModel.SelectedAttachment.FileName} ({viewModel.SelectedAttachment.FileSizeDisplay})";

            if (viewModel.CurrentSession != null && !string.IsNullOrWhiteSpace(viewModel.CurrentSession.TeamContactMessage))
            {
                TeamMessageTextBox.Text = viewModel.CurrentSession.TeamContactMessage;
            }
        }

        private async void SendToTeam_Click(object sender, RoutedEventArgs e)
        {
            bool wasSent = await viewModel.SendCurrentConversationToTeamAsync(TeamMessageTextBox.Text);

            if (!wasSent)
            {
                StatusText.Text = "The support request could not be sent.";
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            TranscriptTextBox.Text = viewModel.BuildCurrentTranscript();
            SessionTitleTextBlock.Text = viewModel.CurrentSession == null
                ? "No active chat selected."
                : $"{viewModel.CurrentSession.Title} ({viewModel.CurrentSession.SessionModeLabel})";

            StatusText.Text = "The full chat transcript, your note, and the selected attachment details were prepared for the support team.";
            StatusText.Foreground = new SolidColorBrush(Colors.Green);
        }

        private async void AttachFileButton_Click(object sender, RoutedEventArgs e)
        {
            await PickAttachmentAsync();
            AttachmentInfoTextBlock.Text = viewModel.SelectedAttachment == null
                ? "No file attached."
                : $"Attached file: {viewModel.SelectedAttachment.FileName} ({viewModel.SelectedAttachment.FileSizeDisplay})";
        }

        private async void OpenRatingDialog_Click(object sender, RoutedEventArgs e)
        {
            selectedRating = NoRatingSelected;

            StackPanel dialogContent = new StackPanel
            {
                Spacing = RatingDialogSpacing
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Rate your experience",
                FontSize = RatingTitleFontSize,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            TextBlock ratingLabel = new TextBlock
            {
                Text = "Please select a rating:"
            };

            StackPanel starsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = RatingButtonsSpacing
            };

            TextBlock selectedRatingText = new TextBlock
            {
                Text = NoRatingText
            };

            Button star1 = new Button { Content = OneStarButtonText, Tag = LowestRating };
            Button star2 = new Button { Content = TwoStarButtonText, Tag = SecondRating };
            Button star3 = new Button { Content = ThreeStarButtonText, Tag = ThirdRating };
            Button star4 = new Button { Content = FourStarButtonText, Tag = FourthRating };
            Button star5 = new Button { Content = FiveStarButtonText, Tag = HighestRating };

            star1.Click += (s, args) =>
            {
                selectedRating = LowestRating;
                selectedRatingText.Text = $"{SelectedRatingPrefix}{OneStarLabel}";
            };

            star2.Click += (s, args) =>
            {
                selectedRating = SecondRating;
                selectedRatingText.Text = $"{SelectedRatingPrefix}{TwoStarsLabel}";
            };

            star3.Click += (s, args) =>
            {
                selectedRating = ThirdRating;
                selectedRatingText.Text = $"{SelectedRatingPrefix}{ThreeStarsLabel}";
            };

            star4.Click += (s, args) =>
            {
                selectedRating = FourthRating;
                selectedRatingText.Text = $"{SelectedRatingPrefix}{FourStarsLabel}";
            };

            star5.Click += (s, args) =>
            {
                selectedRating = HighestRating;
                selectedRatingText.Text = $"{SelectedRatingPrefix}{FiveStarsLabel}";
            };

            starsPanel.Children.Add(star1);
            starsPanel.Children.Add(star2);
            starsPanel.Children.Add(star3);
            starsPanel.Children.Add(star4);
            starsPanel.Children.Add(star5);

            TextBlock feedbackLabel = new TextBlock
            {
                Text = "Leave feedback (optional):"
            };

            TextBox feedbackTextBox = new TextBox
            {
                PlaceholderText = "Write your feedback here...",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = FeedbackTextBoxHeight
            };

            dialogContent.Children.Add(titleText);
            dialogContent.Children.Add(ratingLabel);
            dialogContent.Children.Add(starsPanel);
            dialogContent.Children.Add(selectedRatingText);
            dialogContent.Children.Add(feedbackLabel);
            dialogContent.Children.Add(feedbackTextBox);

            ContentDialog ratingDialog = new ContentDialog
            {
                Title = "Post-Chat Rating",
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = dialogContent,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await ratingDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (selectedRating == NoRatingSelected)
                {
                    StatusText.Text = "Please select a rating before submitting.";
                    StatusText.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }

                try
                {
                    ApiService api = new ApiService();

                    int sessionId = viewModel.CurrentSession?.Id ?? DefaultSessionId;
                    string feedback = feedbackTextBox.Text;

                    api.SubmitFeedback(sessionId, selectedRating, feedback);

                    StatusText.Text = $"Thank you! Rating submitted: {selectedRating} ⭐";
                    StatusText.Foreground = new SolidColorBrush(Colors.Green);
                }
                catch
                {
                    StatusText.Text = "Feedback submission failed (database connection unavailable locally).";
                    StatusText.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async Task PickAttachmentAsync()
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();

                IntPtr hwnd = WindowNative.GetWindowHandle(App.MainAppWindow);
                InitializeWithWindow.Initialize(picker, hwnd);

                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".pdf");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");

                StorageFile? file = await picker.PickSingleFileAsync();

                if (file == null)
                {
                    return;
                }

                BasicProperties properties = await file.GetBasicPropertiesAsync();

                if (properties.Size > AttachmentSizeLimitBytes)
                {
                    viewModel.StatusMessage = "File size must be ten megabytes or less.";
                    viewModel.SetUploadFailed("File size must be ten megabytes or less.");
                    return;
                }

                viewModel.SelectedAttachment = new Models.SelectedAttachment
                {
                    FileName = file.Name,
                    FilePath = file.Path,
                    FileType = Path.GetExtension(file.Name).ToLowerInvariant(),
                    FileSizeBytes = (long)properties.Size
                };

                viewModel.StatusMessage = "Attachment selected successfully.";
                viewModel.SetAttachmentSelected();

                viewModel.SetUploadStarted();
                await Task.Delay(UploadDelayMilliseconds);
                viewModel.SetUploadSucceeded();
            }
            catch (Exception ex)
            {
                viewModel.StatusMessage = $"Attachment selection failed: {ex.Message}";
                viewModel.SetUploadFailed(ex.Message);
            }
        }
    }
}