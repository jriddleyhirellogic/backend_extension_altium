using System;
using AltiumCommandsLibrary;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var commands = new Commands();

                while (true)
                {
                    Console.WriteLine("\nAltium Commands Console");
                    Console.WriteLine("1. Info About Parts");
                    Console.WriteLine("2. Get All Components");
                    Console.WriteLine("3. Get Nets of Components");
                    Console.WriteLine("4. Get Nets of Selected Components");
                    Console.WriteLine("5. Get All Components Parameters");
                    Console.WriteLine("6. Get Component Data");
                    Console.WriteLine("0. Exit");
                    Console.Write("\nEnter your choice: ");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            break;

                        case "2":
                            commands.GetAllComponents();
                            break;

                        case "3":
                            commands.GetNetsOfComponents();
                            break;

                        case "4":
                            commands.GetNetsOfSelectedComponents();
                            break;

                        case "5":
                            commands.GetAllComponentsParameters();
                            break;

                        case "6":
                            commands.GetComponentData();
                            break;

                        case "0":
                            return;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}