//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Xml.Linq;
//using Dynamic.Core.IO;
//using System.IO;

//namespace Dynamic.Core.Xml
//{
//    public class XmlTranslater
//    {
//        private List<XmlConverter> cacheConverter = new List<XmlConverter>();

//        private FileWatcher fileWatcher = null;

//        private Func<String, String> convertValue = null;

//        private XmlConverterCompiler compiler = null;

//        public XmlTranslater()
//            : this(null)
//        {
//        }

//        public XmlTranslater(Func<String,String> convertValue)
//        {
//            this.convertValue = convertValue;
//            compiler = new XmlConverterCompiler();
//            fileWatcher = new FileWatcher();
//            fileWatcher.ScanNotify += new EventHandler<ScanManagerEventArgs>(fileWatcher_ScanNotify);
//            fileWatcher.Start();
//        }

//        public void AddComputerAssembly(String assemblyFile)
//        {
//            compiler.ComputerRefrenceAssembly.Add(assemblyFile);
//        }

//        public void AddComputerNamespace(String ns)
//        {
//            compiler.ComputerNamespaces.Add(ns);
//        }

//        public void AddTesterAssembly(String assemblyFile)
//        {
//            compiler.TesterRefrenceAssembly.Add(assemblyFile);
//        }

//        public void AddTesterNamespace(String ns)
//        {
//            compiler.TesterNamespaces.Add(ns);
//        }

//        void fileWatcher_ScanNotify(object sender, ScanManagerEventArgs e)
//        {
//            if (e.FolderList != null)
//            {
//                List<String> cs = e.FolderList.Select(x => x.PluginID).Distinct().ToList();
//                lock (cacheConverter)
//                {
//                    foreach (string id in cs)
//                    {
//                        var c = cacheConverter.FirstOrDefault(x => x.ID == id);
//                        if (c != null)
//                        {
//                            cacheConverter.Remove(c);
//                        }
//                    }

//                }
//            }
//        }

//        public bool Convert(XElement source, string ruleFile, object computeContext)
//        {
//            XmlConverter converter = cacheConverter.FirstOrDefault(x => x.RuleFile.Equals(ruleFile, StringComparison.OrdinalIgnoreCase));
//            if (converter == null)
//            {
//                lock (cacheConverter)
//                {
//                    converter = cacheConverter.FirstOrDefault(x => x.RuleFile.Equals(ruleFile, StringComparison.OrdinalIgnoreCase));
//                    if (converter == null)
//                    {
//                        converter = new XmlConverter(ruleFile, compiler);
//                        cacheConverter.Add(converter);

//                        //自动监视文件修改
//                        foreach (String file in converter.RelativeFiles)
//                        {
//                            WatchFolder wf = new WatchFolder();
//                            wf.CreatedNotify = true;
//                            wf.DeletedNotify = true;
//                            wf.Enable = true;
//                            wf.ModifiedNotify = true;
//                            wf.RenameNotify = true;
//                            wf.Path = Path.GetDirectoryName(file);
//                            wf.Fiter = Path.GetFileName(file);
//                            wf.PluginID = converter.ID;
//                            fileWatcher.AddFolder(wf);
//                        }

                        
//                    }
//                }
//            }

//            return converter.Convert(source, convertValue, computeContext);
//        }
//    }
//}
