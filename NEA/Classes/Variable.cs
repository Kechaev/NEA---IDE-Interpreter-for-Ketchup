using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NEA.Classes
{
    class Variable
    {
        private static int counter;
        private int ID;
        private string name;
        private object value;
        private DataType type;
        private bool declared;
        
        public Variable(string name, object value)
        {
            ID = counter++;
            this.name = name;
            this.value = value;
            type = IdentifyBestDataType();
            declared = false;
        }

        public Variable(string name, object value, DataType type)
        {
            ID = counter++;
            this.name = name;
            this.value = value;
            this.type = type;
            declared = false;
        }

        private DataType IdentifyBestDataType()
        {
            try
            {
                Convert.ToBoolean(value);
                return DataType.BOOLEAN;
            }
            catch
            {
                try
                {
                    Convert.ToInt32(value);
                    try
                    {
                        Convert.ToDouble(value);
                        return DataType.DECIMAL;
                    }
                    catch
                    {
                        return DataType.INTEGER;
                    }
                }
                catch
                {
                    try
                    {
                        Convert.ToChar(value);
                        return DataType.CHARACTER;
                    }
                    catch
                    {
                        return DataType.STRING;
                    }
                }
            }
        }

        public void SetDataType(DataType type)
        {
            this.type = type;
        }

        public void SetValue(object value)
        {
            this.value = value;
        }

        public int GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return name;
        }
            
        public object GetValue()
        {
            return value;
        }

        public void Declare()
        {
            declared = true;
        }

        public bool IsDeclared()
        {
            return declared;
        }
    }
}
