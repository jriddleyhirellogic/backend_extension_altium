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

public class Commands 
{
    public void Command_InfoAboutParts(IServerDocumentView argView, string argParameters)
    {
        if (CheckKindSCH(argView))
        {
            InfoAboutParts();
        }
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
            
            argReport.Add("Name: " + pin.GetState_Name());
            argReport.Add("Designator: " + pin.GetState_Designator());
            argReport.Add("GetState_DefaultValue: " + pin.GetState_DefaultValue());
            argReport.Add("GetState_Description: " + pin.GetState_Description());
            argReport.Add("GetState_HiddenNetName: " + pin.GetState_HiddenNetName());

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

    private void InfoAboutParts()
    {
        ISch_ServerInterface schServer = SCH.GlobalVars.SchServer;
        if (schServer == null)
            return;    

        ISch_Document currentSheet = schServer.GetCurrentSchDocument();

        ArrayList pinsReport = new ArrayList();

        SCH.TObjectSet objectSet = new SCH.TObjectSet();
        objectSet.Add(SCH.TObjectId.eSchComponent);
        ISch_Iterator iterator = currentSheet.SchIterator_Create();
        iterator.AddFilter_ObjectSet(objectSet);

        ISch_Component schComponent = iterator.FirstSchObject() as ISch_Component;
        while (schComponent != null)
        {
            if(schComponent.GetState_Selection())
                ObtainPins(ref pinsReport, schComponent);
            schComponent = iterator.NextSchObject() as ISch_Component;
        }

        pinsReport.Add("Component Pins Report created on " + DateTime.Now);

        currentSheet.SchIterator_Destroy(ref iterator);

        GeneratePinsReport(pinsReport);
    }

    public void Command_GetAllComponents(IServerDocumentView view, string parameters)
        {
            IClient client = DXP.GlobalVars.Client;
            IWorkspace workSpace = client.GetDXPWorkspace() as IWorkspace;
            IProject project = workSpace.DM_FocusedProject();
            if (project.DM_NeedsCompile())
                project.DM_Compile();

            List<string> designators = new List<string>();

            IDocument document = project.DM_DocumentFlattened();
            int componentCount = document.DM_ComponentCount();
            for (int i = 0; i < componentCount; i++)
            {
                IComponent component = document.DM_Components(i);
                designators.Add(component.DM_PhysicalDesignator());
            }

            string fileName = client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Project_Report.Txt";
            File.WriteAllLines(fileName, designators.ToArray());

            IServerDocument reportDocument = client.OpenDocument("Text", fileName);
            if (reportDocument != null)
                client.ShowDocument(reportDocument);
        }
}