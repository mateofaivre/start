using Start.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Start.UnitTest
{
    public class ZipTest
    {
        public void ZipManyFilesTest()
        {
            var datas = new Dictionary<string, byte[]>();
            datas.Add("a.jpg", null);
            datas.Add("b.jpg", null);
            datas.Add("c.jpg", null);
            datas.Add("d.jpg", null);
            datas.Add("excel.xlsx", null);

            foreach(var name in datas.Keys.ToList())
            {
                datas[name] = GetData(name);
            }

            var zipData = ZipHelper.Zip(datas);
            File.WriteAllBytes(@$"C:\Data\4-Projets\start\import\Start.UnitTest\Files\test.zip", zipData);
        }

        private byte[] GetData(string name)
        {
            byte[] data = null;
            using (var fileStream = File.Open(@$"C:\Data\4-Projets\start\import\Start.UnitTest\Files\{name}", FileMode.Open))
            {
                var ms = new MemoryStream();
                fileStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                data = ms.ToArray();
            }

            return data;
        }
    }
}
