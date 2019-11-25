using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA
{
  public   class valiballecommon
    {
        private static readonly valiballecommon storage = new valiballecommon();

        public static valiballecommon GetStorage()
        {
            return storage;
        }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string Permission { get; set; }
        public string _version { get; set; }
        public string value1 { get; set; }
        public string value2 { get; set; }
        public string value3 { get; set; }
        public string value4{ get; set; }



        //class Storage_Static
        //{
        //    public static string Username { get; set; }
        //}
    }
}
