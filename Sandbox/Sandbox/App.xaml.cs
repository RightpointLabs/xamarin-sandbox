﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using Sandbox.ViewModels;
using Xamarin.Forms;
using Xamvvm;

#if !DEBUG
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
#endif
namespace Sandbox
{
	public static class Interactions
	{
		public static readonly Interaction<Exception, ErrorRecoveryOption> Errors = new Interaction<Exception, ErrorRecoveryOption>();
	}

	public enum ErrorRecoveryOption
	{
		Cancel,
		Retry,
	}

	public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            InitializeServices();

			var factory = new XamvvmFormsRxUIFactory(this);
			XamvvmCore.SetCurrentFactory(factory);

			factory.RegisterNavigationPage<AppShellNavigationPageModel>(() => this.GetPageFromCache<MainPageViewModel>());


            MainPage = this.GetPageAsNewInstance<AppShellNavigationPageModel>() as Page;
		}

        private void InitializeServices()
        {
			// Locator.CurrentMutable.RegisterLazySingleton(() => new SomeService(), typeof(ISomeService));
		}

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}