using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Thrift.Protocol;
using Thrift.Transport;

namespace Test 
{
    class Program
    {
         
        static string host = "127.0.0.1";
        static int port =  9090;
        static string tablename = "table1";
        static string table2name = "table2";

        static TTransport transport = new TSocket(host, port);
        //实例化一个协议对象
        static TProtocol tProtocol = new TBinaryProtocol(transport);
        //实例化一个Hbase的Client对象
        static Hbase.Client client = new Hbase.Client(tProtocol);


         

        /// <summary>
        /// 读取单行
        /// </summary>
        public static void getattachmentSingle()
        {
            //     TTransport transport = null;
            try
            {
                tablename = table2name;
                //打开连接
                transport.Open();
                string rowkey = "k1";
                //根据表名，RowKey名来获取结果集
                List<TRowResult> reslut = client.getRow(Encoding.UTF8.GetBytes(tablename), Encoding.UTF8.GetBytes(rowkey), null);
                //遍历结果集
                foreach (var key in reslut)
                {
                    Console.WriteLine("RowKey:\n{0}", Encoding.UTF8.GetString(key.Row));
                    //打印Qualifier和对应的Value
                    foreach (var k in key.Columns)
                    {
                        Console.WriteLine("******************" + "\n" + Encoding.UTF8.GetString(k.Key));
                        Console.WriteLine("Value:" + Encoding.UTF8.GetString(k.Value.Value));
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                if (null != transport)
                {
                    transport.Close();
                }
            }
        }




        /// <summary>
        /// 读取单行
        /// </summary>
        public static void getSingle()
        {
       //     TTransport transport = null;
            try
            {
 
                //打开连接
                transport.Open();
                string rowkey = "k2";
                //根据表名，RowKey名来获取结果集
                List<TRowResult> reslut = client.getRow(Encoding.UTF8.GetBytes(tablename), Encoding.UTF8.GetBytes(rowkey), null);
                //遍历结果集
                foreach (var key in reslut)
                {
                    Console.WriteLine("RowKey:\n{0}", Encoding.UTF8.GetString(key.Row));
                    //打印Qualifier和对应的Value
                    foreach (var k in key.Columns)
                    {
                        Console.WriteLine("******************" + "\n" + Encoding.UTF8.GetString(k.Key));
                        Console.WriteLine("Value:" + Encoding.UTF8.GetString(k.Value.Value));
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                if (null != transport)
                {
                    transport.Close();
                }
            }
        }


        public static void getPagerTest()
        {
            List<byte[]> _byte = new List<byte[]>();
//            _byte.Add(Encoding.UTF8.GetBytes("s:PatientName"));
//            _byte.Add(Encoding.UTF8.GetBytes("s:StudyInstanceUID"));
             _byte.Add(Encoding.UTF8.GetBytes("common:t_id"));
           
          string filterString = "SingleColumnValueFilter('common','t_id',=,'substring:T0')"; 
             GetDataFromHBaseThroughFilter(tablename, filterString, _byte);
            Console.ReadLine();
    
        }

        /// <summary>
        /// 通过Filter进行数据的Scanner
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filterString"></param>
        static void GetDataFromHBaseThroughFilter(string tablename, string filterString, List<byte[]> _cols)
        {
            TScan _scan = new TScan(); 
            _scan.FilterString = Encoding.UTF8.GetBytes(filterString);
            _scan.Columns = _cols;
            transport.Open();
            int ScannerID = client.scannerOpenWithScan(Encoding.UTF8.GetBytes(tablename), _scan, null);

            List<TRowResult> reslut = client.scannerGetList(ScannerID, 100);
            Console.WriteLine( reslut.Count) ;
                        foreach (var key in reslut)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(key.Row));
            
                            foreach (var k in key.Columns)
                            {
                                Console.Write(Encoding.UTF8.GetString(k.Key) + "\t");
                                Console.WriteLine(Encoding.UTF8.GetString(k.Value.Value));
                                Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
                            }
                        }
        }



        static void Main(string[] args)
        {

          //  getPagerTest();
            //   getSingle();
            //   getattachmentSingle();
              countSingleCarPage();
            Console.ReadLine();
        }


        /// <summary>
        /// 读取开始结束间行数
        /// </summary>
        public static void countSingleCarPage()
        {
           var cc = "";
            var start = DateTime.Now;
            int limit = 50;
            int CurrentPage = 60;
            int pageIndex = CurrentPage - 1;  
            String startRow = "T0001";
            String stopRow = "T0052";

            //   GetDataFromHBaseThroughRowKeyRange(tablename, startRow, stopRow);
            if (!transport.IsOpen)
            {
                transport.Open();
            }
            List<byte[]> _byte = new List<byte[]>();
           _byte.Add(Encoding.UTF8.GetBytes("common:t_id"));
            int ScannerID = client.scannerOpenWithStop(Encoding.UTF8.GetBytes(tablename),
                Encoding.UTF8.GetBytes(startRow), Encoding.UTF8.GetBytes(stopRow),
                _byte, null);
            //   var li=   client.scannerGet(ScannerID);
            List<TRowResult> reslut = client.scannerGetList(ScannerID, 1000000);
            //      reslut = client.scannerGetList(ScannerID, 2);
            //            reslut = client.scannerGetList(ScannerID, 2);
            //            reslut = client.scannerGetList(ScannerID, 2);
            client.scannerClose(ScannerID);
            var TotalCount = reslut.Count;
            Console.WriteLine(TotalCount);
       
            int _row = limit * pageIndex+ 1;
//            Dictionary<int,string> allStartAndEnd=new Dictionary<int, string>();
            List <KeyValuePair<int, string>> alList=new List<KeyValuePair<int, string>>();
            int dicCount = 1;
            int firstBegin = 0;
            for (int i = 0; i < TotalCount; i++)
            {
                if (firstBegin == i)
                {
                   // allStartAndEnd.Add(dicCount, Encoding.UTF8.GetString(reslut[i].Row));
                    alList.Add(new KeyValuePair<int, string>(dicCount, Encoding.UTF8.GetString(reslut[i].Row)));
                    dicCount++;
                    firstBegin += limit;
                } 
            }
             
//            var newPageResult = reslut.Skip(limit * pageIndex).Take(limit).ToList();
//            foreach (var key in newPageResult)
//            {
//                Console.WriteLine("行 "+_row);
//                Console.WriteLine(Encoding.UTF8.GetString(key.Row));
//                foreach (var k in key.Columns)
//                {
//                    Console.Write(Encoding.UTF8.GetString(k.Key) + "\t");
//                    Console.WriteLine(Encoding.UTF8.GetString(k.Value.Value));
//                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
//                }
//
//                _row++;
//            }

            if (transport.IsOpen)
            {
                transport.Close();
            }

            var end1 = DateTime.Now;
            var t = "计算行数共消耗 " + (end1 - start).TotalSeconds + " 秒 ";
            Console.WriteLine(t) ;
            Console.WriteLine("行数" + reslut.Count);

            if(alList.Count>= CurrentPage) { 
            var  beginRow = alList[CurrentPage-1].Value;
            var endRow = alList[CurrentPage].Value;
            getRealPage(beginRow, endRow, limit, CurrentPage); 
            }
        }


        public static void getRealPage(String startRow, String stopRow,int rowCount,int CurrentPage)
        {
            if (!transport.IsOpen)
            {
                transport.Open();
            }
            List<byte[]> _byte = new List<byte[]>(); 
            int ScannerID = client.scannerOpenWithStop(Encoding.UTF8.GetBytes(tablename),
                Encoding.UTF8.GetBytes(startRow), Encoding.UTF8.GetBytes(stopRow),
                _byte, null);
            List<TRowResult> reslut = client.scannerGetList(ScannerID, rowCount);
            client.scannerClose(ScannerID);
            Console.WriteLine(reslut.Count);
            int _row = rowCount*( CurrentPage-1) + 1;
            foreach (var key in reslut)
            {
                Console.WriteLine("行 " + _row);
                Console.WriteLine(Encoding.UTF8.GetString(key.Row));
                foreach (var k in key.Columns)
                {
                    Console.Write(Encoding.UTF8.GetString(k.Key) + "\t");
                    Console.WriteLine(Encoding.UTF8.GetString(k.Value.Value));
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
                }
                _row++;
            }

            if (transport.IsOpen)
            {
                transport.Close();
            }
        }


    }
}
