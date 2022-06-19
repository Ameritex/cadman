using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MFilesAPI;
using System.IO;
using RestAPI.Model;
using System.Globalization;

namespace RestAPI
{
    public class MFileUtil
    {       
        private static string UserName = "osd-app";
        private static string Password = "!Gone@2211!";
        private static string Domain = "amtx";
 
        private static int ModelOrDrawingClassId = 17;

        public static string DownloadDrawingFile(JobDataMap dataMap, string partNo, string downloadPath)
        {
            string logFolder = dataMap.GetString("LogFolder");

            StreamWriter outputFile = new StreamWriter(logFolder + "//cadman_download_parts.txt", append: true);

            try
            {
                // Instantiate an MFilesServerApplication object.       
                var mfServerApplication = new MFilesServerApplication();

                mfServerApplication.Connect(
                MFAuthType.MFAuthTypeSpecificWindowsUser,
                UserName: UserName,
                Password: Password,
                Domain: Domain,
                ProtocolSequence: "ncacn_ip_tcp", // Connect using TCP/IP.
                NetworkAddress: "SQL002", // prod
                //NetworkAddress: "AMTX-TEST", // UAT

                Endpoint: "2266"); // Connect to the default port (2266).

                var vault = mfServerApplication.LogInToVault(dataMap.GetString("Vault"));

                // Connect using the default authentication details, specifying the server details.
                //outputFile.WriteLine("Connected");

                var searchConditions = new SearchConditions();

                // Add a "not deleted" filter.
                {
                    var condition = new SearchCondition();
                    condition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                    condition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);

                    searchConditions.Add(-1, condition);
                }

                // Add a "drawing class" filter.
                {
                    var condition = new SearchCondition();
                    condition.Expression.SetPropertyValueExpression((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
                        MFParentChildBehavior.MFParentChildBehaviorNone);
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                    condition.TypedValue.SetValue(MFilesAPI.MFDataType.MFDatatypeLookup,
                            ModelOrDrawingClassId);

                    searchConditions.Add(-1, condition);
                }

                {
                    var condition = new SearchCondition();
                    condition.Expression.SetPropertyValueExpression(
                        dataMap.GetInt("PartNoObjectId"),
                        MFParentChildBehavior.MFParentChildBehaviorNone);

                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;

                    condition.TypedValue.SetValue(MFilesAPI.MFDataType.MFDatatypeText, partNo);

                    searchConditions.Add(-1, condition);
                }

                // Execute the search.
                var results = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions,
                MFSearchFlags.MFSearchFlagNone, SortResults: false);

                var list = new List<ObjectVersion>();
                if (results != null && results.Count > 0)
                {
                    list = results.Cast<ObjectVersion>().ToList();
                }
                outputFile.WriteLine("list.Count:" + list.Count + ",partNo : " + partNo);


                if (list.Count > 0)
                {
                    foreach (ObjectVersion objectVesion in list)
                    {
                        var objectFiles = vault.ObjectFileOperations.GetFiles(objectVesion.ObjVer)
                        .Cast<ObjectFile>()
                        .ToArray();

                        // Iterate over the files and download each in turn.
                        foreach (var objectFile in objectFiles)
                        {
                            outputFile.WriteLine(objectFile.Title + "." + objectFile.Extension.ToUpper());

                            if (!objectFile.Extension.ToUpper().Equals("SLDPRT") ||
                                !(objectFile.Title.ToUpper() + "-").Contains(partNo.ToUpper() + "-"))
                            {
                                continue;
                            }
                            if (!objectFile.Extension.ToUpper().Equals("STEP") ||
                                !(objectFile.Title.ToUpper() + "-").Contains(partNo.ToUpper() + "-"))
                            {
                                continue;
                            }
                            if (!objectFile.Extension.ToUpper().Equals("SLDASM") ||
                                !(objectFile.Title.ToUpper() + "-").Contains(partNo.ToUpper() + "-"))
                            {
                                continue;
                            }
                            outputFile.WriteLine("downloadPath:" + downloadPath + "\\" + partNo + "." + objectFile.Extension);

                            // Download the file to a temporary location.
                            vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, downloadPath + "\\" + partNo + "." + objectFile.Extension);

                            return partNo + "." + objectFile.Extension;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outputFile.WriteLine(ex.Message);
            }
            finally
            {
                outputFile.Close();
            }


            return null;
        }

    }


}
