﻿﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Common.Models;
using Xamarin.Forms;
using Splat;
using Sandbox.Services;
using Xamvvm;
using System.Linq;
using Common.Output;
using Common.Enums;
using System.IO;
using System.Text;
using ReactiveUI.Fody.Helpers;

namespace Sandbox.ViewModels
{
    public class MainPageViewModel : BasePageViewModel
    {
        // Properties
        [Reactive]
        public MeModel Info { get; set; }

        [Reactive]
        public ImageSource Image { get; set;  }

        // Commands
        public ReactiveCommand<Unit, bool> Navigate { get; }
        public ReactiveCommand<bool, Result<MeModel>> GetUserData { get; }
        public ReactiveCommand<MeModel, Result<byte[]>> GetUserPhoto { get; }

        public MainPageViewModel()
        {
            // Services
            var service = Locator.Current.GetService<IUserService>();

            GetUserData = ReactiveCommand.CreateFromTask<bool, Result<MeModel>>(async _ =>
            {
                return (await service.GetUserInfo());
            });

            GetUserPhoto = ReactiveCommand.CreateFromTask<MeModel, Result<byte[]>>(async _ =>
			{
                return (await service.GetUserPhoto());
			});

            // Binding
            InitRxBindings();
        }

        protected override void InitRxBindings()
        {
			// Subscriptions
			GetSignOn
				.Throttle(TimeSpan.FromSeconds(.5), TaskPoolScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(GetUserData)
                .DisposeWith(SubscriptionDisposables);

            GetUserData
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(result =>
                {
                    if (result.Type != ResultType.Ok)
                        throw new Exception(result.Type.ToString());

                    Info = result.Data;
                })
                .DisposeWith(SubscriptionDisposables);

            GetUserPhoto
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(result =>
                {
                    Image = ImageSource.FromStream(() => new MemoryStream(result.Data));
                })
                .DisposeWith(SubscriptionDisposables);

			this.WhenAnyValue(vm => vm.Info)
				.Throttle(TimeSpan.FromSeconds(.5), TaskPoolScheduler.Default)
                .Where(info => info != null)
                .InvokeCommand(GetUserPhoto)
                .DisposeWith(SubscriptionDisposables);


			// Behaviors
            Observable.Merge(GetSignOn.IsExecuting, GetUserData.IsExecuting, GetUserPhoto.IsExecuting)
				.ToProperty(this, vm => vm.IsBusy, out _busy);

            // Exceptions
            Observable.Merge(GetSignOn.ThrownExceptions, GetUserData.ThrownExceptions, GetUserPhoto.ThrownExceptions)
                .Subscribe(async ex =>
                {
                    Debug.WriteLine($"[{ex.Source}] Error = {ex.Message}");

                    var result = await Interactions.Errors.Handle(ex);

                    if (result == ErrorRecoveryOption.Retry)
                    {
                        // TODO: do something to retry
                    }
                })
                .DisposeWith(SubscriptionDisposables);
        }
    }
}
