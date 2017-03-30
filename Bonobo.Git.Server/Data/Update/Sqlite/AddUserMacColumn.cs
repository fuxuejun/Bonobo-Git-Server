using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update.Sqlite
{
    public class AddUserMacColumn : IUpdateScript
    {
        public string Command
        {
            get
            {
                return "ALTER TABLE User ADD COLUMN [Mac] VARCHAR(255)";
            }
        }

        public string Precondition
        {
            get
            {
                return "SELECT Count([Mac]) = -1 FROM User";
            }
        }

        public void CodeAction(BonoboGitServerContext context) { }
    }
}