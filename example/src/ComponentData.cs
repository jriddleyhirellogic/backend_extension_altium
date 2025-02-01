//***********************************************************************************************
// Schematic Analysis - Component and Pin Information Representation
// 
// This program models schematic components, stores metadata, and processes connections 
// through pins and associated nets.
//***********************************************************************************************

using System;
using System.Collections.Generic;

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
        public ComponentSchematic() { }
    }

   // class Program
   // {
   //     static void Main()
   //     {
   //         // Sample data
   //         var components = new List<ComponentSchematic>
   //         {
   //             new ComponentSchematic
   //             {
   //                 UniqueID = "ID1",
   //                 Designator = "U1",
   //                 Pins = new Dictionary<string, PinInfo>
   //                 {
   //                     { "P1", new PinInfo("CLK", "P1", "Net1") },
   //                     { "P2", new PinInfo("DATA", "P2", "Net2") }
   //                 }
   //             },
   //             new ComponentSchematic
   //             {
   //                 UniqueID = "ID2",
   //                 Designator = "R1",
   //                 Pins = new Dictionary<string, PinInfo>
   //                 {
   //                     { "P1", new PinInfo("Input", "P1", "Net3") },
   //                     { "P2", new PinInfo("Output", "P2", "Net3") }
   //                 }
   //             }
   //         };
//
   //         // Loop through components and pins
   //         foreach (var component in components)
   //         {
   //             Console.WriteLine($"Component ID: {component.UniqueID}, Designator: {component.Designator}");
   //             
   //             foreach (var pinEntry in component.Pins)
   //             {
   //                 var pin = pinEntry.Value;
   //                 Console.WriteLine($"Pin {pin.Designator}: {pin.Name}, Net: {pin.Net}");
   //             }
   //         }
   //     }
   // }
}
