using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DXP;
using EDP;

namespace CSharpPlugin
{
    public interface IPluginFactory
    {
        object InvokePluginFactory(IClient client);
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class PluginFactory : IPluginFactory
    {
        public object InvokePluginFactory(IClient client)
        {
            PluginServerModule serverModule = new PluginServerModule(client, "InfoAboutParts");
            return serverModule;
        }
    }
    
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class PluginServerModule : ServerModule
    {
        public PluginServerModule(IClient argClient, string argModuleName)
            : base(argClient, argModuleName)
        {
        }

        protected override void InitializeCommands()
        {
            ((CommandLauncher)CommandLauncher).RegisterCommand("ShowInfoAboutParts", this.Command_InfoAboutParts);
            ((CommandLauncher)CommandLauncher).RegisterCommand("GetAllComponents", this.Command_GetAllComponents);

        }

        private void Command_InfoAboutParts(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().Command_InfoAboutParts(argView, argParameters);
        }

        private void Command_GetAllComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().Command_GetAllComponents(argView, argParameters);
        }

        protected override IServerDocument NewDocumentInstance(string argKind, string argFileName)
        {
            return null;
        }
    }

    
}