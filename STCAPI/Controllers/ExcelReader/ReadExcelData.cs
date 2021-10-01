﻿using CommonHelper;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STCAPI.Core.Entities.Enums;
using STCAPI.Core.Entities.VATDetailUpload;
using STCAPI.Core.ViewModel.RequestModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STCAPI.Controllers.ExcelReader
{
    [Route("api/[controller]/[controller]")]
    [ApiController]
    public class ReadExcelData : ControllerBase
    {
        private readonly IHostingEnvironment _IHostingEnviroment;

        public ReadExcelData(IHostingEnvironment hostingEnvironment)
        {
            _IHostingEnviroment = hostingEnvironment;
        }
        [HttpPost]
        public async Task<IActionResult> UploadExcelData([FromForm] InvoiceDetail model)
        {
            try
            {
                IDictionary<int, (string, string)> errorResult = new Dictionary<int, (string, string)>();
                var attachmentList = await new BlobHelper().UploadDocument(model.AttachmentList, _IHostingEnviroment);
                var invoiceFiles = new List<IFormFile>() { model.InvoiceExcelFile };

                var uploadFileDetails = await new BlobHelper().UploadDocument(invoiceFiles, _IHostingEnviroment);


                switch (model.ExcelType)
                {
                    case VATExcelType.InputDataFile:
                        var inputVATModel = await Task.Run(() => InputVATExcelData(model.InvoiceExcelFile));
                        errorResult = InputVATvalidationRule.ValidateInputVatData(inputVATModel);
                        break;
                    case VATExcelType.OutputDataFile:
                        var outputVATModel = await Task.Run(() => OutputVATExcelFle(model.InvoiceExcelFile));
                        break;
                    case VATExcelType.VATReturnDataFile:
                        var returnModel = await Task.Run(() => InputVATExcelData(model.InvoiceExcelFile));
                        break;
                    case VATExcelType.VATTrialBalanceDataFile:
                        var balanceDataModel = await Task.Run(() => VATTrialBalance(model.InvoiceExcelFile));
                        break;
                }
              

                return await Task.Run(() => Ok($"{attachmentList},{uploadFileDetails}"));
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
            }
            return await Task.Run(() => Ok(""));
        }

        private List<InputVATFileVm> InputVATExcelData(IFormFile inputFile)
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            string message = string.Empty;
            Stream stream = inputFile.OpenReadStream();
            List<InputVATFileVm> models = new List<InputVATFileVm>();

            try
            {
                if (inputFile != null)
                {
                    if (inputFile.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else if (inputFile.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    else
                        message = "The file format is not supported.";

                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();

                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                    {
                        DataTable inputVatInvoiceDetail = dsexcelRecords.Tables[0];

                        for (int i = 2; i < inputVatInvoiceDetail.Rows.Count; i++)
                        {
                            var model = new InputVATFileVm();
                            model.InvoiceType = Convert.ToString(inputVatInvoiceDetail.Rows[i][0]);
                            model.InvoiceSource = Convert.ToString(inputVatInvoiceDetail.Rows[i][1]);
                            model.InvoiceNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][2]);
                            model.InvoiceDocNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][3]);
                            model.InvoiceDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][4]);
                            model.GLDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][5]);
                            model.TotalInvoiceAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][6]);
                            model.InvoiceCurrency = Convert.ToString(inputVatInvoiceDetail.Rows[i][7]);
                            model.CurrencyExchangeRate = Convert.ToString(inputVatInvoiceDetail.Rows[i][8]);
                            model.SARInvoiceAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][9]);
                            model.SuppierNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][10]);
                            model.SupplierName = Convert.ToString(inputVatInvoiceDetail.Rows[i][11]);
                            model.SupplierSite = Convert.ToString(inputVatInvoiceDetail.Rows[i][12]);
                            model.SupplierAddress = Convert.ToString(inputVatInvoiceDetail.Rows[i][13]);
                            model.SupplierCountry = Convert.ToString(inputVatInvoiceDetail.Rows[i][14]);
                            model.SupplierBankAccount = Convert.ToString(inputVatInvoiceDetail.Rows[i][15]);
                            model.SupplierVATRegistrationNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][16]);
                            model.SupplierVATGroupRegistrationNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][17]);
                            model.InvoiceStatus = Convert.ToString(inputVatInvoiceDetail.Rows[i][18]);
                            model.PaymentStatus = Convert.ToString(inputVatInvoiceDetail.Rows[i][19]);

                            model.PaymentAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][20]);
                            model.PaymentMethod = Convert.ToString(inputVatInvoiceDetail.Rows[i][21]);
                            model.PaymentTerm = Convert.ToString(inputVatInvoiceDetail.Rows[i][22]);
                            model.InvoiceLineNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][23]);
                            model.InvoiceLineDescription = Convert.ToString(inputVatInvoiceDetail.Rows[i][24]);
                            model.PONumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][25]);
                            model.PoDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][26]);
                            model.ReleaseDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][27] ?? DateTime.Now.AddYears(100));
                            model.ReceiptNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][28]);
                            model.PoItemNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][29]);
                            model.PoItemDescription = Convert.ToString(inputVatInvoiceDetail.Rows[i][30]);
                            model.InvoiceSource = Convert.ToString(inputVatInvoiceDetail.Rows[i][31]);
                            model.Quantity = Convert.ToString(inputVatInvoiceDetail.Rows[i][32]);
                            model.UnitPrice = Convert.ToString(inputVatInvoiceDetail.Rows[i][33]);
                            model.DiscountAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][34]);
                            model.DiscountPercentage = Convert.ToString(inputVatInvoiceDetail.Rows[i][35]);
                            model.ContractNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][36]);
                            model.ContractStartDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][37] ?? DateTime.Now.AddYears(100));
                            model.ContractEndDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][38] ?? DateTime.Now.AddYears(100));
                            model.OriginalInvoiceNumberForDebitCreditNote = Convert.ToString(inputVatInvoiceDetail.Rows[i][39]);

                            model.TaxLineNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][40]);
                            model.ProductType = Convert.ToString(inputVatInvoiceDetail.Rows[i][41]);
                            model.TaxCode = Convert.ToString(inputVatInvoiceDetail.Rows[i][42]);
                            model.TaxRateCode = Convert.ToString(inputVatInvoiceDetail.Rows[i][43]);
                            model.TaxRate = Convert.ToString(inputVatInvoiceDetail.Rows[i][44]);
                            model.TaxableAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][45]);
                            model.SARTaxableAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][46]);
                            model.RecoverableTaxableAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][47]);
                            model.SARRecoverableTaxableAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][48]);
                            model.NonRecoverableTaxAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][49]);
                            model.SARNonRecoverableTaxAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][50]);
                            models.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }

            return models;
        }

        private List<OutPutVATModel> OutputVATExcelFle(IFormFile inputFile)
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            string message = string.Empty;
            Stream stream = inputFile.OpenReadStream();
            List<OutPutVATModel> models = new List<OutPutVATModel>();

            try
            {
                if (inputFile != null)
                {
                    if (inputFile.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else if (inputFile.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    else
                        message = "The file format is not supported.";

                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();

                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                    {
                        DataTable inputVatInvoiceDetail = dsexcelRecords.Tables[0];

                        for (int i = 2; i < inputVatInvoiceDetail.Rows.Count; i++)
                        {
                            var model = new OutPutVATModel();
                            model.InvoiceNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][0]);
                            model.InvoiceDocSequence = Convert.ToString(inputVatInvoiceDetail.Rows[i][1]);
                            model.InvoiceSource = Convert.ToString(inputVatInvoiceDetail.Rows[i][2]);
                            model.InvoiceType = Convert.ToString(inputVatInvoiceDetail.Rows[i][3]);
                            model.InvoiceDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][4]);
                            model.GLDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][5]);
                            model.InvoiceAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][6]);
                            model.InvoiceCurrency = Convert.ToString(inputVatInvoiceDetail.Rows[i][7]);
                            model.CurrencyExchangeRate = Convert.ToString(inputVatInvoiceDetail.Rows[i][8]);
                            model.SARInvoiceAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][9]);
                            model.CustomerName = Convert.ToString(inputVatInvoiceDetail.Rows[i][10]);
                            model.BillToAdress = Convert.ToString(inputVatInvoiceDetail.Rows[i][11]);
                            model.CustomerCountryName = Convert.ToString(inputVatInvoiceDetail.Rows[i][12]);
                            model.CustomerVATRegistrationNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][13]);
                            model.SellerNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][14]);
                            model.SellerVATRegistrationNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][15]);
                            model.SellerAddress = Convert.ToString(inputVatInvoiceDetail.Rows[i][16]);
                            model.GroupVARRegistrationNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][17]);
                            model.SellerCommercialNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][18]);
                            model.InvoiceLineNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][19]);

                            model.InvoiceLineDescription = Convert.ToString(inputVatInvoiceDetail.Rows[i][20]);
                            model.IssueDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][21]);
                            model.Quantity = Convert.ToString(inputVatInvoiceDetail.Rows[i][22]);
                            model.UnitPrice = Convert.ToString(inputVatInvoiceDetail.Rows[i][23]);
                            model.DiscountAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][24]);
                            model.DiscountPercentage = Convert.ToString(inputVatInvoiceDetail.Rows[i][25]);
                            model.PaymentMethod = Convert.ToString(inputVatInvoiceDetail.Rows[i][26]);
                            model.PaymentTerm = Convert.ToString(inputVatInvoiceDetail.Rows[i][27] ?? DateTime.Now.AddYears(100));
                            model.InvoiceLineAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][28]);
                            model.SARInvoiceLineAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][29]);
                            model.TaxRateName = Convert.ToString(inputVatInvoiceDetail.Rows[i][30]);
                            model.TaxableAmount  = Convert.ToString(inputVatInvoiceDetail.Rows[i][1]);
                            model.SARTaxableAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][31]);
                            model.TaxAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][32]);
                            model.SARTaxAmount = Convert.ToString(inputVatInvoiceDetail.Rows[i][33]);
                            model.TaxClassificationCode = Convert.ToString(inputVatInvoiceDetail.Rows[i][34]);
                            model.TaxRate = Convert.ToString(inputVatInvoiceDetail.Rows[i][35]);
                            model.TaxAccount = Convert.ToString(inputVatInvoiceDetail.Rows[i][36] ?? DateTime.Now.AddYears(100));
                            model.ContractNumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][37] ?? DateTime.Now.AddYears(100));
                            model.ContractDescription = Convert.ToString(inputVatInvoiceDetail.Rows[i][38]);

                            model.ContractStartDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][39]);
                            model.ContractEndDate = Convert.ToString(inputVatInvoiceDetail.Rows[i][40]);
                            model.OriginalInvoice = Convert.ToString(inputVatInvoiceDetail.Rows[i][41]);
                            model.PONumber = Convert.ToString(inputVatInvoiceDetail.Rows[i][42]);
                            model.UniversalUniqueInvoiceIdentifier = Convert.ToString(inputVatInvoiceDetail.Rows[i][43]);
                            model.QRCode = Convert.ToString(inputVatInvoiceDetail.Rows[i][44]);
                            model.PreviousInvoiceNoteHash = Convert.ToString(inputVatInvoiceDetail.Rows[i][45]);
                            model.InvoiceTamperResistantCounterValue = Convert.ToString(inputVatInvoiceDetail.Rows[i][46]);
                            models.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }

            return models;
        }

        private List<VATTrialBalanceModel> VATTrialBalance(IFormFile inputFile)
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            string message = string.Empty;
            Stream stream = inputFile.OpenReadStream();
            List<VATTrialBalanceModel> models = new List<VATTrialBalanceModel>();

            try
            {
                if (inputFile != null)
                {
                    if (inputFile.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else if (inputFile.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    else
                        message = "The file format is not supported.";

                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();

                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                    {
                        DataTable inputVatInvoiceDetail = dsexcelRecords.Tables[0];

                        for (int i = 1; i < inputVatInvoiceDetail.Rows.Count; i++)
                        {
                            var model = new VATTrialBalanceModel();
                            model.Account = Convert.ToString(inputVatInvoiceDetail.Rows[i][0]);
                            model.Description = Convert.ToString(inputVatInvoiceDetail.Rows[i][1]);
                            model.BeginingBalance = Convert.ToString(inputVatInvoiceDetail.Rows[i][2]);
                            model.Debit = Convert.ToString(inputVatInvoiceDetail.Rows[i][3]);
                            model.Credit = Convert.ToString(inputVatInvoiceDetail.Rows[i][4]);
                            model.Activity = Convert.ToString(inputVatInvoiceDetail.Rows[i][5]);
                            model.EndingBalance = Convert.ToString(inputVatInvoiceDetail.Rows[i][6]);
                            models.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }

            return models;
        }
    }
}
