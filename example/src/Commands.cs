using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using DXP;
using EDP;
using PCB;
using SCH;

using EPSA;
using ComponentData;

namespace AltiumCommandsLibrary
{
    public interface IAltiumCommands
    {
        void InfoAboutParts(IServerDocumentView argView, string argParameters);
        void GetAllComponents();
        void GetNetsOfComponents();
        void GetNetsOfSelectedComponents();
        void GetAllComponentsParameters();
        void GetComponentData();
    }

    public class Commands : IAltiumCommands
    {
        List<ComponentSchematic> compDataList = new List<ComponentSchematic>();

        public static void Log(string text) {
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine(text, false, false);
        }
        
        public void InfoAboutParts(IServerDocumentView argView, string argParameters)
        {
            if (CheckKindSCH(argView))
            {
                _InfoAboutParts();
            }
        }

        public void GetComponentData()
        {
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine("GetComponentData", false, false);
            bool compSelection = false;
            //new Commands().Command_GetComponentData(argView, argParameters, compSelection);
            // Create the object
            ComponentDataExtractor extractor = new ComponentDataExtractor(compSelection);
            // Run it when ready
            extractor.Run();
        }

        public void OpenEPSA()
        {
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine("Command_OpenEPSA", false, false);
            EPSAOpener app = new EPSAOpener();
            app.ExecuteAsync().GetAwaiter().GetResult();
        }

        private bool CheckKindSCH(IServerDocumentView argView)
        {
            if (argView != null)
            {
                IServerDocument ownerDocument = argView.GetOwnerDocument();
                if (!"SCH".Equals(ownerDocument.GetKind(), StringComparison.OrdinalIgnoreCase))
                {
                    DXP.Utils.ShowWarning("This is not a Schematic document");
                    return false;
                }
            }

            return true;
        }

        private void GeneratePinsReport(ArrayList pinsReport)
        {
            ArrayList report = new ArrayList();
            report.Add("Schematic Components and their Pins Report...");
            report.Add("=============================================");
            report.AddRange(pinsReport);

            string fileName = "C:\\work\\ComponentPins.txt";
            File.WriteAllLines(fileName, (string[])report.ToArray(typeof(string)));

            IServerDocument reportDocument = DXP.GlobalVars.Client.OpenDocument("Text", fileName);
            if (reportDocument != null)
                DXP.GlobalVars.Client.ShowDocument(reportDocument);
        }

        private void ObtainPins(ref ArrayList argReport, ISch_Component argComponent)
        {
            bool pinFound = false;

            ISch_Designator designator = argComponent.GetState_SchDesignator();
            argReport.Add("Component Information: Designator = " + designator.GetState_Text() +
                            " Library Reference = " + argComponent.GetState_LibReference());

            ISch_Iterator pinIterator = argComponent.SchIterator_Create();
            pinIterator.SetState_IterationDepth((int)SCH.TIterationDepth.eIterateAllLevels);
            SCH.TObjectSet objectSet = new SCH.TObjectSet();
            objectSet.Add(SCH.TObjectId.ePin);
            pinIterator.AddFilter_ObjectSet(objectSet);

            ISch_Pin pin = pinIterator.FirstSchObject() as ISch_Pin;
            while (pin != null)
            {
                if (!pinFound)
                {
                    argReport.Add("Pins for this component:");
                    pinFound = true;
                }

                IPoint location = pin.GetState_Location();
                
                argReport.Add("Pin name: " + pin.GetState_Name());
                argReport.Add("Pin designator: " + pin.GetState_Designator());
                //argReport.Add("GetState_DefaultValue: " + pin.GetState_DefaultValue());
                //argReport.Add("GetState_Description: " + pin.GetState_Description());
                //argReport.Add("GetState_HiddenNetName: " + pin.GetState_HiddenNetName());

                string result = "Pin " + pin.GetState_Designator() + " located at (x=" +
                                location.GetX() + ", y=" +
                                location.GetY() + ")";
                argReport.Add(result);
                pin = pinIterator.NextSchObject() as ISch_Pin;
            }

            argComponent.SchIterator_Destroy(ref pinIterator);

            if (!pinFound)
                argReport.Add("There are no pins for this component.");
        }

