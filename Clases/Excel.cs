using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;


namespace ParseClientes.Clases
{
    class Excel
    {
        public string path { get; set; }
       

        public DataSet excelToDataset(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DataSet nuevo = new DataSet();
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                    {
                        var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };

                        nuevo = reader.AsDataSet(conf);
                    }
                }
            }
            catch
            {

            }

            return nuevo;
        }
    }
}
