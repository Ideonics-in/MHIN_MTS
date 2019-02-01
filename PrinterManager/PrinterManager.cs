using HDC_COMMSERVER;
using prjParagon_WMS;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Printer
{
    public class PrinterManager
    {
        #region PublicVariables

        public String CombinationTemplate { get; set; }
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
        public String CombinationPrinterName { get; set;}
        public String TemplatePath { get; set; }

        #endregion


        Dictionary<String,BcilNetwork> Drivers;
        #region Constructor
        public PrinterManager(String templatePath)
        {

            Drivers = new Dictionary<string, BcilNetwork>();
            TemplatePath = templatePath;
            
        }
        #endregion

        #region Public Methods

        public void SetupDriver(String name, IPAddress ipaddr, int port, String template)
        {
            BcilNetwork Driver = new BcilNetwork {
                            PrinterIP = ipaddr, 
                            PrinterPort = port,
                            Template = template};
            
                Drivers.Add(name, Driver);
        }



        public bool PrintBarcode(String name, String Model,String ModelCode, String date,String serialno)
        {
            try
            {
                if (Drivers.ContainsKey(name))
                {

                    String BarcodeData = File.ReadAllText(Drivers[name].Template);
                    BarcodeData = BarcodeData.Replace("MODEL", Model);
                    BarcodeData = BarcodeData.Replace("B601A>51401010001", ModelCode + ">5" + date + serialno);
                    return Drivers[name].NetworkPrint(BarcodeData);
                }
                return false;


                
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool PrintBarcode(String name, String Model, String ModelCode, String date, String serialno, String Template)
        {
            try
            {
                if (Drivers.ContainsKey(name))
                {

                    String BarcodeData = File.ReadAllText(Template);
                    BarcodeData = BarcodeData.Replace("MODEL", Model);
                    BarcodeData = BarcodeData.Replace("B601A>51401010001", ModelCode + ">5" + date + serialno);
                    return Drivers[name].NetworkPrint(BarcodeData);
                }
                return false;



            }
            catch (Exception e)
            {
                return false;
            }
        }

        

       

        


        public bool PrintSMN(String PrinterName, String barCode,String template)
        {
            try
            {
                String CombStickerData = File.ReadAllText(TemplatePath + "M1.prn");
                CombStickerData = CombStickerData.Replace("B601A1401010001", barCode);
                


                return clsPrint.SendStringToPrinter(PrinterName, CombStickerData);
            }
            catch
            {
                return false;
            }
        }



        #endregion
    }
}
