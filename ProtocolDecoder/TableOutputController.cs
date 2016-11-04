using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolDecoder
{
    class TableOutputController
    {
        private static DataTable table = new DataTable();

        public static DataTable GetDataTable()
        {
            return table;
        }  

        public static void Clear()
        {
            table.Clear();
        }


        //修改排版只需要修改这一个函数里的分隔符

        private static void AddRow(string string1, string string2)
        {
            DataRow row = table.NewRow();
            row[0] = string1;
            row[1] = string2;
            table.Rows.Add(row); 
        }

        public static void Format(byte[] bytes, string name, string value)
        {
            if(value == null||value.Length==0)
            {
                Format(bytes, name);
            }
            else
            {
                Format(bytes, name + ": " + value);
            }
        }

        public static void Format(string string1)
        {
            AddRow("", string1);
        }

        public static void Format(string string1, string string2)
        {
            AddRow("", string1 + ": " + string2);
        }

        public static void Format(byte value, string result)
        {
            AddRow(String.Format("{0:X2}", value), result);
        }

        public static void Format(byte[] bytes, string result, bool oneline = true)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            
            if (bytes.Length == 1)
            {
                Format(bytes[0], result);
                return;
            }

            if (oneline)
            {
                AddRow(BitConverter.ToString(bytes).Replace("-", null), result);
                return;
            }
            
            for (int i = 0; i < bytes.Length; i++)
            {
                Format(bytes[i],String.Format("{0}[{1}]", result, i));
            }
            
        }

        public static void Format(byte value, List<string> list)
        {
            if (list.Count() == 0)
            {
                return;
            }

            Format(value, list[0]);

            for (int i = 1; i < list.Count(); i++)
            {
                Format(list[i]);
            }
        }


        public static void Format(byte[] bytes, List<string> list)
        {
            if (list.Count() == 0|| bytes == null || bytes.Length==0)
            {
                return;
            }
            Format(bytes, list[0]);

            for (int i = 1; i < list.Count(); i++)
            {
                Format(list[i]);
            }
        }


    }
}
