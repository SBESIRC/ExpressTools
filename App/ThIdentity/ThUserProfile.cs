using System;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace ThIdentity
{
    public class ThUserProfile
    {
        private PropertyCollection properties;

        public string Name
        {
            get
            {
                return properties["name"][0].ToString();
            }
        }

        public string Title
        {
            get
            {
                return properties["title"][0].ToString();
            }
        }

        public string Company
        {
            get
            {
                return properties["company"][0].ToString();
            }
        }

        public string Department
        {
            get
            {
                return properties["department"][0].ToString();
            }
        }

        public string Mail
        {
            get
            {
                return properties["mail"][0].ToString();
            }
        }

        public string Accountname
        {
            get
            {
                return properties["sAMAccountName"][0].ToString();
            }
        }

        public bool IsDomainUser()
        {
            return (properties != null);
        }

        private string AccountName()
        {
            // Machines current user is as AD account name
            return Environment.UserName;
        }

        public ThUserProfile()
        {
            properties = Collectionproperty(null, null);
        }

        public ThUserProfile(string name)
        {
            properties = Collectionproperty(name, null);
        }

        public ThUserProfile(string name, string domain)
        {
            properties = Collectionproperty(name, domain);
        }

        private PropertyCollection Collectionproperty(string name, string domain)
        {
            try
            {
                //get domain
                if (domain == null)
                {
                    IPGlobalProperties ipglobalproperties = IPGlobalProperties.GetIPGlobalProperties();
                    domain = ipglobalproperties.DomainName;
                }

                //split domain
                string LDAPString = string.Empty;
                string[] domainComponents = domain.Split('.');
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < domainComponents.Length; i++)
                {
                    builder.AppendFormat(",dc={0}", domainComponents[i]);
                }
                if (builder.Length > 0)
                    LDAPString = builder.ToString(1, builder.Length - 1);

                //create AD
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + LDAPString);
                if (entry.NativeObject == null) { }//这里只是让entry.NativeObject抛出异常用catch捕捉，而不需要进行操作
                DirectorySearcher searcher = new DirectorySearcher(entry)
                {
                    Filter = "sAMAccountName=" + (name ?? AccountName())
                };

                //find
                SearchResult result = searcher.FindOne();
                return result?.GetDirectoryEntry().Properties;
            }
            catch
            {
                return null;
            }
        }
    }

    public class ThUserProfileEx
    {
        public string Name;
        public string Title;
        public string Company;
        public string Department;
        public string Mail;
        public string Accountname;

        //oracle 相关信息
        private const string oracleinformation = "User Id=NC63_AI;" +
            "Password=NC63_AI;" +
            "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=172.16.0.8)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME= thapeorcl)))";
        private OracleConnection connection;

        public ThUserProfileEx()
        {
            try
            {
                var userprofilefromlocal = new ThUserProfile();
                connection = Connection(oracleinformation);
                //查询
                Select("select * from v_person_onjob where EMAIL = '" + userprofilefromlocal.Mail + "'");
            }
            catch
            {
                Name = null;
                Title = null;
                Company = null;
                Department = null;
                Mail = null;
                Accountname = null;
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        public OracleConnection Connection(string oracleStr)
        {
            try
            {
                connection = new OracleConnection(oracleinformation);
                connection.Open();
                return connection;
            }
            catch
            {
                return connection;
            }
        }

        public void Select(string sql)
        {
            try
            {
                OracleCommand cmd = new OracleCommand(sql, connection);
                using (var datareader = cmd.ExecuteReader())
                {
                    while (datareader.Read())
                    {
                        Accountname = datareader["PNCODE"].ToString();
                        Name = datareader["NAME"].ToString();
                        Mail = datareader["EMAIL"].ToString();
                        Company = datareader["CORPNAME"].ToString();
                        Department = datareader["DEPTNAME"].ToString();
                        Title = datareader["POSTNAME"].ToString();
                    }
                }
            }
            catch
            {
                Name = null;
                Title = null;
                Company = null;
                Department = null;
                Mail = null;
                Accountname = null;
            }
        }

        public bool IsDomainUser()
        {
            return (Mail != null);
        }
    }
}
