using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StreamReaderTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
//            using (FileStream fileStream = new FileStream(@"D:\Code\StreamReader\StreamReader\TestFile.txt"
//, FileMode.Open, FileAccess.Read,FileShare.Read, 1))
//            using (StreamReader sr = new StreamReader(fileStream,Encoding.UTF8, false, 1))
//            {
//                //sr.BaseStream.Seek(70, SeekOrigin.Begin);
//                var line = sr.ReadLine();
//            }

            string xmlString = @"<books>
                                <book>
                                    <title ID=""1"" Name=""Tom"">
                                        <part>Part 1</part>
                                        <part>Part 2</part>
                                        <part>Part 3</part>
                                        <part>Part 4</part>
                                        <part>Part 5</part>
                                    </title>
                                    <author>Author 1</author>
                                </book>
                            </books>";

            using (XmlTextReader reader = new XmlTextReader(new StringReader(xmlString)))
            {
                reader.ReadToDescendant("title");
                reader.ReadToDescendant("part");
                var count = reader.AttributeCount;

                var names = reader.NameTable;
                var dic = new Dictionary<string, string>();
                for (var j = 0; j < count; j++)
                {
                    reader.MoveToAttribute(j);
                    //dic.Add($"Attr{j}", reader.GetAttribute($"Value{j}"));
                    dic.Add(reader.Name, reader.Value);
                }

                Console.WriteLine(reader.ReadInnerXml());
            }
            //TestXElement2(2000000);
            //TestXmlReader(100000);

            TestGetter.TestXmlReaderReflection(100000);
            TestGetter.TestXmlReaderExtractAll(100000);

            Console.ReadLine();
        }
        private static void ProcessTitleElement(XmlNode titleNode)
        {
               if (titleNode != null)
            {
                Console.WriteLine("Processing <title> element:");
                // Process the titleNode as needed, potentially complex logic here
                foreach (XmlNode child in titleNode.ChildNodes)
                {
                    Console.WriteLine($"Part: {child.InnerText}");
                }
            }
        }

        static void TestXElement(int iterations)
        {
            string xml = GetTestXml(100);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < iterations; i++)
            {
                XElement xmlTree = XElement.Parse(xml);
            }

            stopwatch.Stop();
            Console.WriteLine($"XElement test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }
        static void TestXElement2(int iterations)
        {
            string xml = GetTestXml(100);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var list = new List<string>();
            for (int i = 0; i < iterations; i++)
            {
                list.Add(xml);
            }


            stopwatch.Stop();
            Console.WriteLine($"XElement test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }

        static void TestXmlReader2(int iterations)
        {
            string xml = GetTestXml(100);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var list = new List<string>();
            for (int i = 0; i < iterations; i++)
            {
                list.Add(xml);
            }
            for (int i = 0; i < iterations; i++)
            {

                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    while (reader.Read()) { }
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"XmlReader test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }
        static void TestXmlReader(int iterations)
        {
            string xml = GetTestXml(500);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < iterations; i++)
            {

                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    reader.Read();
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"XmlReader test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }

        private static string GetTestXml(int count)
        {
            string xml = "<EmptyElement ";
            for (int j = 0; j < count; j++)
            {
                xml += $"Attr{j}=\"Value{j}\" ";
            }
            xml += "/>";
            return xml;
        }
    }
    public class TestGetter
    {

        private static string GetTestXml(int count)
        {
            string xml = "<EmptyElement ";
            for (int j = 0; j < count; j++)
            {
                xml += $"Attr{j}=\"Value{j}\" ";
            }
            xml += "/>";
            return xml;
        }

        public static void TestXmlReaderReflection(int iterations)
        {
            string xml = GetTestXml(100);
            using (XmlTextReader reader = new XmlTextReader(new StringReader(xml)))
            {
                reader.ReadToDescendant("EmptyElement");

                // 反射私有類別
                var implInfo = typeof(XmlTextReader).GetField("impl", BindingFlags.Instance | BindingFlags.NonPublic);
                var impl = implInfo.GetValue(reader);
                var nodeInfo = impl.GetType().GetField("nodes", BindingFlags.Instance | BindingFlags.NonPublic);

                var node = (object[])nodeInfo.GetValue(impl);

                var stringValueInfo = node[0].GetType().GetProperty("StringValue", BindingFlags.Instance | BindingFlags.NonPublic);
                var localName = node[0].GetType().GetField("localName", BindingFlags.Instance | BindingFlags.NonPublic);
                var getMethod = stringValueInfo.GetGetMethod(true);
                


                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < iterations; i++)
                {
                   var s= node.Where(x => x != null).Select(x => (
                   localName.GetValue(x),
                   stringValueInfo.GetValue(x)
                   //getMethod.Invoke(x,null)
                   )).ToList();
                }


                stopwatch.Stop();
                Console.WriteLine($"TestXmlReaderReflection test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
        }

        public static void TestXmlReaderExtractAll(int iterations)
        {
            string xml = GetTestXml(100);
            using (XmlTextReader reader = new XmlTextReader(new StringReader(xml)))
            {
                reader.ReadToDescendant("EmptyElement");
                var count = reader.AttributeCount;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var names = reader.NameTable;
                for (int i = 0; i < iterations; i++)
                {
                    new MyReader(reader);
                }


                stopwatch.Stop();
                Console.WriteLine($"TestXmlReaderReflection test completed in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
        }

        public class MyReader : XmlReader
        {
            private Dictionary<string, string> _attrsMapping = new Dictionary<string, string>();
            private string _innerText = string.Empty;
            public MyReader(XmlReader xmlReader)
            {
                var totalAttritubeCount = xmlReader.AttributeCount;
                var dic = new Dictionary<string, string>(totalAttritubeCount);

                for (var j = 0; j < totalAttritubeCount; j++)
                {
                    xmlReader.MoveToAttribute(j);
                    dic.Add(xmlReader.Name, xmlReader.Value);
                }

                _innerText = xmlReader.ReadInnerXml();
                NodeType = xmlReader.NodeType;
            }

            public override XmlNodeType NodeType { get; }

            public override string LocalName => throw new NotImplementedException();

            public override string NamespaceURI => throw new NotImplementedException();

            public override string Prefix => throw new NotImplementedException();

            public override string Value => throw new NotImplementedException();

            public override int Depth => throw new NotImplementedException();

            public override string BaseURI => throw new NotImplementedException();

            public override bool IsEmptyElement => throw new NotImplementedException();

            public override int AttributeCount => throw new NotImplementedException();

            public override bool EOF => throw new NotImplementedException();

            public override ReadState ReadState => throw new NotImplementedException();

            public override XmlNameTable NameTable => throw new NotImplementedException();

            public override string GetAttribute(string name)
            {
                return _attrsMapping.TryGetValue(name, out var attr) ? attr: null;
            }

            public override string ReadInnerXml()
            {
                return _innerText;
            }

            public override string GetAttribute(string name, string namespaceURI)
            {
                throw new NotImplementedException();
            }

            public override string GetAttribute(int i)
            {
                throw new NotImplementedException();
            }

            public override string LookupNamespace(string prefix)
            {
                throw new NotImplementedException();
            }

            public override bool MoveToAttribute(string name)
            {
                throw new NotImplementedException();
            }

            public override bool MoveToAttribute(string name, string ns)
            {
                throw new NotImplementedException();
            }

            public override bool MoveToElement()
            {
                throw new NotImplementedException();
            }

            public override bool MoveToFirstAttribute()
            {
                throw new NotImplementedException();
            }

            public override bool MoveToNextAttribute()
            {
                throw new NotImplementedException();
            }

            public override bool Read()
            {
                throw new NotImplementedException();
            }

            public override bool ReadAttributeValue()
            {
                throw new NotImplementedException();
            }

            public override void ResolveEntity()
            {
                throw new NotImplementedException();
            }
        }
    }
}
