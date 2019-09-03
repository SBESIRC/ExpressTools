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

    public class ThUserProfileex
    {
        public string Name;
        public string Title;
        public string Company;
        public string Department;
        public string Mail;
        public string Accountname;

        //oracle 相关信息
        private const string oracleinformation = "";
        private OracleConnection connection;

        public ThUserProfileex()
        {
            connection = Connection(oracleinformation);
            //查询
            Select("");
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public OracleConnection Connection(string oracleStr)
        {
            OracleConnection connection = null;
            try
            {
                connection = new OracleConnection(oracleinformation);
                connection.Open();
                return connection;
            }
            catch
            {
                return null;
            }
        }

        public void Select(string sql)
        {
            try
            {
                OracleCommand cmd = new OracleCommand(sql, connection);
                using (var dataReader = cmd.ExecuteReader())
                {
                    Name = dataReader.GetValue(1).ToString();
                    Title = dataReader.GetValue(2).ToString();
                    Company = dataReader.GetValue(3).ToString();
                    Department = dataReader.GetValue(4).ToString();
                    Mail = dataReader.GetValue(5).ToString();
                    Accountname = dataReader.GetValue(6).ToString();
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
            finally
            {
                connection.Close();
            }
        }

        public bool IsDomainUser()
        {
            return (Mail != null);
        }
    }
}