        private void ObtainParameters(ref ArrayList argReport, ISch_Component argComponent)
        {
        
            ISch_Iterator compParamIterator = argComponent.SchIterator_Create();
            compParamIterator.SetState_IterationDepth((int)SCH.TIterationDepth.eIterateAllLevels);
            SCH.TObjectSet objectSet = new SCH.TObjectSet();
            objectSet.Add(SCH.TObjectId.eParameter);
            compParamIterator.AddFilter_ObjectSet(objectSet);

        // List<ISch_Parameter> listOfParameters = new List<ISch_Parameter>();

            ISch_Parameter parameter = compParamIterator.FirstSchObject() as ISch_Parameter;
            while(parameter != null)
            {
                //listOfParameters.Add(parameter);
                argReport.Add("comp GetState_DesignItemId() : " + argComponent.GetState_DesignItemId());
                argReport.Add("comp GetState_ComponentDescription() : " + argComponent.GetState_ComponentDescription());
                argReport.Add("comp GetState_ConfiguratorName() : " + argComponent.GetState_ConfiguratorName());
                argReport.Add("comp GetState_SymbolReference() : " + argComponent.GetState_SymbolReference());
                argReport.Add("comp GetState_ConfigurationParameters() : " + argComponent.GetState_ConfigurationParameters());

                argReport.Add("GetState_Description : " + parameter.GetState_Description());
                argReport.Add("GetState_Name : " + parameter.GetState_Name());
                parameter = compParamIterator.NextSchObject() as ISch_Parameter;
            }

            argComponent.SchIterator_Destroy(ref compParamIterator);

        }

        private void _InfoAboutParts()
        {
            ISch_ServerInterface schServer = SCH.GlobalVars.SchServer;
            if (schServer == null)
                return;    

            ISch_Document currentDoc = schServer.GetCurrentSchDocument();

            ISch_Sheet currentSheet = schServer.GetCurrentSchDocument() as ISch_Sheet;
            /*
            if(null == currentSheet)
                return;

            if(currentSheet.LibIsEmpty())
            {
                DXP.Utils.ShowWarning("Schematic Library is Empty.");
                return;
            }

            //IConnectionsArray wireConnections = ISch_SheetHelper.GetState_WireConnections(currentSheet);
            wireConnections = currentSheet.WireConnections;
            */
            ArrayList pinsReport = new ArrayList();

            SCH.TObjectSet objectSet = new SCH.TObjectSet();
            objectSet.Add(SCH.TObjectId.eSchComponent);
            ISch_Iterator iterator = currentDoc.SchIterator_Create();
            iterator.AddFilter_ObjectSet(objectSet);

            ISch_Component schComponent = iterator.FirstSchObject() as ISch_Component;
            while (schComponent != null)
            {
                if(schComponent.GetState_Selection())
                    ObtainPins(ref pinsReport, schComponent);
                    ObtainParameters(ref pinsReport, schComponent);
                schComponent = iterator.NextSchObject() as ISch_Component;
            }

            pinsReport.Add("Component Pins Report created on " + DateTime.Now);

            currentDoc.SchIterator_Destroy(ref iterator);

            GeneratePinsReport(pinsReport);
        }

        public void GetAllComponents()
            {
                IClient client = DXP.GlobalVars.Client;
                IWorkspace workSpace = client.GetDXPWorkspace() as IWorkspace;
                IProject project = workSpace.DM_FocusedProject();
                if (project.DM_NeedsCompile())
                    project.DM_Compile();

                List<string> designators = new List<string>();
                ArrayList compReport = new ArrayList();

                IDocument document = project.DM_DocumentFlattened();
                int componentCount = document.DM_ComponentCount();
                for (int i = 0; i < componentCount; i++)
                {
                    IComponent component = document.DM_Components(i);
                    designators.Add(component.DM_PhysicalDesignator());
                    compReport.Add("Designator: " + component.DM_PhysicalDesignator());
                    compReport.Add("DM_UniqueId: " + component.DM_UniqueId());
                    //compReport.Add("DM_AlternatePart: " + component.DM_AlternatePart());
                    //compReport.Add("DM_VariationCount: " + component.DM_VariationCount());
                }

                string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Command_GetAllComponents.Txt";
                //File.WriteAllLines(fileName, compReport.ToArray());

                File.WriteAllLines(fileName, (string[])compReport.ToArray(typeof(string)));

                IServerDocument reportDocument = client.OpenDocument("Text", fileName);
                if (reportDocument != null)
                    client.ShowDocument(reportDocument);
            }

