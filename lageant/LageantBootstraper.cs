using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using lageant.Models;
using lageant.Utils;
using lageant.ViewModels;

namespace lageant
{
    public class LageantBootstraper : BootstrapperBase
    {
        private CompositionContainer _container;
        private IEventAggregator _events;
        private ApplicationInstanceMonitor<ActivationDataMessage> _instanceMonitor;

        public LageantBootstraper()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            if (_instanceMonitor.Assert())
            {
                // This is the only instance.
                base.OnStartup(sender, e);
                DisplayRootViewFor<MainViewModel>();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.FullName.StartsWith("ManagedInjector"))
                Application.Shutdown(0);
        }

        protected override void Configure()
        {
            try
            {
                _instanceMonitor = new ApplicationInstanceMonitor<ActivationDataMessage>();
                _events = new EventAggregator();
                _container =
                    new CompositionContainer(
                        new AggregateCatalog(
                            // ReSharper disable once RedundantEnumerableCastCall
                            AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
                var batch = new CompositionBatch();
                batch.AddExportedValue<IWindowManager>(new AppWindowManager());
                batch.AddExportedValue(_events);
                batch.AddExportedValue(_container);
                _container.Compose(batch);
            }
            catch (Exception)
            {
            }
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            try
            {
                var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
                var exports = _container.GetExportedValues<object>(contract);
                var enumerable = exports as IList<object> ?? exports.ToList();
                if (enumerable.Any())
                {
                    return enumerable.First();
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}