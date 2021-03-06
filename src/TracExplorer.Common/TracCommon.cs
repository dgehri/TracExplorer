﻿#region GPL Licence
/**********************************************************************
 TracExplorer - Trac Integration for Visual Studio and TortoiseSvn
 Copyright (C) 2008 Mladen Mihajlovic
 http://tracexplorer.devjavu.com/
 
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
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CookComputing.XmlRpc;
using System.Windows;

namespace TracExplorer.Common
{
    public static class TracCommon
    {
        static TracCommon()
        {
            // This delegate makes sure that non-validating SSL certificates are passed successfully.
            ServicePointManager.ServerCertificateValidationCallback = delegate(object certsender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
            {
                return true;
            };

            SetAllowUnsafeHeaderParsing();
        }

        /// <summary>
        /// From http://xml-rpc.net/faq/xmlrpcnetfaq.html#2.27
        /// See bug #29
        /// </summary>
        /// <returns></returns>
        public static bool SetAllowUnsafeHeaderParsing()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(
              typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for 
                // the internal class
                Type aSettingsType = aNetAssembly.GetType(
                  "System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance 
                    // of the internal settings class. If the static instance 
                    // isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      BindingFlags.Static | BindingFlags.GetProperty
                      | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the 
                        // framework is unsafe header parsing should be 
                        // allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField(
                          "useUnsafeHeaderParsing",
                          BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns an <see cref="ITrac"/> instance which is connected to a <see cref="ServerDetails"/> object.
        /// </summary>
        /// <param name="serverDetails"></param>
        /// <returns></returns>
        public static ITrac GetTrac(ServerDetails serverDetails)
        {
            ITrac trac = XmlRpcProxyGen.Create<ITrac>();
            trac.Proxy = WebRequest.DefaultWebProxy;
            trac.Url = serverDetails.XmlRpcUrl();

            switch (serverDetails.RequiredAuthentication)
            {
                case AuthenticationTypes.BasicAuthentication:
                    trac.Credentials = new NetworkCredential(serverDetails.Username, serverDetails.Password);
                    break;
                case AuthenticationTypes.IntegratedAuthentication:
                    trac.Credentials = CredentialCache.DefaultNetworkCredentials;
                    break;
                case AuthenticationTypes.ClientCertAuthentication:
                    try
                    {
                        X509Store s = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                        X509Certificate2Collection col;

                        s.Open(OpenFlags.ReadOnly);
                        col = s.Certificates.Find(X509FindType.FindBySubjectName, serverDetails.Username, true);
                        if (col.Count == 1)
                        {
                            trac.ClientCertificates.Add(col[0]);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("No or multiple (" + col.Count + ") certificate with name [" + serverDetails.Username + "] found.");
                        }
                        s.Close();
                    }
                    catch (System.Security.Cryptography.CryptographicException s)
                    {
                        System.Windows.Forms.MessageBox.Show("CryptographicException: " + s.ToString());
                    }
                    catch (System.Security.SecurityException s)
                    {
                        System.Windows.Forms.MessageBox.Show("SecurityException: " + s.ToString());
                    }
                    catch (System.ArgumentException s)
                    {
                        System.Windows.Forms.MessageBox.Show("ArgumentException: " + s.ToString());
                    }
                    break;
                case AuthenticationTypes.None:
                    trac.Credentials = null;
                    break;
            }

            return trac;
        }
    }
}
