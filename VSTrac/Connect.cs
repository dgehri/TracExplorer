#region GPL Licence
/**********************************************************************
 VSTrac - Visual Studo Trac Integration
 Copyright (C) 2008 Mladen Mihajlovic
 http://vstrac.devjavu.com/
 
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
**********************************************************************/
#endregion

using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace VSTrac
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        #region Variables
        internal DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Window _explorerWindow;
        #endregion

        #region Constants
        private const string _explorerWindowGuid = "{F1AEDCD1-B798-417c-B770-C9F6AE9BB44F}";
        #endregion

        #region ctor
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }
        #endregion

        #region OnConnection
        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                string toolsMenuName;

                try
                {
                    //If you would like to move the command to a different menu, change the word "Tools" to the 
                    //  English version of the menu. This code will take the culture, append on the name of the menu
                    //  then add the command to that menu. You can find a list of all the top-level menus in the file
                    //  CommandBar.resx.
                    string resourceName;
                    ResourceManager resourceManager = new ResourceManager("VSTrac.CommandBar", Assembly.GetExecutingAssembly());
                    CultureInfo cultureInfo = new CultureInfo(_applicationObject.LocaleID);

                    if (cultureInfo.TwoLetterISOLanguageName == "zh")
                    {
                        System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
                        resourceName = String.Concat(parentCultureInfo.Name, "View");
                    }
                    else
                    {
                        resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "View");
                    }
                    toolsMenuName = resourceManager.GetString(resourceName);
                }
                catch
                {
                    //We tried to find a localized version of the word Tools, but one was not found.
                    //  Default to the en-US word, which may work for the current culture.
                    toolsMenuName = "View";
                }

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                //Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "TracExplorer", "Trac Explorer", "Opens The Trac Explorer Window", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((command != null) && (toolsPopup != null))
                    {
                        command.AddControl(toolsPopup.CommandBar, 1);
                    }
                }
                catch (System.ArgumentException)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                }
            }
        }
        #endregion

        #region Query Status
        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "VSTrac.Connect.TracExplorer")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }
        #endregion

        #region Exec
        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "VSTrac.Connect.TracExplorer")
                {
                    // create the window

                    CreateExplorerWindow();

                    handled = true;
                    return;
                }
            }
        }
        #endregion

        #region Create Windows
        internal void CreateTicketWindow(ServerDetails serverDetails, TicketQueryDefinition ticketDef)
        {
            Windows2 windows2 = (Windows2)_applicationObject.Windows;
            Assembly asm = Assembly.GetExecutingAssembly();

            object customControl = null;
            string className = "VSTrac.TicketView";
            string caption = ticketDef.Name;
            string guid = Guid.NewGuid().ToString("B");


            Window toolWindow = windows2.CreateToolWindow2(_addInInstance, asm.Location, className,
                                                     caption, guid, ref customControl);

            if (customControl != null)
            {
                ((TicketView)customControl).VSTracConnect = this;
                ((TicketView)customControl).ServerDetails = serverDetails;
                ((TicketView)customControl).TicketDefinition = ticketDef;
                ((TicketView)customControl).RunQuery();
            }

            toolWindow.Visible = true;
        }

        private void CreateExplorerWindow()
        {
            if (_explorerWindow != null)
            {
                _explorerWindow.Activate();
            }
            else
            {
                Windows2 windows2 = (Windows2)_applicationObject.Windows;
                Assembly asm = Assembly.GetExecutingAssembly();

                object customControl = null;
                string className = "VSTrac.TracExplorer";
                string caption = "Trac Explorer";
                _explorerWindow = windows2.CreateToolWindow2(_addInInstance, asm.Location, className,
                                                         caption, _explorerWindowGuid, ref customControl);

                if (customControl != null)
                {
                    ((TracExplorer)customControl).VSTracConnect = this;
                }

                _explorerWindow.Visible = true;
            }
        }
        #endregion

        #region Static Stuff
        public void OpenBrowser(string url)
        {
            _applicationObject.ExecuteCommand("View.WebBrowser", url);
        }
        #endregion

        #region IDTExtensibility2 Members
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
        }
        #endregion
    }
}