using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DataAccess;
using Microsoft.Win32;
using OposScanner_CCO;
using Printer;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Part> Parts;
        ObservableCollection<FGPart> FGParts;
        ObservableCollection<Part> DisplayParts;
        ObservableCollection<Part> DisplayFGParts;



        Part CurrentPart;

        PrinterManager PrinterManager;
        string templatePath = String.Empty;
        String PrinterName = String.Empty; 


        public MainWindow()
        {
            InitializeComponent();
            Parts = new ObservableCollection<Part>();
            FGParts = new ObservableCollection<FGPart>();
            DisplayParts = new ObservableCollection<Part>();
            DisplayFGParts = new ObservableCollection<Part>();
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            select p;

                foreach (Part p in parts)
                {
                    p.ImageReference = "./Images/" + p.PartNo + ".png";
                    Parts.Add(p);
                }
                   

                var fgparts = from p in db.FGParts
                            select p;

                foreach (FGPart p in fgparts)
                {
                    p.ImageReference = "./Images/" + p.PartNo + ".png";
                    FGParts.Add(p);
                }
                  
            }
            PickList_PartComboBox.DataContext = Parts;
            Line_PartComboBox.DataContext = Parts;
            Track_PartComboBox.DataContext = Parts;
            Scrap_PartNoCombobox.DataContext = Parts;
            FG_PartComboBox.DataContext = FGParts;
            Track_FGPartComboBox.DataContext = FGParts;

            templatePath = ConfigurationSettings.AppSettings["TEMPLATE_PATH"];

            PrinterName =  ConfigurationSettings.AppSettings["BARCODE_PRINTER_NAME"];

            PrinterManager = new Printer.PrinterManager(templatePath);
            PrinterManager.CombinationPrinterName = PrinterName;
            PrinterManager.CombinationTemplate = "M1.prn";
        }

        #region PICK_LIST
        private void PickList_SUNTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PickList_SUNTextBox.Text.Length == 14)
            {
               // PickList_PartComboBox.SelectedIndex = -1;
                PickList_PartComboBox.Focus();
            }


        }

       

        private void PickList_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPart = PickList_PartComboBox.SelectedItem as Part;
            if (CurrentPart == null) return;
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            where p.PartNo == CurrentPart.PartNo
                            select p;
                var fs = from s in db.FromStores
                         select s;
                Part P = parts.SingleOrDefault();
                if (P == null)
                {
                    MessageBox.Show("Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (PickList_SUNTextBox.Text == String.Empty && P.Kanban == false)
                {
                    MessageBox.Show("Please Scan/Enter SUN", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (PickList_SUNTextBox.Text.Length !=10 && (PickList_SUNTextBox.Text.Length != 14) )
                {
                    MessageBox.Show("SUN Not Equal to expected number of characters. Please Veifry ", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                int qty = 0;
                if(int.TryParse(PickList_QtyTextBox.Text,out qty) == false || (qty == 0))
                {
                    MessageBox.Show("Please Verify Quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                    
                FromStores f = new FromStores();
                f.SUN = PickList_SUNTextBox.Text;
                f.Part = P;
                f.Quantity = qty;
                f.Balance = qty;
                f.Timestamp = DateTime.Now;

                db.FromStores.Add(f);


                P.Quantity += f.Quantity.Value ;
                P.LastUpdated = DateTime.Now;
                P.LastActivity = "Stores Inward Entry";
                P.FromStoresRecords.Add(f);

                db.SaveChanges();

                DisplayParts.Clear();
                DisplayParts.Add(P);
                Stores_PartGrid.DataContext = null;
                Stores_PartGrid.DataContext = DisplayParts;

                MessageBox.Show("Part Added To Inventory", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             //PickList_PartComboBox.SelectedIndex = -1;
                             PickList_QtyTextBox.Clear();
                             PickList_SUNTextBox.Clear();
                             DisplayParts.Clear();
                             PickList_SUNTextBox.Focus();
                         }));
            }
        }

        private void PickList_PartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox PartsCombobox = (ComboBox)sender;
            DisplayParts.Clear();
            CurrentPart = (Part)PartsCombobox.SelectedItem;
            if (CurrentPart == null) return;
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            where p.PartNo == CurrentPart.PartNo
                            select p;

                Part P = parts.SingleOrDefault();


                if (P == null)
                {
                    MessageBox.Show("Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentPart = P;
                CurrentPart.ImageReference = "./Images/" + CurrentPart.PartNo + ".png";
                DisplayParts.Add(CurrentPart);
                PickList_StoresToSMImage.DataContext = CurrentPart;

                Stores_PartGrid.DataContext = null;
                Stores_PartGrid.DataContext = DisplayParts;
            }
           
        }

        private void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Stores_PartGrid.DataContext = null;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                       new Action(() =>
                       {
                           PickList_PartComboBox.Text = "";
                           PickList_PartComboBox.SelectedItem  = null;
                           PickList_QtyTextBox.Clear();
                           PickList_SUNTextBox.Clear();

                           Line_PartComboBox.Text = "";
                           Line_PartComboBox.SelectedItem = null;
                           Line_ReleaseQtyTextBox.Clear();

                           Line_TrackingCodeTextBox.Clear();
                           Line_ReturnQtyTextBox.Clear();

                           Scrap_PartNoCombobox.Text = "";
                           Scrap_PartNoCombobox.SelectedItem = null;
                           Scrap_QtyTextBox.Clear();
                           Scrap_ReasonTextBox.Clear();

                       }));
        }

        #endregion

        #region LINE
        private void GenerateTrackingCodeButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPart = (Part)Line_PartComboBox.SelectedItem;
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            where p.PartNo == CurrentPart.PartNo
                            select p;
                
                Part P = parts.SingleOrDefault();


                if (P == null)
                {
                    MessageBox.Show("Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                int qty = 0;
                if (int.TryParse(Line_ReleaseQtyTextBox.Text, out qty) == false || (qty == 0))
                {
                    MessageBox.Show("Please Verify Quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                CurrentPart = P;

                if(P.Quantity < qty)
                {
                    MessageBox.Show("Quantity Unavailable", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                ToLine tl = new ToLine();
                
                foreach (FromStores fs in P.FromStoresRecords)
                {
                    if (qty > 0)
                    {
                        if (fs.Balance <= 0) continue;
                        if (fs.Balance >= qty)
                        {
                            var FromStoresToLines = new FromStoresToLines();
                            FromStoresToLines.FromStores = fs;
                            FromStoresToLines.ToLine = tl;
                            FromStoresToLines.FromStoresBalance = fs.Balance.Value;

                            fs.Balance -= qty;
                            P.Quantity -= qty;

                            tl.Quantity += qty;
                            tl.Timestamp = DateTime.Now;
                            tl.Part = P;
                            if ((tl.SMN == String.Empty) || tl.SMN == null)
                            {
                                DateTime ts = DateTime.Now;
                                int offset = 0;
                                if (ts.Hour >= 22 && ts.Hour < 6) offset = 1;
                                P.ToLineReference++;
                                tl.SMN = P.PartID.ToString("D4") + P.ToLineReference.Value.ToString("D4")
                                    + (ts.DayOfYear - offset).ToString("D3") + (ts.Year - 2000).ToString("D2");
                            }
                            tl.SUN = fs.SUN;

                            FromStoresToLines.ToLineBalance = qty;
                            FromStoresToLines.Part = P;
                            db.FromStoresToLines.Add(FromStoresToLines);
                            break;
                        }
                        else
                        {
                            var FromStoresToLines = new FromStoresToLines();
                            FromStoresToLines.FromStores = fs;
                            FromStoresToLines.ToLine = tl;
                            FromStoresToLines.FromStoresBalance = fs.Balance.Value;

                            tl.Quantity += fs.Balance;
                            tl.Timestamp = DateTime.Now;
                            tl.Part = P;


                            if ((tl.SMN == String.Empty) || tl.SMN == null)
                            {
                                DateTime ts = DateTime.Now;
                                int offset = 0;
                                if (ts.Hour >= 22 && ts.Hour < 6) offset = 1;
                                
                                    P.ToLineReference++;
                                tl.SMN = P.PartID.ToString("D4") + P.ToLineReference.Value.ToString("D4") 
                                    + (ts.DayOfYear- offset).ToString("D3") + (ts.Year -2000).ToString("D2");
                            }

                            tl.SUN = fs.SUN;
 
                            qty -= fs.Balance.Value;
                            FromStoresToLines.ToLineBalance = fs.Balance.Value;
                            P.Quantity -= fs.Balance.Value;
                            fs.Balance = 0;
                            FromStoresToLines.Part = P;
                            db.FromStoresToLines.Add(FromStoresToLines);
                            

                        }
                    }
                }
                tl.Balance = tl.Quantity;
                tl.Timestamp = DateTime.Now;
                
                P.ToLineRecords.Add(tl);
                P.LastUpdated = DateTime.Now;
                P.LastActivity = "To Line Entry";
                db.SaveChanges();

                //Code for generation of Tracking Code


               // PrinterManager.PrintSMN(PrinterName, tl.SMN, templatePath);
                MessageBox.Show("SMN Generated - " + tl.SMN, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             Line_PartComboBox.Text = "";
                             Line_PartComboBox.SelectedItem = null;
                             Line_ReleaseQtyTextBox.Clear();
                             
                             DisplayParts.Clear();
                             
                         }));
            }
        }

        private void Line_PartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox PartsCombobox = (ComboBox)sender;
            DisplayParts.Clear();
            CurrentPart = (Part)PartsCombobox.SelectedItem;
            if (CurrentPart == null) return;
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            where p.PartNo == CurrentPart.PartNo
                            select p;

                Part P = parts.SingleOrDefault();

                P.LineQuantity = 0;
                foreach(ToLine t in P.ToLineRecords)
                {
                    P.LineQuantity += t.Balance.Value;
                }
                if (P == null)
                {
                    MessageBox.Show("Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentPart = P;
                CurrentPart.ImageReference = "./Images/" + CurrentPart.PartNo + ".png";
                DisplayParts.Add(CurrentPart);


                Stores_PartGrid.DataContext = null;
                Stores_PartGrid.DataContext = DisplayParts;
            }
        }

        private void Line_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new MTSDB())
            {

                var tl = (from t in db.ToLine
                          where t.SMN == Line_TrackingCodeTextBox.Text
                          select t).SingleOrDefault();

                if (tl == null)
                {
                    MessageBox.Show("Part Not Found!! Please Verify", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Line_TrackingCodeTextBox.Clear();
                    return;
                }

                CurrentPart = tl.Part;

                int qty = 0;
                if (int.TryParse(Line_ReturnQtyTextBox.Text, out qty) == false || (qty == 0))
                {
                    MessageBox.Show("Please Verify Quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                FromLine fl = new FromLine();
                fl.ToLineRecord = tl;
                fl.Quantity = qty;
                fl.Part = tl.Part;
                fl.Timestamp = DateTime.Now;
                tl.Balance -= qty; 
                tl.Part.Quantity += qty;
                tl.Part.FromLineRecords.Add(fl);
                db.SaveChanges();

                MessageBox.Show("Inventory Updated", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            Line_ReturnQtyTextBox.Clear();
                            Line_TrackingCodeTextBox.Clear();
                            DisplayParts.Clear();
                            Stores_PartGrid.DataContext = null;
                            Stores_PartGrid.DataContext = DisplayParts;
                            
                        }));

            }
        }

        private void Line_TrackingCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if(tb.Text.Length == 13 )
            {
                using (var db = new MTSDB())
                {

                    var tl = (from t in db.ToLine
                              where t.SMN == Line_TrackingCodeTextBox.Text
                              select t).SingleOrDefault();

                    if (tl == null)
                    {
                        MessageBox.Show("Part Not Found!! Please Verify", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Line_TrackingCodeTextBox.Clear();
                        return;
                    }


                    CurrentPart = tl.Part;
                    CurrentPart.ImageReference = "./Images/" + CurrentPart.PartNo + ".png";
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            DisplayParts.Clear();
                            DisplayParts.Add(CurrentPart);
                            Stores_PartGrid.DataContext = null;
                            Stores_PartGrid.DataContext = DisplayParts;
                            Line_ReturnQtyTextBox.Focus();
                        }));
            }
        }

        #endregion

        #region FG
        private void FGPart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoCompleteBox a = sender as AutoCompleteBox;
            DisplayParts.Clear();
            FGPart part = (FGPart)a.SelectedItem;
            if (part == null) return;
            using (var db = new MTSDB())
            {
                var P = db.FGParts.Where(p => p.PartNo == part.PartNo).SingleOrDefault();


                

                if (P == null)
                {
                    MessageBox.Show(" Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                part = P;


                part.ImageReference = "./Images/" + part.PartNo + ".png";
                FGImage.DataContext = part;

            }

        }

        private void FG_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            FG_ShortageGrid.Visibility = Visibility.Collapsed;
                        }));
            List<ShortagePart> shortageParts = new List<ShortagePart>();
            if (FG_PartComboBox.SelectedItem == null) return;
            FGPart fgpart = (FGPart)FG_PartComboBox.SelectedItem;
            if (fgpart == null) return;

            String FGRef = FG_RefTextBox.Text;
            if(FGRef == String.Empty || FGRef == null)
            {
                MessageBox.Show("FG Reference Required !!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(FG_AcceptedTextBox.Text != fgpart.PartNo)
            {
                MessageBox.Show("FG Part No Mismatch. Please Verify ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new MTSDB())
            {

                var FGEntry = (from p in db.FGs
                               where p.FGRef == FGRef
                               select p).SingleOrDefault();
                if (FGEntry != null)
                {
                    MessageBox.Show("FG Already Completed. Please Verify Reference No" , "Update Error", MessageBoxButton.OK , MessageBoxImage.Error);
                    return;
                }
                var fgparts = from p in db.FGParts
                            where p.PartNo == fgpart.PartNo
                            select p;

                FGPart FGP = fgparts.SingleOrDefault();


                if (FGP == null)
                {
                    MessageBox.Show("FG Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int qty = 0;
                if (int.TryParse(FG_QuantityTextBox.Text, out qty) == false || (qty == 0))
                {
                    MessageBox.Show("Please Verify Quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                var bom = (from p in db.BOMs
                           where p.FGPartID == fgpart.FGPartID
                           select p).ToList() ;

                DateTime? tlTs = DateTime.Now;
                int shift = 0;
                if (tlTs.Value.TimeOfDay >= new TimeSpan(14, 00, 0) && tlTs.Value.TimeOfDay < new TimeSpan(22, 00, 0))
                    shift = 2;
                else if (tlTs.Value.TimeOfDay >= new TimeSpan(22, 00, 0) && tlTs.Value.TimeOfDay < new TimeSpan(6, 00, 0))
                    shift = 3;
                else shift = 1;

                foreach (BOM b in bom)
                {
                    FG fgEntry = new FG();
                    fgEntry.FGPart = FGP;
                    fgEntry.EntryTimestamp = tlTs;
                    fgEntry.JulianDate = tlTs.Value.DayOfYear;
                    var part = (from prt in db.Parts
                                where prt.PartID == b.PartID
                                select prt).SingleOrDefault();
                    fgEntry.Part = part;
                    fgEntry.Shift = shift;
                    fgEntry.Quantity =(int) (b.PartQuantity.Value * qty);
                    fgEntry.FGRef = FGRef;

                    var toline = (from tl in part.ToLineRecords
                                  where (tl.Timestamp <= tlTs) && tl.Balance > 0
                                  select tl).OrderBy(l => l.Timestamp).ToList();

                    int? FGQty = fgEntry.Quantity;
                    int totalPartQuantity = 0;

                    foreach(ToLine l in toline)
                    {
                        totalPartQuantity += l.Balance.Value;
                    }

                    if(totalPartQuantity < FGQty)
                    {
                        shortageParts.Add(new ShortagePart
                        {
                            PartNo = fgEntry.Part.PartNo,
                            Description = fgEntry.Part.Description,
                            LineQuantity = totalPartQuantity,
                            RequiredQuantity = (FGQty - totalPartQuantity).Value
                        });


                      
                    }
                    
                    foreach (ToLine l in toline)
                    {
                        if (FGQty > 0)
                        {
                            if (l.Balance >= FGQty)
                            {
                                l.Balance-= FGQty;
 
                                l.FGs.Add(fgEntry);

                                break;
                            }
                            else
                            {

                                FGQty -= l.Balance;
                                l.Balance = 0;
                                l.FGs.Add(fgEntry);

                            }
                        }
                    }
                    part.FGRecords.Add(fgEntry);
       
                }
                if(shortageParts.Count > 0 )
                {
                    FG_ShortageGrid.DataContext = shortageParts;
                    MessageBox.Show("Parts Shortage for FG Update" + Environment.NewLine +
                            "Unable to Update FG ,  Please Verify",
                            "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            FG_ShortageGrid.Visibility = Visibility.Visible;
                        }));

                    return;
                }
                

                FG Fg = new FG();
                Fg.FGPart = FGP;
                Fg.EntryTimestamp = tlTs;
                Fg.JulianDate = tlTs.Value.DayOfYear;
                Fg.Shift = shift;
                Fg.Quantity = qty;
                Fg.FGRef = FG_RefTextBox.Text;
                db.FGs.Add(Fg);
                db.SaveChanges();

                MessageBox.Show("FG Updated", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
        private void FG_RefTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FG_RefTextBox.Text.Length == 10)
            {
                FG_PartComboBox.Text = "";
                FG_PartComboBox.SelectedItem = null;
                FG_PartComboBox.Focus();
            }
        }

        #endregion

        #region TRACK
        private void Track_FGUnitButton_Click(object sender, RoutedEventArgs e)
        {
            String FGUnitNumber = Track_FGUnitNumber.Text;
            if (FGUnitNumber == String.Empty || (FGUnitNumber == null))
            {
                MessageBox.Show("Please enter FG Unit Number", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            int JulianDate = Convert.ToInt16(FGUnitNumber.Substring(0, 3));
            int year = Convert.ToInt16(FGUnitNumber.Substring(3, 2));
            int shift = Convert.ToInt16(FGUnitNumber.Substring(5, 1));

            String PartNo = String.Empty, FGPartNo = String.Empty;

            if(Track_PartComboBox.SelectedIndex != -1 )
                PartNo = ((Part)Track_PartComboBox.SelectedItem).PartNo;
            if (Track_FGPartComboBox.SelectedIndex != -1)
                FGPartNo = ((FGPart)Track_FGPartComboBox.SelectedItem).PartNo;

            if (PartNo == string.Empty && (FGPartNo == String.Empty))
            {
                MessageBox.Show("FG Part / Child Part Not Found, Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            using (var db = new MTSDB())
            {
                            
                
                
                List<FG> FGs;

                if (PartNo != string.Empty && FGPartNo == null)
                {
                    FGs = db.FGs.Include("ToLines").Where(f => f.JulianDate == JulianDate && f.Shift == shift)
                        .Where(f => f.ToLines.Any(t => t.Part.PartNo == PartNo)).ToList();
                }
                else if (PartNo == string.Empty && FGPartNo != string.Empty)
                {
                    FGs = db.FGs.Include("ToLines")
                        .Where(f => (f.JulianDate == JulianDate) && (f.Shift == shift) && (f.FGPart.PartNo == FGPartNo)).ToList();

                }
                else
                {
                    FGs = db.FGs.Include("ToLines")
                        .Where(f => (f.JulianDate == JulianDate) && (f.Shift == shift) && (f.FGPart.PartNo == FGPartNo))
                        .Where(f => f.ToLines.Any(t => t.Part.PartNo == PartNo)).ToList();
                }    


                    ObservableCollection<FromStores> fromStores = new ObservableCollection<FromStores>();
                    ObservableCollection<TrackPartEntry> trackParts = new ObservableCollection<TrackPartEntry>();

                    foreach (FG f in FGs)
                    {
                        foreach (ToLine t in f.ToLines)
                        {
                            var trackEntries = (from track in db.FromStoresToLines
                                                where track.ToLine.ToLineID == t.ToLineID
                                                select track).ToList();

                            foreach (FromStoresToLines te in trackEntries)
                            {

                                trackParts.Add(new TrackPartEntry
                                {
                                    SUN = te.FromStores.SUN,
                                    ExistingQuantity = te.FromStoresBalance,
                                    ReceivedOn = te.FromStores.Timestamp,
                                    SMN = te.ToLine.SMN,
                                    IssuedOn = te.ToLine.Timestamp,
                                    IssuedQuantity = te.ToLineBalance,
                                    FGPartNo = f.FGPart.PartNo,
                                    FGQty = f.Quantity.Value,
                                    DispatchedOn = f.EntryTimestamp.Value,
                                    FGReference = f.FGRef

                                });
                            }

                        }
                    }

                    Track_Grid.DataContext = trackParts;
                }

                
             
                
                

            
        }

        

        private void Track_PartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Track_PartImage.DataContext = null;
            ComboBox PartsCombobox = (ComboBox)sender;
            DisplayParts.Clear();
            CurrentPart = (Part)PartsCombobox.SelectedItem;
            if (CurrentPart == null) return;
            using (var db = new MTSDB())
            {
                var parts = from p in db.Parts
                            where p.PartNo == CurrentPart.PartNo
                            select p;

                Part P = parts.SingleOrDefault();


                if (P == null)
                {
                    MessageBox.Show("Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentPart = P;
                CurrentPart.ImageReference = "./Images/" + CurrentPart.PartNo + ".png";
                DisplayParts.Add(CurrentPart);
                Track_PartImage.DataContext = CurrentPart;

                Stores_PartGrid.DataContext = null;
                Stores_PartGrid.DataContext = DisplayParts;
            }
        }

        private void Track_FGPartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Track_FGPartImage.DataContext = null;
            ComboBox PartsCombobox = (ComboBox)sender;
            DisplayParts.Clear();
            FGPart fgpart = (FGPart)PartsCombobox.SelectedItem;
            if (fgpart == null) return;
            using (var db = new MTSDB())
            {
                var parts = from p in db.FGParts
                            where p.PartNo == fgpart.PartNo
                            select p;

                FGPart P = parts.SingleOrDefault();


                if (P == null)
                {
                    MessageBox.Show("FG Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fgpart = P;
                fgpart.ImageReference = "./Images/" + fgpart.PartNo + ".png";

                Track_FGPartImage.DataContext = fgpart;

            }
        }

        #endregion


        #region  INVENTORY
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ObservableCollection<Part> Inventory = new ObservableCollection<Part>();
            TabControl tc = sender as TabControl;
            TabItem ti = tc.SelectedItem as TabItem;
            if (ti.Name == "InventoryTab")
            {
                using (var db = new MTSDB())
                {
                    var parts = (from p in db.Parts.Include("ToLineRecords")
                                select p).ToList();

                    foreach (Part p in parts)
                    {
                        p.LineQuantity = 0;
                        var tl = p.ToLineRecords.ToList();
                        foreach (ToLine t in tl)
                            p.LineQuantity += t.Balance.Value; 
                        Inventory.Add(p);
                    }
                        
                }
                Inventory_PartGrid.DataContext = Inventory;
            }

        }

        private void Inventory_ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV (.csv)|*.csv";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    String filename = dlg.FileName;
                    FileInfo report = new FileInfo(filename);
                    StreamWriter sw = report.CreateText();

                    sw.Write("Part No,Description,Quantity,Location" + Environment.NewLine);
                    using (var db = new MTSDB())
                    {
                        var parts = from p in db.Parts
                                    select p;

                        foreach (Part p in parts)
                        {
                            String reportEntry = p.PartNo + "," + p.Description + "," + p.Quantity + "," 
                                + p.LineQuantity +"," + p.Location;

                            sw.Write(reportEntry);
                            sw.Write(Environment.NewLine);
                        }



                    }
                    sw.Close();
                    MessageBox.Show("Report Generation Successful", "Report Generation Message",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Error Generating Report" + Environment.NewLine + exp.Message, "Report Generation Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        #endregion

        #region REPORTS

        private void ReportGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Report_FGGrid.Visibility = Visibility.Collapsed;
            Report_CPGrid.Visibility = Visibility.Collapsed;
            ObservableCollection<ReportEntry> Report;
            switch(Reports_TypeCombobox.SelectedIndex )
            {
                case 0:
                    Report_FGGrid.DataContext = GenerateFGReport(dpFrom.SelectedDate.Value, dpTo.SelectedDate.Value);
                Report_FGGrid.Visibility = Visibility.Visible;
                    break;

                case 1:
                    Report_CPGrid.DataContext = GenerateCPReport(dpFrom.SelectedDate.Value, dpTo.SelectedDate.Value);
                    Report_CPGrid.Visibility = Visibility.Visible;
                    break;

                case 2:
                    Report_CPGrid.DataContext = GenerateCPReport(dpFrom.SelectedDate.Value, dpTo.SelectedDate.Value);
                    Report_CPGrid.Visibility = Visibility.Visible;
                    break;

            }
            
        }
        
        private void ReportExportButton_Click(object sender, RoutedEventArgs e)
        {

        }


        ObservableCollection<ReportEntry> GenerateFGReport(DateTime fromts, DateTime to)
        {
            ObservableCollection<ReportEntry> report= new ObservableCollection<ReportEntry>();
            fromts = new DateTime(fromts.Year, fromts.Month, fromts.Day, 6, 0, 0);
            to = to.AddDays(1);
            to = new DateTime(to.Year, to.Month, to.Day, 6, 0, 0);
            using (var db = new MTSDB())
            {

                var FGs = db.FGs.Where(p => (p.EntryTimestamp >= fromts) && (p.EntryTimestamp < to) && (p.Part == null)).ToList();
                foreach (FG f in FGs)
                {
                    report.Add(new ReportEntry
                    {
                        Timestamp = f.EntryTimestamp,
                        FGRef = f.FGRef,
                        PartNo = f.FGPart.PartNo,
                        Quantity = f.Quantity.Value

                    });
                }

            }
                return report;
        }


        ObservableCollection<ReportEntry> GenerateCPReport(DateTime fromts, DateTime to)
        {
            ObservableCollection<ReportEntry> report = new ObservableCollection<ReportEntry>();

            fromts = new DateTime(fromts.Year, fromts.Month, fromts.Day, 6, 0, 0);
            to = to.AddDays(1);
            to = new DateTime(to.Year, to.Month, to.Day, 6, 0, 0);
            using (var db = new MTSDB())
            {

                var toLines = db.ToLine.Include("FromStoresToLines").Where(p => (p.Timestamp >= fromts) && (p.Timestamp < to) ).ToList();
                foreach (ToLine t in toLines)
                {
                    report.Add(new ReportEntry
                    {
                        Timestamp = t.Timestamp,
                        SMN = t.SMN,
                        PartNo = t.Part.PartNo,
                        Quantity = t.Quantity.Value,
                        SUN = t.SUN


                    });
                }

            }

            return report;
        }


        #endregion

        #region SCRAP

        private void Scrap_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Scrap_PartNoCombobox.SelectedItem == null) return;
            CurrentPart = (Part)Scrap_PartNoCombobox.SelectedItem;

           

            int qty = 0;
            if (int.TryParse(Scrap_QtyTextBox.Text, out qty) == false || (qty == 0))
            {
                MessageBox.Show("Please Verify Quantity", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            using (var db = new MTSDB())
            {
                var part = db.Parts.Where(p => p.PartID == CurrentPart.PartID).SingleOrDefault();
                

                if (part == null)
                {
                    MessageBox.Show("Part Not Found!! Please Verify", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Line_TrackingCodeTextBox.Clear();
                    return;
                }

                var tl = db.ToLine.Include("Part").Where(t => t.Part.PartID == part.PartID && t.Balance > 0).OrderBy(t => t.Timestamp).ToList();

                int lineQty = 0;
                foreach (ToLine t in tl)
                {
                    lineQty += t.Balance.Value;

                }
                if(lineQty < qty)
                {
                    MessageBox.Show("Line Quantity less than Scrap Quantity. Please Verify", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Line_TrackingCodeTextBox.Clear();
                    return;
                }


                LineRejection fl = new LineRejection();
                fl.Part = part ;
                fl.Reason = Scrap_ReasonTextBox.Text;
                fl.Timestamp = DateTime.Now;

                foreach (ToLine t in tl)
                {
                    if (qty > 0)
                    {
                        if (t.Balance > qty)
                        {
                            t.Balance -= qty;
                            fl.Quantity += qty;
                            fl.ToLines.Add(t);
                            break;
                        }
                        else
                        {
                            qty -= t.Balance.Value;
                            fl.Quantity += t.Balance.Value;
                            t.Balance = 0;
                            fl.ToLines.Add(t);
                        }
                    }
                }



                db.LineRejections.Add(fl);
                db.SaveChanges();

                MessageBox.Show("Scrap Updated", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            Scrap_PartNoCombobox.Text = "";
                            Scrap_PartNoCombobox.SelectedItem = null;
                            Scrap_QtyTextBox.Clear();
                            Scrap_ReasonTextBox.Clear();

                        }));

            }
        }
        #endregion

        private void Part_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            AutoCompleteBox a = sender as AutoCompleteBox;
            DisplayParts.Clear();
            Part part = (Part)a.SelectedItem;
            if (part == null) return;
            using (var db = new MTSDB())
            {
                var P = db.Parts.Include("ToLineRecords").Include("LineRejections").Where(p => p.PartID == part.PartID).SingleOrDefault();


                P.UpdateLineRejectionQuantity();
                
                if (P == null)
                {
                    MessageBox.Show(" Part Not Found !! Please Verify", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                part = P;


                DisplayParts.Add(P);
                Stores_PartGrid.DataContext = DisplayParts;

            }
            
        }
    }

    public class TrackPartEntry
    {
        public String FGReference { get; set; }
        public String FGPartNo { get; set; }
        public int FGQty { get; set; }
        public DateTime DispatchedOn { get; set; }
        public String SUN { get; set; }
        public int ExistingQuantity { get; set; }
        public DateTime ReceivedOn { get; set; }
        public String SMN { get; set; }
        public int IssuedQuantity {get;set;}
        public DateTime IssuedOn { get; set; }

    }

    public class ShortagePart
    {
        public String PartNo { get; set; }
        public String Description { get; set; }
        public int LineQuantity { get; set; }
        public int RequiredQuantity { get; set; }
      

    }

    public class ReportEntry
    {
        public DateTime? Timestamp { get; set; }
        public String PartNo { get; set; }
        public int Quantity { get; set; }
        public String FGRef { get; set; }
        public String SUN { get; set; }
        public String SMN { get; set; }
    }


}
