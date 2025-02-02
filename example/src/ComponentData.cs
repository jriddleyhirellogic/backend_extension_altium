//***********************************************************************************************
// Schematic Analysis - Component and Pin Information Representation
// 
// This program models schematic components, stores metadata, and processes connections 
// through pins and associated nets.
//***********************************************************************************************

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using DXP;
using EDP;
using PCB;
using SCH;

namespace ComponentData
{
    /// <summary>
    /// Represents information about a specific pin in a schematic component.
    /// </summary>
    public class PinInfo
    {
        /// <summary>
        /// Designator for the pin (e.g., "P1").
        /// </summary>
        public string Designator { get; set; }

        /// <summary>
        /// Pin name, typically representing the signal or function (e.g., "VCC", "GND").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Single net associated with this pin.
        /// </summary>
        public string Net { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the pin.</param>
        /// <param name="designator">The designator of the pin.</param>
        /// <param name="net">The net name connected to the pin.</param>
        public PinInfo(string name, string designator, string net)
        {
            Name = name;
            Designator = designator;
            Net = net;
        }
    }

    /// <summary>
    /// Represents a schematic component and its associated metadata.
    /// </summary>
    public class ComponentSchematic
    {

        private readonly Dictionary<string, string> PartPatterns = new()
        {
            { @"^R.*", "Resistors" },
            { @"^C", "Capacitors" },
            { @"^D", "Diodes" },
            { @"^Z", "Zeners" },
            { @"^L", "Inductors" },
            { @"^T", "Transformers" },
            { @"^Q", "BJTs" },
            { @"^74", "Digital ICs" },
            { @"^LM", "Linear ICs" },
            { @"^U", "Optocouplers" },
            { @"^K", "Relays" },
            { @"^NTC", "Thermistors" },
            { @"^IRF", "MOSFETs" },
            { @"^H", "Heaters" },
            { @"^F", "Fuses" },
            { @"^FLT", "Filter" }
        };
        /// <summary>
        /// A unique identifier for the component (e.g., "C12345").
        /// </summary>
        public string UniqueID { get; set; }

        /// <summary>
        /// The designator of the component (e.g., "U1", "R2").
        /// </summary>
        public string Designator { get; set; }

        /// <summary>
        /// Generic part number or reference for the component (e.g., "LM7805").
        /// </summary>
        public string GenericPartnumber { get; set; }

        /// <summary>
        /// Part type 
        /// </summary>
        public string PartType {get; set;}

        /// <summary>
        /// A description providing additional information about the component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Collection of pins associated with the component.
        /// </summary>
        public Dictionary<string, PinInfo> Pins { get; set; } = new Dictionary<string, PinInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentSchematic"/> class.
        /// </summary>
        public ComponentSchematic(string designator)
        {
            Designator = designator;
            DecodePartNumber();
        }

        /// <summary>
        /// Private method for decoding the part number to determine the part type.
        /// </summary>
        private void DecodePartNumber()
        {
            PartType = "Unrecognized Part"; // Default instead of "Unknown"

            if (!string.IsNullOrWhiteSpace(Designator))
            {
                foreach (var pattern in PartPatterns)
                {
                    if (Regex.IsMatch(Designator, pattern.Key, RegexOptions.IgnoreCase))
                    {
                        PartType = pattern.Value;
                        return; // Exit as soon as a match is found
                    }
                }
            }
        }

    }

    public class ComponentDataExtractor
    {
        private bool _compSelection;
        private List<ComponentSchematic> _compDataList;
        private List<string> _compInfo;
        private IClient _client;

        /// <summary>
        /// Constructor to initialize the object with selection preference.
        /// </summary>
        public ComponentDataExtractor(bool compSelection)
        {
            _compSelection = compSelection;
            _compDataList = new List<ComponentSchematic>();
            _compInfo = new List<string>();
            _client = DXP.GlobalVars.Client;
        }

        /// <summary>
        /// Executes the component extraction process.
        /// </summary>
        public void Run()
        {
            if (_client == null) return;

            IDXPWorkSpace workSpace = _client.GetDXPWorkspace();
            if (workSpace == null) return;

            IProject proj = workSpace.DM_FocusedProject() as IProject;
            proj.DM_Compile();

            IUnifiedDataModel dataModel = proj.DM_DataModel() as IUnifiedDataModel;
            dataModel.BeginRead();

            IDocument doc = proj.DM_DocumentFlattened() as IDocument;
            if (doc.DM_DocumentKind() != "SCH")
            {
                DXP.Utils.ShowWarning("This is not a schematic document");
                return;
            }

            ExtractComponents(doc, dataModel);

            dataModel.EndRead();
            SaveResults();
        }

