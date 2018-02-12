using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.IO;

namespace oil_3s
{
    class Program
    {
        static void Main(string[] args)
        {
            // 執行檔同目錄之setup.ini
            string iniFN = System.Environment.CurrentDirectory + "\\setup.ini";
            // 取得firebird連接信息
            string dbServer = GetKeyValueString(iniFN, "shift", "db_server");
            string dbName = GetKeyValueString(iniFN, "shift", "db_name");
            string dbUser = GetKeyValueString(iniFN, "shift", "db_user");
            string dbPass = GetKeyValueString(iniFN, "shift", "db_pass");
            string dbPort = GetKeyValueString(iniFN, "shift", "db_port");
            // firebird shift database connection string
            string fbCS =
               "User=" + dbUser +";" +
               "Password=" + dbPass + ";" +
               "Database=" + dbName + ";" +
               "DataSource=" + dbServer + ";" +
               "Port=" + dbPort + ";";
            FbConnection fbConn = new FbConnection(fbCS);
            // 取得mysql連接信息
            string MdbServer = GetKeyValueString(iniFN, "oil", "db_server");
            string MdbName = GetKeyValueString(iniFN, "oil", "db_name");
            string MdbUser = GetKeyValueString(iniFN, "oil", "db_user");
            string MdbPass = GetKeyValueString(iniFN, "oil", "db_pass");
            string MdbPort = GetKeyValueString(iniFN, "oil", "db_port");
            // mysql oil database connection string
            string mysqlCS =
               "User=" + MdbUser + ";" +
               "Password=" + MdbPass + ";" +
               "Database=" + MdbName + ";" +
               "DataSource=" + MdbServer + ";" +
               "Port=" + MdbPort + ";";
            MySqlConnection mysqlConn = new MySqlConnection(mysqlCS);

            fbConn.Open();
            string fbsql = "SELECT * FROM CAR_DATA";
            FbCommand fbCMD = new FbCommand(fbsql,fbConn);
            //fbCMD.Connection = fbConn;
            //fbCMD.CommandText = fbsql;
            FbDataAdapter fbADP = new FbDataAdapter();
            fbADP.SelectCommand = fbCMD;
            DataSet fbDS = new DataSet();
            fbADP.Fill(fbDS);
            fbConn.Close();
            DataTable fbDT = new DataTable();
            fbDT = fbDS.Tables[0];
            mysqlConn.Open();
            for (int i  = 0; i < fbDT.Rows.Count; i++)
            {
                string sfCAR_NO = fbDT.Rows[i]["CAR_NO"].ToString();
                string sfCUST_ID = fbDT.Rows[i]["CUST_ID"].ToString();
                string sfUNIT_ID = fbDT.Rows[i]["UNIT_ID"].ToString();
                string sfPRODUCT_ID = fbDT.Rows[i]["PRODUCT_ID"].ToString();
               
                String mysql = "SELECT * FROM CAR_DATA where car_no='" +
                    sfCAR_NO + "' and cust_id='" + sfCUST_ID +"'";
                MySqlCommand mysqlCMD = new MySqlCommand(mysql,mysqlConn);
                //mysqlCMD.Connection = mysqlConn;
                //mysqlCMD.CommandText = mysql;
                MySqlDataAdapter mysqlADP = new MySqlDataAdapter();
                mysqlADP.SelectCommand = mysqlCMD;
                DataSet mysqlDS = new DataSet();
                mysqlADP.Fill(mysqlDS);
                
                DataTable mysqlDT = new DataTable();
               
                if (mysqlDS.Tables[0].Rows.Count == 0)
                {
                    string strInsert = "insert  into CAR_DATA (CAR_NO,CUST_ID,UNIT_ID,PRODUCT_ID) VALUES ('" +
                        sfCAR_NO+ "','" + sfCUST_ID + "','" + sfUNIT_ID + "','" + sfPRODUCT_ID + "')";
                    //mysqlCMD.Connection = mysqlConn;
                    mysqlCMD.CommandText = strInsert;
                    mysqlCMD.ExecuteNonQuery();
                }
                else
                {
                    string strUpdate = "update CAR_DATA set UNIT_ID='" + sfUNIT_ID +
                        "',PRODUCT_ID='" + sfPRODUCT_ID + "' where car_no='" + sfCAR_NO + "' and cust_id='" + sfCUST_ID + "'";
                    //mysqlCMD.Connection = mysqlConn;
                    mysqlCMD.CommandText = strUpdate;
                    mysqlCMD.ExecuteNonQuery();
                }

            }
            mysqlConn.Close(); 
        }
       
        private static string GetKeyValueString(string fileName, string Section, string Key)
        {
            StringBuilder value = new StringBuilder(255);
            bool hasSection = false;

            //開啟IO串流
            StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
            while (true)
            {
                string s = sr.ReadLine();

                //空值或空字串判斷
                if (s == null || s == "")
                {
                    continue;
                }

                //以;或是#開頭作註解的判斷
                if (Regex.Match(s, @"^(;|#).*$").Success)
                {
                    continue;
                }

                //讀取[Section]
                if (Regex.Match(s, @"^\[.*\]").Success)
                {
                    //判斷Section名稱是否符合
                    if (System.Text.RegularExpressions.Regex.Match(s, Section).Success)
                    {
                        hasSection = true;
                    }
                }

                //如果Section存在，才去判斷Key
                if (hasSection)
                {
                    string[] KeyValue = s.Split('=');

                    //判斷Key名稱是否符合
                    if (Regex.Match(KeyValue[0].Trim(), Key).Success)
                    {
                        value.Append(KeyValue[1].Trim());
                        break;
                    }
                }
            }

            //關閉IO串流
            sr.Close();

            return value.ToString();
        }
    }
}