            private void CompileCurrentProject()
            {
                // force a compile on the current project
                string str = "";
                DXP.Utils.SetParamterValue(ref str, "Action", "Compile");
                DXP.Utils.SetParamterValue(ref str, "ObjectKind", "Project");
                DXP.Utils.MessageRouterSendCommandToModule("WorkspaceManager:Compile", ref str, null);
            }

            
            public void GetNetsOfComponents()
            {
                int j, k;
                List<string> compInfo = new List<string>();
                IComponent comp;
                int multiPartCnt;
                IPart multiPart;
                INetItem pin;

                IClient client = DXP.GlobalVars.Client;
                if (client == null)
                    return;

                IDXPWorkSpace workSpace = client.GetDXPWorkspace();
                if (workSpace == null)
                    return;

                CompileCurrentProject();
                // Get current schematic document.
                IDocument doc = workSpace.DM_FocusedDocument() as IDocument;
                if (doc.DM_DocumentKind() != "SCH")
                {
                    DXP.Utils.ShowWarning("This is not a schematic document");
                    return;
                }

                for (j = 0; j < doc.DM_ComponentCount(); j++)
                {
                    comp = doc.DM_Components(j);

                    //DXP.Utils.ShowWarning($"component count : {j}");

                    if (comp.DM_SubPartCount() == 1)
                    {
                        compInfo.Add(comp.DM_FullLogicalDesignator());
                        for (k = 0; k < comp.DM_PinCount(); k++)
                        {
                            pin = comp.DM_Pins(k);
                            compInfo.Add("sub " + pin.DM_PinNumber() + " : " + pin.DM_FlattenedNetName());
                        }                    
                    }
                    else if (comp.DM_SubPartCount() > 1)
                    {
                        for (multiPartCnt = 0; multiPartCnt < comp.DM_SubPartCount(); multiPartCnt++)
                        {
                            multiPart = comp.DM_SubParts(multiPartCnt);
                            compInfo.Add(multiPart.DM_FullLogicalDesignator());
                            for (k = 0; k < multiPart.DM_PinCount(); k++)
                            {
                                pin = multiPart.DM_Pins(k);
                                if (pin.DM_FlattenedNetName() != "?")
                                    compInfo.Add(pin.DM_PinNumber() + " : " + pin.DM_FlattenedNetName());
                            }
                        }
                    }
                }

                string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Project_Report.Txt";
                File.WriteAllLines(fileName, compInfo.ToArray());

                IServerDocument reportDocument = client.OpenDocument("Text", fileName);
                if (reportDocument != null)
                    client.ShowDocument(reportDocument);
            }
        
        
            public void GetNetsOfSelectedComponents()
            {
                int j, k;
                List<string> compInfo = new List<string>();
                IComponent comp;
                int multiPartCnt;
                IPart multiPart;
                INetItem pin;
                ISch_BasicContainer Sch_BasicContainer;
                ISch_Component schComponent;
                IUnifiedDataModel dataModel;
                object dispatch;
                SCH.TObjectId ObjectId;
                IProject proj;

                IClient client = DXP.GlobalVars.Client;
                if (client == null)
                    return;

                IDXPWorkSpace workSpace = client.GetDXPWorkspace();
                if (workSpace == null)
                    return;

                proj = workSpace.DM_FocusedProject() as IProject;         
                proj.DM_Compile();

                dataModel= proj.DM_DataModel() as IUnifiedDataModel;
                dataModel.BeginRead();

                //CompileCurrentProject();
                // Get current schematic document.
                IDocument doc = workSpace.DM_FocusedDocument() as IDocument;
                if (doc.DM_DocumentKind() != "SCH")
                {
                    DXP.Utils.ShowWarning("This is not a schematic document");
                    return;
                }

                for (j = 0; j < doc.DM_ComponentCount(); j++)
                {
                    comp = doc.DM_Components(j);

                    //DXP.Utils.ShowWarning($"component count : {j}");
                    if(comp != null)
                    {
                    if (dataModel.SupportsInterface(comp.DM_SchHandle(), out dispatch) && dispatch != null)
                    {
                            Sch_BasicContainer = dispatch as ISch_BasicContainer;
                            if(Sch_BasicContainer != null)
                            {
                                ObjectId = Sch_BasicContainer.GetState_ObjectId();
                                if(ObjectId == SCH.TObjectId.eSchComponent)
                                {
                                    schComponent = Sch_BasicContainer as ISch_Component;

                                    if(schComponent.GetState_Selection())
                                    {
                                        if (comp.DM_SubPartCount() == 1)
                                        {                            
                                            compInfo.Add(comp.DM_FullLogicalDesignator());
                                            for (k = 0; k < comp.DM_PinCount(); k++)
                                            {
                                                pin = comp.DM_Pins(k);
                                                compInfo.Add("sub " + pin.DM_PinNumber() + " : " + pin.DM_FlattenedNetName());
                                            }                    
                                        }
                                        else if (comp.DM_SubPartCount() > 1)
                                        {
                                            for (multiPartCnt = 0; multiPartCnt < comp.DM_SubPartCount(); multiPartCnt++)
                                            {
                                                multiPart = comp.DM_SubParts(multiPartCnt);
                                                compInfo.Add(multiPart.DM_FullLogicalDesignator());
                                                for (k = 0; k < multiPart.DM_PinCount(); k++)
                                                {
                                                    pin = multiPart.DM_Pins(k);
                                                    if (pin.DM_FlattenedNetName() != "?")
                                                        compInfo.Add(pin.DM_PinNumber() + " : " + pin.DM_FlattenedNetName());
                                                }
                                            }
                                        }
                                    }    
                                }
                            }    
                    }
                    }
                }
                dataModel.EndRead();
                string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Selected_Report.Txt";
                File.WriteAllLines(fileName, compInfo.ToArray());

                IServerDocument reportDocument = client.OpenDocument("Text", fileName);
                if (reportDocument != null)
                    client.ShowDocument(reportDocument);

            }

