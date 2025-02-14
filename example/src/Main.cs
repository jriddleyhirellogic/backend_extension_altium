using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DXP;
using EDP;
using AltiumCommandsLibrary;

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
            ((CommandLauncher)CommandLauncher).RegisterCommand("GetNetsOfComponents",this.Command_GetNetsOfComponents);
            ((CommandLauncher)CommandLauncher).RegisterCommand("GetNetsOfSelectedComponents",this.Command_GetNetsOfSelectedComponents);
            ((CommandLauncher)CommandLauncher).RegisterCommand("GetComponentData",this.Command_GetComponentData);
            ((CommandLauncher)CommandLauncher).RegisterCommand("OpenEPSA",this.Command_OpenEPSA);
        }

        private void Command_InfoAboutParts(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().InfoAboutParts(argView, argParameters);
        }

        private void Command_GetAllComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().GetAllComponents();
        }

        private void Command_GetNetsOfComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().GetNetsOfComponents();
        }

        private void Command_GetNetsOfSelectedComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().GetNetsOfSelectedComponents();
        }

        private void Command_GetComponentData(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().GetComponentData();
        }

        private void Command_OpenEPSA(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().OpenEPSA();
        }

        protected override IServerDocument NewDocumentInstance(string argKind, string argFileName)
        {
            return null;
        }
    }

    
}