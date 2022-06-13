using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RestAPI.Model;
using RestAPI.Cadman;
using System.Linq;
using System.IO;

namespace RestAPI
{
    [DisallowConcurrentExecution]
    public class CadmanJob : IJob
    {
        private string DEFAULT_MATERIAL = "CRS";

        private string searchMaterial(string material)
        {
            if (string.IsNullOrEmpty(material))
            {               
                return DEFAULT_MATERIAL;
            }
            if(material.Contains("5052 ALU")|| material.Contains("ALU 5052"))
            {
                return "AL-5052";
            }
            if (material.Contains("5086 ALU") || material.Contains("ALU 5086"))
            {
                return "AL-5086";
            }
            if (material.Contains("6061 ALU") || material.Contains("ALU 6061"))
            {
                return "AL-6061";
            }
            if (material.Contains("BRASS"))
            {
                return "BRASS";
            }
            if (material.Contains("COPPER"))
            {
                return "COPPER";
            }
            if (material.Contains("CRS"))
            {
                return "CRS";
            }
            if (material.Contains("DC01"))
            {
                return "DC01";
            }
            if (material.Contains("DX51D+Z"))
            {
                return "DX51D+Z";
            }
            if (material.Contains("GALV"))
            {
                return "GALV";
            }
            if (material.Contains("GALANNEAL"))
            {
                return "GNEAL";
            }
            if (material.Contains("HRS"))
            {
                return "HRS";
            }
            if (material.Contains("G50"))
            {
                return "HRS-G50";
            }
            if (material.Contains("GRADE"))
            {
                return "HRS-G70";
            }

            if (material.Contains("GRADE 70") || material.Contains("70 GRADE"))
            {
                return "HRS-G70";
            }
            if (material.Contains("S235J2"))
            {
                return "S235J2";
            }
            if (material.Contains("S235JR"))
            {
                return "S235JR";
            }
            if (material.Contains("2B FINISH") || material.Contains("FINISH 2B"))
            {
                return "SS-304-2B";
            }
            if (material.Contains("GRAINED FINISH") || material.Contains("FINISH GRAINED"))
            {
                return "SS-304-GRN";
            }           
            return DEFAULT_MATERIAL;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var services = new ServiceCollection();

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            services.AddDbContextPool<RestAPIContext>(options =>
            options.UseSqlServer(dataMap.GetString("ConnectionString")));
           
            //add cadman connection
            services.AddDbContextPool<CadmanContext>(options =>
            options.UseSqlServer(dataMap.GetString("CadmanConnectionString")));

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<RestAPIContext>();

            var dbContextCadman = serviceProvider.GetService<CadmanContext>();
           
            List<CadmanPO> list = new List<CadmanPO>();
            string logFolder = dataMap.GetString("LogFolder");
            StreamWriter outputFile = new StreamWriter(logFolder+"//cadman.txt", append: false);
            try
            {
                DateTime date = new DateTime(2022, 5, 27, 0, 0, 0);

                //get list from database
                var jobs = from order in dbContext.Orders
                            join release in dbContext.Releases on order.OrderNo equals release.OrderNo
                            join orderDet in dbContext.OrderDet on release.JobNo equals orderDet.JobNo
                          
                            from m in dbContext.Materials
                           .Where(o => release.PartNo == o.PartNo)
                            .Take(1)
                           .DefaultIfEmpty()

                            where orderDet.Status != "Closed" && orderDet.MasterJobNo == null
                            && release.DueDate !=null
                            && release.DateComplete == null
                            && release.DelType != 2 && order.DateEnt >= date
                            &&(release.Cadman_Generated == null
                                  || release.Cadman_Generated == false)
                            orderby release.CreatedDate descending
                            select new
                            {
                                release.Releases_ID,
                                orderDet.OrderNo,
                                orderDet.JobNo,
                                orderDet.PartNo,
                                release.Qty,
                                Unit = "",
                                Comments = "",
                                release.DueDate,
                                Material = m.Descrip
                            };

                //for testing
                //jobs = jobs.Where(a => a.JobNo.Equals("50930-01"));

                outputFile.WriteLine(DateTime.Now + "-" + "Count:" + jobs.Count());

                string partDownloadFolder = dataMap.GetString("PartDownloadFolder");
                string project = dataMap.GetString("ProjectName");

                if (jobs != null)
                {
                    foreach (var po in jobs)
                    {
                        try
                        {
                            //inteligent search material. Material must exist in cadman resource
                            string material = po.Material;
                            material = searchMaterial(material);
                                                       
                            if (string.IsNullOrEmpty(material))
                            {
                                outputFile.WriteLine("Ignore invalid material, order no:" + po.OrderNo + ", material:" + material);

                                continue;
                            }
                            
                            //search drawing file in m-files and download to local folder
                            string partNo = po.PartNo;
                            string partDrawingFile =  MFileUtil.DownloadDrawingFile(dataMap, partNo, partDownloadFolder);
                            if (string.IsNullOrEmpty(partDrawingFile))
                            {
                                outputFile.WriteLine("Part drawing file does not exist, order no:" + po.OrderNo + ", part:"+ partNo);

                                continue;
                            }

                            outputFile.WriteLine("Creating cadman object for order no " + po.OrderNo);

                            CadmanPO cadmanPO = new CadmanPO();
                            cadmanPO.PODet_ID = po.Releases_ID;

                            List<PartType> partTypeList = new List<PartType>();

                            PartType partType = new PartType();
                            partType.ID = po.PartNo;
                            partType.MaterialID = material;
                            partType.Unit = "INCH";
                            partType.ImportFileName = partDownloadFolder + "\\" + partDrawingFile;

                            partTypeList.Add(partType);

                            cadmanPO.PartTypeList = partTypeList;
                          
                            List<ProductionOrder> productionOrderList = new List<ProductionOrder>();
                            ProductionOrder productionOrder = new ProductionOrder();
                            productionOrder.ID = po.OrderNo;
                            productionOrder.Project = project;
                            productionOrder.PartID = po.PartNo;
                            productionOrder.Quantity = po.Qty;
                            productionOrder.Comment = po.Comments;
                            productionOrder.Priority = 1;

                            List<Operation> operationList = new List<Operation>();

                            if (po.DueDate != null)
                            {
                                Operation op1 = new Operation();
                                op1.Kind = "BENDING";
                                op1.DueDate = new DueDate();
                                op1.DueDate.Year = po.DueDate.Value.Year;
                                op1.DueDate.Month = po.DueDate.Value.Month;
                                op1.DueDate.Day = po.DueDate.Value.Day;

                                operationList.Add(op1);
                                Operation op2 = new Operation();
                                op2.DueDate = new DueDate();
                                op2.Kind = "2DLASER";
                                op2.DueDate.Year = po.DueDate.Value.Year;
                                op2.DueDate.Month = po.DueDate.Value.Month;
                                op2.DueDate.Day = po.DueDate.Value.Day;
                                operationList.Add(op2);
                            }
                          
                            productionOrder.OperationList = operationList;

                            productionOrderList.Add(productionOrder);
                            cadmanPO.ProductionOrderList = productionOrderList;

                            list.Add(cadmanPO);
                        }
                        catch (Exception ex1)
                        {
                            outputFile.WriteLine(ex1.Message + po.OrderNo);
                        }
                    }
                }

                //write part xml to cadman SDI watch folder
                CadmanUtil.OutputCreatePartXml(dataMap, list, dbContext);

                //write PO xml to queue
                List<CadmanPO> filteredList = new List<CadmanPO>();
                foreach (var job in list)
                {
                    bool partExist = false;
                    foreach (var part in job.PartTypeList)
                    {
                        var a = dbContextCadman.PartTemplateRevision.Where(a => a.Name.Equals(part.ID) && a.NameUnique).FirstOrDefault();
                        if (a != null)
                        {
                            partExist = true;
                        }
                    }
                    if (partExist)
                    {
                        filteredList.Add(job);
                    }
                }
               
                CadmanUtil.OutputCreatePOXml(dataMap, filteredList, dbContext);

                //process PO in queue
                CadmanUtil.ProcessQueue(dataMap, dbContext);

            }catch(Exception ex)
            {
                outputFile.WriteLine(ex.Message);
            }
            finally
            {
                outputFile.Close();
            }
            return Task.CompletedTask;
        }
    }
}
