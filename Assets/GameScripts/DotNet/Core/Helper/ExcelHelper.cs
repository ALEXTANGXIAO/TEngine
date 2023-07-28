#if TENGINE_NET
using OfficeOpenXml;

namespace TEngine.Helper;

public static class ExcelHelper
{
    public static ExcelPackage LoadExcel(string name)
    {
        return new ExcelPackage(name);
    }
    
    public static string GetCellValue(this ExcelWorksheet sheet, int row, int column)
    {
        ExcelRange cell = sheet.Cells[row, column];
            
        try
        {
            if (cell.Value == null)
            {
                return "";
            }

            string s = cell.GetValue<string>();
                
            return s.Trim();
        }
        catch (Exception e)
        {
            throw new Exception($"Rows {row} Columns {column} Content {cell.Text} {e}");
        }
    }
}
#endif