// See https://aka.ms/new-console-template for more information

using System;
using System.Collections;
using System.ComponentModel;
using System.IO.Ports;

internal class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PsuInterfaceForm());
    }
}