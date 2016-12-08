using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace Si.Common.Core
{
    public static class CompositionContainerFactory
    {
        private static readonly Lazy<CompositionContainer> ContainerInstance
            = new Lazy<CompositionContainer>(BuildContainer);

        public static CompositionContainer Instance => ContainerInstance.Value;

        private static CompositionContainer BuildContainer()
        {
            SetWorkingFoldertoCurrentAssemblyPath();

            var exeCatalog = new DirectoryCatalog(Directory.GetCurrentDirectory(), "*.exe");
            var exeExportProvider = new CatalogExportProvider(exeCatalog);

            var dllCatalog = new DirectoryCatalog(Directory.GetCurrentDirectory(), "*.dll");
            var dllExportProvider = new CatalogExportProvider(dllCatalog);

            var exportProviders = new ExportProvider[] { exeExportProvider, dllExportProvider, };
            var container = new CompositionContainer(CompositionOptions.DisableSilentRejection, exportProviders);

            exeExportProvider.SourceProvider = container;
            dllExportProvider.SourceProvider = container;
            
            return container;
        }

        private static void SetWorkingFoldertoCurrentAssemblyPath()
        {
            var path = Path.GetDirectoryName(SafeAssemblyPath(Assembly.GetCallingAssembly()));
            if (!string.IsNullOrEmpty(path))
            {
                Directory.SetCurrentDirectory(path);
            }
        }

        private static string SafeAssemblyPath(Assembly assembly)
        {
            return assembly.CodeBase
                           .Replace("file:///", String.Empty)
                           .Replace("/", "\\");
        }

    }
}
