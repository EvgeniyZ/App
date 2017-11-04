﻿namespace XamarinEvolve.Clients.Portable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Xml.Serialization;

    using DotNetRu.DataStore.Audit.Entities;
    using DotNetRu.DataStore.Audit.Extensions;
    using DotNetRu.DataStore.Audit.Models;

    using FormsToolkit;

    using MvvmHelpers;

    using Xamarin.Forms;

    using XamarinEvolve.Utils;
    using XamarinEvolve.Utils.Helpers;

    public class MeetupViewModel : ViewModelBase
    {
        public MeetupViewModel(INavigation navigation, MeetupModel meetupModel = null)
            : base(navigation)
        {
            this.MeetupModel = meetupModel;
            this.NextForceRefresh = DateTime.UtcNow.AddMinutes(45);
        }

        public ObservableRangeCollection<TalkModel> Sessions { get; } = new ObservableRangeCollection<TalkModel>();        

        public ObservableRangeCollection<Grouping<string, TalkModel>> SessionsGrouped { get; } =
            new ObservableRangeCollection<Grouping<string, TalkModel>>();

        public DateTime NextForceRefresh { get; set; }

        public MeetupModel MeetupModel { get; set; }

        #region Properties

        private TalkModel selectedTalkModel;

        public TalkModel SelectedTalkModel
        {
            get => this.selectedTalkModel;

            set
            {
                this.selectedTalkModel = value;
                this.OnPropertyChanged();
                if (this.selectedTalkModel == null)
                {
                    return;
                }

                MessagingService.Current.SendMessage(MessageKeys.NavigateToSession, this.selectedTalkModel);

                this.SelectedTalkModel = null;
            }
        }

        #endregion

        private bool noSessionsFound;
        private string noSessionsFoundMessage;

        public bool NoSessionsFound
        {
            get => this.noSessionsFound;

            set => this.SetProperty(ref this.noSessionsFound, value);
        }

        public string NoSessionsFoundMessage
        {
            get => this.noSessionsFoundMessage;

            set => this.SetProperty(ref this.noSessionsFoundMessage, value);
        }

        #region Commands

        private ICommand forceRefreshCommand;

        public ICommand ForceRefreshCommand => this.forceRefreshCommand
                                               ?? (this.forceRefreshCommand = new Command(
                                                       this.ExecuteForceRefreshCommandAsync));

        public void ExecuteForceRefreshCommandAsync()
        {
            this.ExecuteLoadSessionsAsync();
        }

        private ICommand loadSessionsCommand;

        public ICommand LoadSessionsCommand => this.loadSessionsCommand
                                               ?? (this.loadSessionsCommand = new Command<bool>(
                                                       (f) => this.ExecuteLoadSessionsAsync()));

        private IEnumerable<TalkModel> GetSessions()
        {
            var assembly = Assembly.Load(new AssemblyName("DotNetRu.DataStore.Audit"));
            var stream = assembly.GetManifestResourceStream("DotNetRu.DataStore.Audit.Storage.talks.xml");
            IEnumerable<TalkEntity> sessions;
            using (var reader = new StreamReader(stream))
            {
                var serializer = new XmlSerializer(typeof(List<TalkEntity>), new XmlRootAttribute("Talks"));
                var deserialized = (List<TalkEntity>)serializer.Deserialize(reader);
                
                sessions = deserialized.Where(
                    t => this.MeetupModel.EventTalksIds.Any(t1 => t1 == t.Id));
            }

            return sessions.Select(x => x.ToModel());
        }

        private void ExecuteLoadSessionsAsync()
        {
            if (this.IsBusy)
            {
                return;
            }

            try
            {
                this.NextForceRefresh = DateTime.UtcNow.AddMinutes(45);
                this.IsBusy = true;
                this.NoSessionsFound = false;

                var sessions = this.GetSessions();
                this.Sessions.ReplaceRange(sessions);

                if (!sessions.Any())
                {
                    this.NoSessionsFoundMessage = "No Sessions Found";
                    this.NoSessionsFound = true;
                }
                else
                {
                    this.NoSessionsFound = false;
                }

                var day = this.MeetupModel.GetDate();
                this.SessionsGrouped.ReplaceRange(new[] { new Grouping<string, TalkModel>(day, sessions) });

                if (Device.RuntimePlatform != Device.UWP && FeatureFlags.AppLinksEnabled)
                {
                    foreach (var session in this.Sessions)
                    {
                        try
                        {
                            // TODO uncomment

                            // data migration: older applinks are removed so the index is rebuilt again
                            // Application.Current.AppLinks.DeregisterLink(
                            // new Uri(
                            // $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.SessionsSiteSubdirectory}/{session.Id}"));

                            // Application.Current.AppLinks.RegisterLink(session.GetAppLink());
                        }
                        catch (Exception applinkException)
                        {
                            // don't crash the app
                            this.Logger.Report(applinkException, "AppLinks.RegisterLink", session.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Report(ex, "Method", "ExecuteLoadSessionsAsync");
                MessagingService.Current.SendMessage(MessageKeys.Error, ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        #endregion
    }
}