            public void GetAllComponentsParameters()
            {
                IClient client = DXP.GlobalVars.Client;
                IWorkspace workSpace = client.GetDXPWorkspace() as IWorkspace;
                IProject project = workSpace.DM_FocusedProject();
                if (project.DM_NeedsCompile())
                    project.DM_Compile();

                List<string> designators = new List<string>();
                ArrayList compReport = new ArrayList();

                IDocument document = project.DM_DocumentFlattened();
                int componentCount = document.DM_ComponentCount();
                for (int i = 0; i < componentCount; i++)
                {
                    IComponent component = document.DM_Components(i);
                    designators.Add(component.DM_PhysicalDesignator());
                    compReport.Add("Designator: " + component.DM_PhysicalDesignator());
                    compReport.Add("DM_UniqueIdName_1: " + component.DM_UniqueIdName_1());
                    //compReport.Add("DM_AlternatePart: " + component.DM_AlternatePart());
                    //compReport.Add("DM_VariationCount: " + component.DM_VariationCount());
                }

                string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Project_Report.Txt";
                //File.WriteAllLines(fileName, compReport.ToArray());

                File.WriteAllLines(fileName, (string[])compReport.ToArray(typeof(string)));

                IServerDocument reportDocument = client.OpenDocument("Text", fileName);
                if (reportDocument != null)
                    client.ShowDocument(reportDocument);
            }

            //***********************************************************************************************
            //  uniqueid
            //      designator
            //      type
            //      description
            //      schlocation
            //      designItemId (part number)
            //      partnumberURL
            //      altParts
            //      pins:{
            //          {designator0 : name, net},
            //          {designator1 : name, net},
            //           ...
            //          {designatorn : name, net},
            //      }
            //***********************************************************************************************

