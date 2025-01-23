using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA.Classes
{
    class StackFrame
    {
        private Variable[] localVariables;
        private int returnAddress;
        private Variable[] parameters;
        private bool isFunction;

        private string[] intermediate;

        public StackFrame(Variable[] parameters, Variable[] localVariables, int returnAddress, bool isFunction, string[] intermediate)
        {
            this.parameters = parameters;
            this.localVariables = localVariables;
            this.returnAddress = returnAddress;
            this.isFunction = isFunction;
            this.intermediate = intermediate;
        }

        public Variable[] GetLocalVariables()
        {
            return localVariables;
        }

        public Variable[] GetParameters()
        {
            return parameters;
        }

        public int GetReturnAddress()
        {
            return returnAddress;
        }

        public bool IsFunction()
        {
            return isFunction;
        }

        public string[] GetIntermediate()
        {
            return intermediate;
        }
    }
}
