using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;

namespace Dynamic.Net.Command
{
    public abstract class CommandBase : INetCommand
    {
        public abstract String Name { get;  }

        private Encoding encoding = Encoding.UTF8;
        public Encoding Encoding
        {
            get { return encoding ?? Encoding.UTF8; }
            set { encoding = value; }
        }

        public Byte[] CommandName
        {
            get
            {
                return Encoding.GetBytes(Name);
            }
        }

        private List<Byte[]> parameters = null;
        public IEnumerable<Byte[]> Parameters {
            get
            {
                return parameters;
            }
        }



        #region INetCommand 成员

        public virtual bool IsMatch(byte[] commandName)
        {
            char[] sourceName = Name.ToCharArray();
            if (sourceName.Length == commandName.Length)
            {
                bool isMatch = true;
                for (int i = 0; i < sourceName.Length; i++)
                {
                    if (sourceName[i] != commandName[i])
                    {
                        isMatch = false;
                        break;
                    }
                }
                return isMatch;
            }
            else
            {
                return false;
            }
        }

        protected abstract bool ParameterCheck(int idx );

        protected virtual void ProccessParameter(byte[] parValue, int idx)
        {
        }

        public bool SetParameter(byte[] parValue, int idx)
        {
            if (!ParameterCheck(idx))
            {
                return false;
            }
            if (idx == 0)
            {
                parameters = new List<byte[]>();
            }
            parameters.Add(parValue);

            ProccessParameter(parValue, idx);

            return true;
        }

        public abstract INetCommand Execute(INetSession session);


        #endregion
    }
}
