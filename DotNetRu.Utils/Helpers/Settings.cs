using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotNetRu.Utils.Interfaces;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

using Xamarin.Forms;

namespace XamarinEvolve.Clients.Portable
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        static Settings settings;

        /// <summary>
        /// Gets or sets the current settings. This should always be used
        /// </summary>
        /// <value>The current.</value>
        public static Settings Current
        {
            get
            {
                return settings ?? (settings = new Settings());
            }
        }

        private IPlatformSpecificSettings _platformSettings;

        public Settings()
        {
            _platformSettings = DependencyService.Get<IPlatformSpecificSettings>();
        }

        public void SaveReminderId(string id, string calId)
        {
            // TODO
        }


        string GetReminderId(string id)
        {
            return "reminder_" + id;
        }

        public string GetEventId(string id)
        {
            return AppSettings.GetValueOrDefault(GetReminderId(id), string.Empty);
        }

        public void RemoveReminderId(string id)
        {
            AppSettings.Remove(GetReminderId(id));
        }

        public void LeaveConferenceFeedback()
        {
            AppSettings.AddOrUpdateValue("conferencefeedback_finished", true);
        }

        public bool IsConferenceFeedbackFinished()
        {
            return AppSettings.GetValueOrDefault("conferencefeedback_finished", false);
        }

        const string HasSetReminderKey = "set_a_reminder";

        static readonly bool HasSetReminderDefault = false;

        public bool HasSetReminder
        {
            get
            {
                return AppSettings.GetValueOrDefault(HasSetReminderKey, HasSetReminderDefault);
            }

            set
            {
                AppSettings.AddOrUpdateValue(HasSetReminderKey, value);
            }
        }

        const string EventCalendarKey = "event_calendar";

        static readonly string EventCalendarIdDefault = string.Empty;

        public string EventCalendarId
        {
            get
            {
                return AppSettings.GetValueOrDefault(EventCalendarKey, EventCalendarIdDefault);
            }

            set
            {
                AppSettings.AddOrUpdateValue(EventCalendarKey, value);
            }
        }


        const string PushNotificationsEnabledKey = "push_enabled";

        static readonly bool PushNotificationsEnabledDefault = false;

        public bool PushNotificationsEnabled
        {
            get
            {
                return AppSettings.GetValueOrDefault(PushNotificationsEnabledKey, PushNotificationsEnabledDefault);
            }

            set
            {
                if (AppSettings.AddOrUpdateValue(PushNotificationsEnabledKey, value)) OnPropertyChanged();
            }
        }

        const string FirstRunKey = "first_run";

        static readonly bool FirstRunDefault = true;

        public bool FirstRun
        {
            get
            {
                return AppSettings.GetValueOrDefault(FirstRunKey, FirstRunDefault);
            }

            set
            {
                if (AppSettings.AddOrUpdateValue(FirstRunKey, value)) OnPropertyChanged();
            }
        }

        const string ShowAllCategoriesKey = "all_categories";

        static readonly bool ShowAllCategoriesDefault = true;

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to show all categories.
        /// </summary>
        /// <value><c>true</c> if show all categories; otherwise, <c>false</c>.</value>
        public bool ShowAllCategories
        {
            get
            {
                return AppSettings.GetValueOrDefault(ShowAllCategoriesKey, ShowAllCategoriesDefault);
            }

            set
            {
                if (AppSettings.AddOrUpdateValue(ShowAllCategoriesKey, value)) OnPropertyChanged();
            }
        }

        const string FilteredCategoriesKey = "filtered_categories";

        static readonly string FilteredCategoriesDefault = string.Empty;


        public string FilteredCategories
        {
            get
            {
                return AppSettings.GetValueOrDefault(FilteredCategoriesKey, FilteredCategoriesDefault);
            }

            set
            {
                if (AppSettings.AddOrUpdateValue(FilteredCategoriesKey, value)) OnPropertyChanged();
            }
        }

        const string DatabaseIdKey = "azure_database";

        static readonly int DatabaseIdDefault = 0;

        public static int DatabaseId
        {
            get
            {
                return AppSettings.GetValueOrDefault(DatabaseIdKey, DatabaseIdDefault);
            }

            set
            {
                AppSettings.AddOrUpdateValue(DatabaseIdKey, value);
            }
        }

        public static int UpdateDatabaseId()
        {
            return DatabaseId++;
        }

        bool isConnected;

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                if (isConnected == value) return;
                isConnected = value;
                OnPropertyChanged();
            }
        }

        #region Helpers

        public bool HasFilters => !string.IsNullOrWhiteSpace(FilteredCategories) && !ShowAllCategories;

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}