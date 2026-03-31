using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KarmaBanking.App.Models;
using KarmaBanking.App.Utils;

namespace KarmaBanking.App.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private IssueCategory? selectedCategory;
        private string statusMessage = "Please select a category to continue.";
        private SelectedAttachment? selectedAttachment;

        public IssueCategory? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                if (selectedCategory != value)
                {
                    selectedCategory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanContinue));
                    ContinueCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public SelectedAttachment? SelectedAttachment
        {
            get => selectedAttachment;
            set
            {
                if (selectedAttachment != value)
                {
                    selectedAttachment = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasAttachmentPreview));
                }
            }
        }

        public bool HasAttachmentPreview => SelectedAttachment != null;

        public bool CanContinue => SelectedCategory != null;

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (statusMessage != value)
                {
                    statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand ContinueCommand { get; }
        public RelayCommand RemoveAttachmentCommand { get; }

        public event Action<string>? ContinueRequested;

        public ChatViewModel()
        {
            ContinueCommand = new RelayCommand(OnContinueAsync, () => CanContinue);
            RemoveAttachmentCommand = new RelayCommand(OnRemoveAttachmentAsync);
        }

        private Task OnContinueAsync()
        {
            if (SelectedCategory == null)
                return Task.CompletedTask;

            string categoryDisplayName = GetCategoryDisplayName(SelectedCategory.Value);
            StatusMessage = $"Selected category: {categoryDisplayName}";

            ContinueRequested?.Invoke(categoryDisplayName);

            return Task.CompletedTask;
        }

        private Task OnRemoveAttachmentAsync()
        {
            SelectedAttachment = null;
            StatusMessage = "Attachment removed.";
            return Task.CompletedTask;
        }

        private string GetCategoryDisplayName(IssueCategory category)
        {
            return category switch
            {
                IssueCategory.Account => "Account",
                IssueCategory.Cards => "Cards",
                IssueCategory.Transfers => "Transfers",
                IssueCategory.Loans => "Loans",
                IssueCategory.TechnicalIssue => "Technical Issue",
                IssueCategory.Other => "Other",
                _ => category.ToString()
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}