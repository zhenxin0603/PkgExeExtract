using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PkgExeExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            string ExeFileName = @"D:\TelpoFaceServer\TelpoFaceServer.exe";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"[^']*'([\d]+)[ ]*'[^']*");
            using (StreamReader br = new StreamReader(ExeFileName, Encoding.ASCII, false))
            {
                var tmp = br.BaseStream.ScanAOB(Encoding.ASCII.GetBytes("PAYLOAD_POSITION"));
                tmp.First();
                long PAYLOAD_POSITION = long.Parse(reg.Replace(br.ReadLine(), "$1"));
                long PAYLOAD_SIZE = long.Parse(reg.Replace(br.ReadLine(), "$1"));
                long PRELUDE_POSITION = long.Parse(reg.Replace(br.ReadLine(), "$1"));
                long PRELUDE_SIZE = long.Parse(reg.Replace(br.ReadLine(), "$1"));

                br.BaseStream.Seek(PRELUDE_POSITION, SeekOrigin.Begin);
                br.DiscardBufferedData();

                string tt = @"{""D:\\snapshot";
                tmp = br.BaseStream.ScanAOB(Encoding.ASCII.GetBytes(tt));
                tmp.First();
                br.BaseStream.Seek(-tt.Length, SeekOrigin.Current);
                br.DiscardBufferedData();
                string pre = br.ReadToEnd();
                foreach (Match m in new Regex(@"""([^""]*)"":{(""\d"":\[\d+,\d+],{0,1})+}").Matches(pre))
                {
                    JObject obj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject("{" + m.Value + "}");
                    JToken o = obj.First;
                    string name = o.First.Path.Replace("['", string.Empty).Replace("']", string.Empty).Replace("\\\\", "\\");

                    if (!name.Contains("node_modules"))
                    {
                        JToken n = o.First.SelectToken("1");
                        if (n != null)
                        {
                            long pos = n.First.Value<long>();
                            int size = n.Last.Value<int>();

                            byte[] buff = new byte[size];
                            br.BaseStream.Seek(PAYLOAD_POSITION + pos, SeekOrigin.Begin);
                            br.DiscardBufferedData();
                            br.BaseStream.Read(buff, 0, size);

                            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(name)))
                            {
                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(name));
                            }

                            File.WriteAllBytes(name, buff);
                        }
                    }
                }

                Console.WriteLine(PAYLOAD_POSITION);
            }
        }
    }
}
