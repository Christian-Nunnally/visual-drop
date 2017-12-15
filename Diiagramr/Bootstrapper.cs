using Stylet;
using StyletIoC;
using StyletIoC.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Diiagramr.PluginNodeApi;
using Diiagramr.Service;
using Diiagramr.Service.Interfaces;
using Diiagramr.View.CustomControls;
using Diiagramr.ViewModel;

namespace Diiagramr
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            GetPluginAssemblies().ForEach(builder.Assemblies.Add);
            builder.Bind<IDirectoryService>().To<DirectoryService>();
            builder.Bind<IProjectLoadSave>().To<ProjectLoadSave>();
            builder.Bind<IProjectFileService>().To<ProjectFileService>().InSingletonScope();
            builder.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
            builder.Bind<IProvideNodes>().To<NodeProvider>().InSingletonScope();
            builder.Bind<IFileDialog>().To<OpenFileDialog>().WithKey("open");
            builder.Bind<IFileDialog>().To<SaveFileDialog>().WithKey("save");
            builder.Bind<PluginNode>().ToAllImplementations();
            ConfigurePluginNodesIntoIoC(builder);
        }

        private static void ConfigurePluginNodesIntoIoC(IStyletIoCBuilder builder)
        {
            foreach (var pluginAssembly in GetPluginAssemblies())
                foreach (var exportedType in pluginAssembly.ExportedTypes)
                    if (exportedType.Implements(typeof(PluginNode)))
                        builder.Bind<PluginNode>().To(exportedType);
        }

        protected override void Configure()
        {
            var viewManager = Container.Get<ViewManager>();
            GetPluginAssemblies().ForEach(viewManager.ViewAssemblies.Add);
        }

        private static IEnumerable<Assembly> GetPluginAssemblies()
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins";
            if (!Directory.Exists(pluginDir)) Directory.CreateDirectory(pluginDir);
            return Directory.GetFiles(pluginDir, "*.dll").Select(Assembly.LoadFile);
        }
    }
}