        /// <summary>
        /// Extracts components from the schematic document.
        /// </summary>
        private void ExtractComponents(IDocument doc, IUnifiedDataModel dataModel)
        {
            int j;
            for (j = 0; j < doc.DM_ComponentCount(); j++)
            {
                IComponent comp = doc.DM_Components(j);
                if (comp == null) continue;

                object dispatch;
                if (!dataModel.SupportsInterface(comp.DM_SchHandle(), out dispatch) || dispatch == null)
                    continue;

                ISch_BasicContainer Sch_BasicContainer = dispatch as ISch_BasicContainer;
                if (Sch_BasicContainer == null) continue;

                SCH.TObjectId ObjectId = Sch_BasicContainer.GetState_ObjectId();
                if (ObjectId != SCH.TObjectId.eSchComponent) continue;

                ISch_Component schComponent = Sch_BasicContainer as ISch_Component;
                if (!schComponent.GetState_Selection() && _compSelection) continue;

                ProcessComponent(comp, schComponent);
            }
        }

        /// <summary>
        /// Processes individual components and extracts relevant data.
        /// </summary>
        private void ProcessComponent(IComponent comp, ISch_Component schComponent)
        {
            int k;
            if (comp.DM_SubPartCount() == 1)
            {
                ComponentSchematic componentData = CreateComponentData(comp, schComponent);
                for (k = 0; k < comp.DM_PinCount(); k++)
                    AddPinData(componentData, comp.DM_Pins(k));

                _compDataList.Add(componentData);
            }
            else if (comp.DM_SubPartCount() > 1)
            {
                for (int multiPartCnt = 0; multiPartCnt < comp.DM_SubPartCount(); multiPartCnt++)
                {
                    IPart multiPart = comp.DM_SubParts(multiPartCnt);
                    ComponentSchematic componentData = CreateComponentData(comp, schComponent);
                    for (k = 0; k < multiPart.DM_PinCount(); k++)
                        AddPinData(componentData, multiPart.DM_Pins(k));

                    _compDataList.Add(componentData);
                }
            }
        }

        /// <summary>
        /// Creates a new component data object.
        /// </summary>
        private ComponentSchematic CreateComponentData(IComponent comp, ISch_Component schComponent)
        {
            return new ComponentSchematic(comp.DM_FullLogicalDesignator())
            {
                UniqueID = comp.DM_UniqueId(),
                GenericPartnumber = schComponent.GetState_DesignItemId(),
                Description = schComponent.GetState_ComponentDescription()
            };
        }

        /// <summary>
        /// Adds pin data to a component.
        /// </summary>
        private void AddPinData(ComponentSchematic componentData, INetItem pin)
        {
            if (pin.DM_FlattenedNetName() != "?")
            {
                componentData.Pins[pin.DM_PinNumber()] = new PinInfo(
                    name: pin.DM_PinName(),
                    designator: pin.DM_PinNumber(),
                    net: pin.DM_FlattenedNetName()
                );
            }
        }

        /// <summary>
        /// Saves extracted data to a text file.
        /// </summary>
        private void SaveResults()
        {
            foreach (var component in _compDataList)
            {
                _compInfo.Add($"Component: {component.Designator}");
                _compInfo.Add($" Part: {component.GenericPartnumber}");
                _compInfo.Add($" Part Type: {component.PartType}");
                foreach (var pinEntry in component.Pins)
                {
                    _compInfo.Add($"  Pin {pinEntry.Key}: Name={pinEntry.Value.Name}: Net={pinEntry.Value.Net}");
                }
            }

            string fileName = _client.GetClientAPI().SpecialFolder_AltiumAllUserApplicationData() + "\\Command_GetComponentData2.txt";
            File.WriteAllLines(fileName, _compInfo.ToArray());

            IServerDocument reportDocument = _client.OpenDocument("Text", fileName);
            if (reportDocument != null)
                _client.ShowDocument(reportDocument);
        }
}


}
