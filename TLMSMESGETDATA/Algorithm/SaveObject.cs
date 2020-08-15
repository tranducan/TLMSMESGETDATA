using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
namespace TLMSMESGETDATA.Algorithm
{
    
 public static   class SaveObject
    {
        public static string Pathsave = AppDomain.CurrentDomain.BaseDirectory + "\\setting.ini";
        public static bool Save_data(string filename, object model)
        {
            string[] files = filename.Split("\\".ToCharArray());
            string _FileName = "";
            for (int i = 0; i < files.Length; i++)
            {
                if (i < files.Length - 1)
                    _FileName += files[i] + "\\";
                else
                    _FileName += "temp-"+files[i];
            }

            FileStream fs;
            try
            {
                fs = new FileStream(_FileName, FileMode.Create);
            }
            catch { return false; }


            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, model);
                fs.Close();
                File.Copy(_FileName, filename, true);
                File.Delete(_FileName);
                return true;
            }
            catch (Exception ex)
            {
                fs.Close();
                Console.Write(ex.Message);

                return false;
            }
        }

        public static object Load_data(string filename)
        {
            string[] files = filename.Split("\\".ToCharArray());
            string _FileName = "";
            for (int i = 0; i < files.Length; i++)
            {
                if (i < files.Length - 1)
                    _FileName += files[i] + "\\";
                else
                    _FileName +=  files[i];
            }

            if (filename == "") return null;
            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Open);

            }
            catch { return null; }
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                fs.Seek(0, SeekOrigin.Begin);
                object TS = formatter.Deserialize(fs);
                fs.Close();
                if (TS != null)
                    return TS;
            }
            catch (Exception EX)
            {
                fs.Close();
                Console.Write(EX.Message);
            }

            // Mo file du phong.
            try
            {
                fs = new FileStream(_FileName, FileMode.Open);
            }
            catch { return null; }
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                object TS = formatter.Deserialize(fs);
                fs.Close();
                if (TS != null)
                    return TS;
            }
            catch
            {
                fs.Close();
            }
            return null;
        }
    }
}
