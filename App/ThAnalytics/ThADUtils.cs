using System;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;

namespace ThAnalytics
{
    public class ThADUtils
    {
        public static PropertyCollection UserInDomain(string username, string domain)
        {
            string LDAPString = string.Empty;
            string[] domainComponents = domain.Split('.');
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < domainComponents.Length; i++)
            {
                builder.AppendFormat(",dc={0}", domainComponents[i]);
            }
            if (builder.Length > 0)
                LDAPString = builder.ToString(1, builder.Length - 1);

            DirectoryEntry entry = new DirectoryEntry("LDAP://" + LDAPString);

            DirectorySearcher searcher = new DirectorySearcher(entry)
            {
                Filter = "sAMAccountName=" + username
            };

            SearchResult result = searcher.FindOne();
            return result?.GetDirectoryEntry()?.Properties;
        }

        public static string CurrentUser()
        {
            try
            {
                // set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

                // find current user
                UserPrincipal user = UserPrincipal.Current;

                // return account name;
                return user?.SamAccountName;
            }
            catch
            {
                return string.Empty;
            }

        }

        public static string CurrentDomain()
        {
            try
            {
                return Domain.GetCurrentDomain().Name;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
