using System.Collections.Generic;
using System.IO;

namespace Tangzx.ABSystem
{
    public class AssetBundleDataWriter
    {
        public void SaveMap(string path,AssetTarget[] targets)
        {
            FileStream fs = new FileStream(path, FileMode.CreateNew);
            SaveMap(fs, targets);
        }

        public virtual void SaveMap(Stream stream, AssetTarget[] targets)
        {
            StreamWriter sw = new StreamWriter(stream);
            //写入文件头判断文件类型用，ABDT 意思即 Asset-Bundle-Data-Text
            sw.WriteLine("ABDT");

            for (int i = 0; i < targets.Length; i++)
            {
                AssetTarget target = targets[i];
                //debug name
                sw.WriteLine(target.assetPath);
                //bundle name
                sw.WriteLine(target.bundleName);
                //hash  
                sw.WriteLine(target.bundleCrc);

                sw.WriteLine("<------------->");
            }
            sw.Close();
        }
    }
}