            public void GetComponentData(IServerDocumentView view, string parameters, bool compSelection)
            {
                int j, k;
                List<string> compInfo = new List<string>();
                IComponent comp;
                int multiPartCnt;
                IPart multiPart;
                INetItem pin;
                ISch_BasicContainer Sch_BasicContainer;
                ISch_Component schComponent;
                IUnifiedDataModel dataModel;
                object dispatch;
                SCH.TObjectId ObjectId;
                IProject proj;
                //List<ComponentSchematic> compDataList = new List<ComponentSchematic>();

                IClient client = DXP.GlobalVars.Client;
                if (client == null)
                    return;

                IDXPWorkSpace workSpace = client.GetDXPWorkspace();
                if (workSpace == null)
                    return;

                proj = workSpace.DM_FocusedProject() as IProject;         
                proj.DM_Compile();

                dataModel= proj.DM_DataModel() as IUnifiedDataModel;
                dataModel.BeginRead();

                IDocument doc = proj.DM_DocumentFlattened() as IDocument;
                //IDocument doc = workSpace.DM_FocusedDocument() as IDocument;
                
                if (doc.DM_DocumentKind() != "SCH")
                {
                    DXP.Utils.ShowWarning("This is not a schematic document");
                    return;
                }

                for (j = 0; j < doc.DM_ComponentCount(); j++)
                {
                    comp = doc.DM_Components(j);

                    //DXP.Utils.ShowWarning($"component count : {j}");
                    if(comp != null)
                    {
                    if (dataModel.SupportsInterface(comp.DM_SchHandle(), out dispatch) && dispatch != null)
                    {
                            Sch_BasicContainer = dispatch as ISch_BasicContainer;
                            if(Sch_BasicContainer != null)
                            {
                                ObjectId = Sch_BasicContainer.GetState_ObjectId();
                                if(ObjectId == SCH.TObjectId.eSchComponent)
                                {
                                    schComponent = Sch_BasicContainer as ISch_Component;

                                    if(schComponent.GetState_Selection() || !compSelection)
                                    {
                                        if (comp.DM_SubPartCount() == 1)
                                        {                            
                                            //compInfo.Add(comp.DM_UniqueId());
                                            //compInfo.Add(comp.DM_FullLogicalDesignator());
                                            //compInfo.Add(schComponent.GetState_DesignItemId());

                                            ComponentSchematic componentDataFromSchematic = new ComponentSchematic(comp.DM_FullLogicalDesignator())
                                            {
                                                UniqueID = comp.DM_UniqueId(),
                                                GenericPartnumber = schComponent.GetState_DesignItemId(),
                                                Description = schComponent.GetState_ComponentDescription()
                                            };

                                            for (k = 0; k < comp.DM_PinCount(); k++)
                                            {
                                                pin = comp.DM_Pins(k);
                                                //compInfo.Add(pin.DM_PinNumber());
                                                //compInfo.Add(pin.DM_PinName());
                                                //compInfo.Add(pin.DM_FlattenedNetName());
                                                //compInfo.Add(pin.DM_PartType());
                                                //compInfo.Add(pin.DM_SheetName());

                                                componentDataFromSchematic.Pins[pin.DM_PinNumber()] = new PinInfo(
                                                    name: pin.DM_PinName(),
                                                    designator: pin.DM_PinNumber(),
                                                    net: pin.DM_FlattenedNetName()
                                                );
                                                
                                            }                    
                                            
                                            compDataList.Add(componentDataFromSchematic);
                                        }
                                        else if (comp.DM_SubPartCount() > 1)
                                        {
                                            for (multiPartCnt = 0; multiPartCnt < comp.DM_SubPartCount(); multiPartCnt++)
                                            {
                                                multiPart = comp.DM_SubParts(multiPartCnt);
                                                
                                                //compInfo.Add(comp.DM_UniqueId());
                                                //compInfo.Add(comp.DM_FullLogicalDesignator());
                                                //compInfo.Add(schComponent.GetState_DesignItemId());

                                                ComponentSchematic componentDataFromSchematic = new ComponentSchematic(comp.DM_FullLogicalDesignator())
                                                {
                                                    UniqueID = comp.DM_UniqueId(),
                                                    GenericPartnumber = schComponent.GetState_DesignItemId(),
                                                    Description = schComponent.GetState_ComponentDescription()
                                                };
                                            

                                                for (k = 0; k < multiPart.DM_PinCount(); k++)
                                                {
                                                    pin = multiPart.DM_Pins(k);
                                                    if (pin.DM_FlattenedNetName() != "?")
                                                    {
                                                        //compInfo.Add(pin.DM_PinNumber());
                                                        //compInfo.Add(pin.DM_PinName());
                                                        //compInfo.Add(pin.DM_FlattenedNetName());
                                                        //compInfo.Add(pin.DM_PartType());
                                                        //compInfo.Add(pin.DM_SheetName());

                                                        componentDataFromSchematic.Pins[pin.DM_PinNumber()] = new PinInfo(
                                                            name: pin.DM_PinName(),
                                                            designator: pin.DM_PinNumber(),
                                                            net: pin.DM_FlattenedNetName()
                                                        );

                                                    }
                                                }

                                                compDataList.Add(componentDataFromSchematic);
                                            }
                                        }
                                    }    
                                }
                            }    
                    }
                    }
                }
                dataModel.EndRead();

                foreach (var component in compDataList)
                {
                    compInfo.Add($"Component: {component.Designator}");
                    compInfo.Add($" Part: {component.GenericPartnumber}");
                    compInfo.Add($" Part Type: {component.PartType}");
                    foreach (var pinEntry in component.Pins)
                    {
                        compInfo.Add($"  Pin {pinEntry.Key}: Name={pinEntry.Value.Name}: Net={pinEntry.Value.Net}");
                    }
                }

                string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Command_GetComponentData.txt";
                File.WriteAllLines(fileName, compInfo.ToArray());

                IServerDocument reportDocument = client.OpenDocument("Text", fileName);
                if (reportDocument != null)
                    client.ShowDocument(reportDocument);

            }
            /*
            public void FetchNetsFromFocussedDocumentOnlyAndGenerateParameters(
                List<string> compInfo, 
                IProject project, 
                System.Drawing.Color parameterColor, 
                int parameterOffset, 
                string parameterName, 
                bool parameterNameShow, 
                bool assignParamToPin)
            {
                int j, k;
                IDocument doc;
                IDocument focussedDoc = GetWorkspace.DM_FocusedDocument;
                ISch_Document currentSch;

                if (focussedDoc == null)
                    return;

                // Obtain the physical document that's the same as the focussed document
                // Need the physical document for net information...
                for (j = 0; j < project.DM_PhysicalDocumentCount; j++)
                {
                    doc = project.DM_PhysicalDocuments(j);
                    if (doc.DM_FullPath == focussedDoc.DM_FullPath)
                        break;
                }

                if (doc.DM_FullPath != focussedDoc.DM_FullPath)
                    return;

                // Get nets from the Doc - IDocument type
                FetchNetsOfComponents(doc, compInfo);

                currentSch = SchServer.GetCurrentSchDocument();
                if (currentSch == null)
                    return;

                try
                {
                    SchServer.ProcessControl.PreProcess(currentSch, "");
                    GenerateParametersForPins(currentSch, compInfo, parameterColor, parameterOffset, parameterName, parameterNameShow, assignParamToPin);
                }
                finally
                {
                    SchServer.ProcessControl.PostProcess(currentSch, "");
                }
            }


            public void FetchComponentNetInfo(
                int documentScope, 
                List<string> documentList, 
                System.Drawing.Color parameterColor, 
                int parameterOffset, 
                string parameterName, 
                bool parameterNameShow, 
                bool assignParamToPin)
            {
                List<string> compInfo = new List<string>();
                IProject project = GetWorkspace.DM_FocusedProject;

                try
                {
                    BeginHourGlass();

                    // Check if schematic server exists or not.
                    if (SchServer == null)
                        return;

                    // Do a compile so the logical documents get expanded into physical documents.
                    if (project == null)
                        return;

                    project.DM_Compile();

                    if (documentScope == 0)
                    {
                        FetchNetsFromFocussedDocumentOnlyAndGenerateParameters(
                            compInfo, project, parameterColor, parameterOffset, 
                            parameterName, parameterNameShow, assignParamToPin);
                    }
                }
                finally
                {
                    compInfo.Clear();
                }
            }
        */
    }
}