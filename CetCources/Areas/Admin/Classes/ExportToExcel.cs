using CetCources.Database;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
namespace Exporter
{
    public static class ExportTo
    {
        public static string ExcelFileOfficeDll(IEnumerable<object> data)
        {
            try
            {
                var excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.Workbooks.Add();
                Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;

                // set column headings
                var columnNames = new Dictionary<int, string>();
                var anyItem = data.FirstOrDefault();
                var ColumnNo = 1;
                foreach (var item in anyItem.GetType().GetProperties())
                {
                    columnNames.Add(ColumnNo, item.Name);
                    workSheet.Rows.Cells[1, ColumnNo] = item.Name.Replace("_", " ");
                    workSheet.Cells[1, ColumnNo].Interior.Color 
                        = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightBlue);
                    ColumnNo++;
                }

                // set data
                var row = 1;
                foreach (var item in data)
                {
                    row++;
                    foreach (var column in columnNames)
                    {
                        var val = item.GetType().GetProperty(column.Value).GetValue(item);
                        if (val == null) val = "";
                        workSheet.Rows.Cells[row, column.Key] = val;
                    }
                }

                foreach (var column in columnNames)
                {
                    workSheet.Columns[column.Key].AutoFit();
                }

                //workSheet.Range["A1", ColumnNo.ToString() + row.ToString()]
                //    .AutoFormat(Excel.XlRangeAutoFormat.xlRangeAutoFormatSimple);
                
                var guid = Guid.NewGuid().ToString().Replace("-", "");
                
                //var path = $"{System.IO.Path.GetTempPath()}{guid}.xlsx";
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}{guid}.xlsx";
                workSheet.SaveAs(path, Excel.XlFileFormat.xlWorkbookDefault);
                Marshal.ReleaseComObject(workSheet);
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
                return path;
            }
            catch(Exception ex)
            {
                throw ex;
                //return "";
            }
        }


        public static byte[] ExcelFileNP(IEnumerable<object> data)
        {

            IWorkbook workbook = new HSSFWorkbook();

            ISheet sheet = workbook.CreateSheet("CET Export Data");

            //make a header row  
            IRow row1 = sheet.CreateRow(0);
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.BorderBottom = BorderStyle.Medium;
            headerStyle.FillBackgroundColor = HSSFColor.LightBlue.Index;

            var columnNames = new Dictionary<int, string>();
            var anyItem = data.FirstOrDefault();
            var ColumnNo = 0;
            if (anyItem != null)
            {
                foreach (var item in anyItem.GetType().GetProperties())
                {
                    columnNames.Add(ColumnNo, item.Name);
                    ICell cell = row1.CreateCell(ColumnNo);
                    string columnName = item.Name.Replace("_", " ");
                    cell.SetCellValue(columnName);
                    cell.CellStyle = headerStyle;
                    ColumnNo++;
                }



                // set data
                var rowNo = 1;
                foreach (var item in data)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    foreach (var column in columnNames)
                    {
                        ICell cell = row.CreateCell(column.Key);
                        var val = item.GetType().GetProperty(column.Value).GetValue(item);
                        if (val == null) val = "";
                        cell.SetCellValue(val.ToString());
                    }
                    rowNo++;

                }

                foreach (var column in columnNames)
                {
                    sheet.AutoSizeColumn(column.Key);
                }
            }
            else
            {
                ICell cell = row1.CreateCell(0);
                cell.SetCellValue("NO DATA FOUND!");
                cell.CellStyle = headerStyle;
                sheet.AutoSizeColumn(0);
            }
            var exportData = new MemoryStream();
            workbook.Write(exportData);
            
            return exportData.ToArray();
        }
    }
}