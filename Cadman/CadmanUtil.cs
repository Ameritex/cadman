
using Quartz;
using RestAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RestAPI.Cadman
{
    public static class CadmanUtil
    {
        public static void ProcessQueue(JobDataMap dataMap, RestAPIContext dbContext)
        {
            string logFolder = dataMap.GetString("LogFolder");
            string cadmanQueue = dataMap.GetString("CadmanQueueFolder");
            string cadmanWatch = dataMap.GetString("CadmanWatchFolder");

            string cadmanSdiWatch = dataMap.GetString("CadmanSdiWatchFolder");
            string project = dataMap.GetString("ProjectName");

            StreamWriter outputFile = new StreamWriter(logFolder + "//cadman_process.txt", append: false);
            DirectoryInfo di = new DirectoryInfo(cadmanQueue);
            FileInfo[] files = di.GetFiles("*.xml");
          
            foreach (var file in files)
            {
                outputFile.WriteLine(file.FullName);
                XmlDocument doc = new XmlDocument();
                doc.Load(file.FullName);
                int poId = Int32.Parse( file.Name.Replace(project +  "_PO_", "").Replace(".xml",""));
                outputFile.WriteLine("poId:"+ poId);
                try
                {
                    outputFile.WriteLine("Move file to  : "+ cadmanWatch + "\\" + file.Name);
                    File.Copy(file.FullName, cadmanWatch + "\\" + file.Name );
                    file.Delete();
                    outputFile.WriteLine("Move file done");
                    /*
                    var poRec = dbContext.PODet.SingleOrDefault(a => a.PODet_ID == poId);
                    if (poRec != null)
                    {
                        poRec.Cadman_Generated = true;
                        dbContext.Update(poRec);
                        dbContext.SaveChanges();

                        outputFile.WriteLine("Updated " + poId);
                    }
                  */

                }
                catch(Exception ex)
                {
                    outputFile.WriteLine(ex.Message);
                }
                
            }
            outputFile.Close();

        }

        public static void OutputCreatePOXml(JobDataMap dataMap, List<CadmanPO> poList, RestAPIContext dbContext)
        {
            string logFolder = dataMap.GetString("LogFolder");
            string cadmanQueue = dataMap.GetString("CadmanQueueFolder");
            string project = dataMap.GetString("ProjectName");

            StreamWriter outputFile = new StreamWriter(logFolder + "//cadman_queue.txt", append: false);
                                  
            foreach (CadmanPO job in poList)
            {
                try
                {
                    outputFile.WriteLine(cadmanQueue + "/" + project + "_PO_" + job.PODet_ID + ".xml");
                    using (XmlWriter writer = XmlWriter.Create(cadmanQueue + "/" + project + "_PO_" + job.PODet_ID + ".xml"))
                    {
                        writer.WriteStartElement("CADMAN");
                        writer.WriteAttributeString("Version", "1");
                        writer.WriteStartElement("PRODUCTIONORDERIMPORT");
                        writer.WriteAttributeString("Version", "1");
                                            
                        #region "PRODUCTIONORDERLIST"
                        writer.WriteStartElement("PRODUCTIONORDERLIST");
                        foreach (ProductionOrder po in job.ProductionOrderList)
                        {
                            writer.WriteStartElement("PRODUCTIONORDER");
                            writer.WriteAttributeString("ID",project+"_"+ po.ID);

                            writer.WriteElementString("PartID", po.PartID);
                            //writer.WriteElementString("PartRev", po.PartRev);
                            //writer.WriteElementString("PartSubRev", po.PartSubRev);
                            writer.WriteElementString("QUANTITY", po.Quantity.ToString());
                            writer.WriteElementString("PROJECT", po.Project);

                            writer.WriteStartElement("OPERATIONLIST");
                            if (po.OperationList != null)
                            {
                                foreach (Operation op in po.OperationList)
                                {
                                    writer.WriteStartElement("OPERATION");
                                    writer.WriteElementString("KIND", op.Kind);
                                    DueDate dueDate = op.DueDate;
                                    writer.WriteStartElement("DUEDATE");
                                    writer.WriteElementString("YEAR", dueDate.Year.ToString());
                                    writer.WriteElementString("MONTH", dueDate.Month.ToString());
                                    writer.WriteElementString("DAY", dueDate.Day.ToString());
                                    writer.WriteElementString("HOUR", dueDate.Hour.ToString());
                                    writer.WriteElementString("MIN", dueDate.Min.ToString());
                                    writer.WriteElementString("SEC", dueDate.Sec.ToString());
                                    writer.WriteElementString("MSEC", dueDate.MSec.ToString());
                                    writer.WriteElementString("TZ", dueDate.TZ.ToString());

                                    writer.WriteEndElement();
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        #endregion "PRODUCTIONORDERLIST"
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.Flush();

                    }


                    try
                    {
                        var poRec = dbContext.Releases.SingleOrDefault(a => a.Releases_ID == job.PODet_ID);
                        if (poRec != null)
                        {
                            poRec.Cadman_Generated = true;
                            dbContext.Update(poRec);
                            dbContext.SaveChanges();

                            outputFile.WriteLine("Updated " + job.PODet_ID + " to cadman processed.");
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {
                    outputFile.WriteLine(ex.Message);
                }

            }
            outputFile.Close();

        }
     
        public static void OutputCreatePartXml(JobDataMap dataMap, List<CadmanPO> poList, RestAPIContext dbContext)
        {            
            string logFolder = dataMap.GetString("LogFolder");
            string cadmanSdiWatch = dataMap.GetString("CadmanSdiWatchFolder");
            string project = dataMap.GetString("ProjectName");

            StreamWriter outputFile = new StreamWriter(logFolder+"//cadman_parts.txt", append: false);
           
            foreach (CadmanPO job in poList)
            {
                try
                {
                    string path = cadmanSdiWatch + "/Done/" + project + "_Part_" + job.PartTypeList[0].ID + ".xml";
                    if (!File.Exists(path))
                    {
                        using (XmlWriter writer = XmlWriter.Create(cadmanSdiWatch + "/" + project + "_Part_" + job.PartTypeList[0].ID + ".xml"))
                        {
                            writer.WriteStartElement("CADMAN");
                            writer.WriteAttributeString("Version", "1");
                            writer.WriteStartElement("PARTTYPEIMPORT");
                            writer.WriteAttributeString("Version", "1");
                            #region "PARTTYPELIST"
                            writer.WriteStartElement("PARTTYPELIST");
                            foreach (PartType pt in job.PartTypeList)
                            {
                                writer.WriteStartElement("PARTTYPE");
                                writer.WriteAttributeString("ID", pt.ID);
                                writer.WriteElementString("MaterialID", pt.MaterialID);
                                if (!string.IsNullOrEmpty(pt.Thickness))
                                {
                                    writer.WriteElementString("THICKNESS", pt.Thickness);
                                }
                                writer.WriteElementString("UNIT", pt.Unit);
                                writer.WriteElementString("IMPORTFILENAME", pt.ImportFileName);

                                //TAGS
                                writer.WriteStartElement("TAGS");
                                if (pt.Tags != null)
                                {
                                    foreach (Tag tag in pt.Tags)
                                    {
                                        writer.WriteStartElement("TAG");
                                        writer.WriteAttributeString("Type", tag.Type);
                                        writer.WriteString(tag.Value);
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();

                                writer.WriteElementString("COMMENT", pt.Comment);
                                writer.WriteElementString("DESCRIPTION", pt.Description);

                                //DOCUMENTATION
                                writer.WriteStartElement("DOCUMENTATION");
                                if (pt.DocumentList != null)
                                {
                                    foreach (CadmanDocument doc in pt.DocumentList)
                                    {
                                        writer.WriteStartElement("DOCUMENT");
                                        writer.WriteElementString("CATEGORY", doc.Category);
                                        writer.WriteElementString("NAME", doc.Name);
                                        writer.WriteElementString("FILE", doc.File);

                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            #endregion "PARTTYPELIST"

                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.Flush();
                        }
                    }

                }
                catch(Exception ex)
                {
                    outputFile.WriteLine(ex.Message);
                }

            }
            outputFile.Close();

        }
       
        public static void OutputDeletePOXml(JobDataMap dataMap, List<CadmanPO> poList)
        {
            string cadmanWatch = dataMap.GetString("CadmanWatchFolder");
            string project = dataMap.GetString("ProjectName");

            foreach (CadmanPO job in poList)
            {
                using (XmlWriter writer = XmlWriter.Create(cadmanWatch + "/" + project + "_PO_Delete_" + job.PODet_ID + ".xml"))
                {
                    writer.WriteStartElement("CADMAN");                 
                               
                    foreach (ProductionOrder po in job.ProductionOrderList)
                    {
                        writer.WriteStartElement("PRODUCTIONORDERDELETE");                  
                        writer.WriteAttributeString("ID", po.ID);
                        writer.WriteEndElement();
                    }                
                    writer.WriteEndElement();
                    writer.Flush();
                }
            }
        }

    }
}
