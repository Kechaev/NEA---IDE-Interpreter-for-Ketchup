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
        private List<object> listOfValues;

        private DataType type;
        private bool declared;
        private bool isList;
        
        public Variable(string name, List<object> listOfValues, bool isList)
        {
            ID = counter++;
            this.name = name;
            if (isList)
            {
                this.listOfValues = listOfValues;
                type = DataType.LIST;
                declared = false;
                this.isList = true;
            }
            else
            {
                if (listOfValues != null && listOfValues.Count > 0)
                {
                    value = listOfValues[0];
                }
                else
                {
                    value = null;
                }
                type = IdentifyBestDataType();
                declared = false;
                this.isList = false;
            }
        }

        // Do we need a specific data type declaration?
        public Variable(string name, object value, DataType type)
        {
            ID = counter++;
            this.name = name;
            this.value = value;
            this.type = type;
            declared = false;
        }

        public static void ResetVariables()
        {
            counter = 0;
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

        public DataType GetDataType()
        {
            return type;
        }

        public void SetDataType(DataType type)
        {
            this.type = type;
        }

        public void SetValue(object value)
        {
            this.value = value;
        }

        public void CreateNewList()
        {
            listOfValues = new List<object>();
        }

        public void Add(object value)
        {
            listOfValues.Add(value);
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
            if (isList)
            {
                throw new Exception("LOGIC ERROR: Failed to access general value for a list");
            }
            return value;
        }

        public object GetValueFromIndex(int index)
        {
            if (!isList && !(IdentifyBestDataType() == DataType.STRING))
            {
                throw new Exception("LOGIC ERROR: Failed to index a non-list variable");
            }
            if (index < 0)
            {
                throw new Exception($"LOGIC ERROR: Index out of bound. Tried to index {index + 1}. The index must be a positive number greater or equal to 1.");
            }
            if (index >= listOfValues.Count)
            {
                throw new Exception($"LOGIC ERROR: Index out of bound. Tried to index {index + 1}. The list's size is {listOfValues.Count}, you cannot index past this.");
            }
            return listOfValues[index];
        }

        public List<object> GetValuesList()
        {
            return listOfValues;
        }

        public void Declare()
        {
            declared = true;
        }

        public bool IsDeclared()
        {
            return declared;
        }

        public void SetNull()
        {
            value = null;
            listOfValues = null;
        }

        public bool IsNull()
        {
            return value == null && listOfValues == null;
        }
    }
}
