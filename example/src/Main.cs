using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DXP;
using EDP;

using EPSA;
using ComponentData;

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
            new Commands().Command_InfoAboutParts(argView, argParameters);
        }

        private void Command_GetAllComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().Command_GetAllComponents(argView, argParameters);
        }

        private void Command_GetNetsOfComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().Command_GetNetsOfComponents(argView, argParameters);
        }

        private void Command_GetNetsOfSelectedComponents(IServerDocumentView argView, ref string argParameters)
        {
            new Commands().Command_GetNetsOfSelectedComponents(argView, argParameters);
        }

        private void Command_GetComponentData(IServerDocumentView argView, ref string argParameters)
        {
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine("Command_GetComponentData", false, false);
            bool compSelection = false;
            //new Commands().Command_GetComponentData(argView, argParameters, compSelection);
            // Create the object
            ComponentDataExtractor extractor = new ComponentDataExtractor(compSelection);
            // Run it when ready
            extractor.Run();
        }

        private void Command_OpenEPSA(IServerDocumentView argView, ref string argParameters)
        {
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine("Command_OpenEPSA", false, false);
            EPSAOpener app = new EPSAOpener();
            app.ExecuteAsync().GetAwaiter().GetResult();
        }

        protected override IServerDocument NewDocumentInstance(string argKind, string argFileName)
        {
            return null;
        }
    }

    
}