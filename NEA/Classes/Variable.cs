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
            if (value.ToString().ToUpper() == "TRUE" || value.ToString().ToUpper() == "FALSE")
            { 
                return DataType.BOOLEAN;
            }
            else
            {
                try
                {
                    double numericalValue = Convert.ToDouble(value);
                    if (numericalValue == (int)numericalValue)
                    {
                        return DataType.INTEGER;
                    }
                    else
                    {
                        return DataType.DECIMAL;
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
            type = IdentifyBestDataType();
            Console.WriteLine($"Type = {type}");
        }

        public void SetListValues(List<object> listOfValues)
        {
            this.listOfValues = listOfValues;
            type = DataType.LIST;
            Console.WriteLine(type);
        }

        public void CreateNewList()
        {
            listOfValues = new List<object>();
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

        #region List Methods
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
            if (index > listOfValues.Count)
            {
                throw new Exception($"LOGIC ERROR: Index out of bound. Tried to index {index + 1}. The list's size is {listOfValues.Count}, you cannot index past this.");
            }
            // 1-Based Indexing
            return listOfValues[index - 1];
        }

        public void SetValueFromIndex(int index, object value)
        {
            if (!isList && !(IdentifyBestDataType() == DataType.STRING))
            {
                throw new Exception("LOGIC ERROR: Failed to index a non-list variable");
            }
            if (index < 0)
            {
                throw new Exception($"LOGIC ERROR: Index out of bound. Tried to index {index + 1}. The index must be a positive number greater or equal to 1.");
            }
            if (index > listOfValues.Count)
            {
                throw new Exception($"LOGIC ERROR: Index out of bound. Tried to index {index + 1}. The list's size is {listOfValues.Count}, you cannot index past this.");
            }
            listOfValues[index - 1] = value;
        }

        public List<object> GetValuesList()
        {
            return listOfValues;
        }

        public void Add(object value)
        {
            listOfValues.Add(value);
        }

        public void Remove(object value)
        {
            bool found = false;
            int toRemove = -1;
            do
            {
                toRemove++;
                if (listOfValues[toRemove].ToString() == value.ToString())
                {
                    found = true;
                }
            }
            while (toRemove < listOfValues.Count - 1 && !found);
            
            if (found)
            {
                listOfValues.RemoveAt(toRemove);
            }
        }

        public void MakeList()
        {
            isList = true;
            type = DataType.LIST;
        }

        public int GetLength()
        {
            if (type == DataType.LIST)
            {
                return listOfValues.Count;
            }
            if (type == DataType.STRING || type == DataType.CHARACTER)
            {
                return value.ToString().Length;
            }
            if (type == DataType.INTEGER)
            {
                // Returns number of digits in the integer
                return (int)Math.Log10((double)value);
            }
            if (type == DataType.DECIMAL)
            {
                int counter = 0;
                double decimalValue = (double)value;
                while (decimalValue != (int)decimalValue)
                {
                    decimalValue *= 10;
                    counter++;
                }
                return (int)Math.Log10(decimalValue) + 1;
            }
            throw new Exception($"LOGIC ERROR: Cannot get length from {name} of type {type}.");
        }
        #endregion

        #region Declaration
        public void Declare()
        {
            declared = true;
        }

        public bool IsDeclared()
        {
            return declared;
        }
        #endregion

        #region Nullification
        public void SetNull()
        {
            value = null;
            listOfValues = null;
        }

        public bool IsNull()
        {
            return value == null && listOfValues == null;
        }
        #endregion
    }
}
