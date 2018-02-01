using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Windows.SySServices.Common
{
    public class IdentityHelper
    {
        //  summary
        /// 判断程序是否是以管理员身份运行。
        ///  /summary
        public static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }//不是以管理员身份开启，则自动以管理员身份重新打开程序
    }
}
