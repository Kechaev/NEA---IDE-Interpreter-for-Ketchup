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

        // Creates a Stack Frame
        public StackFrame(Variable[] parameters, Variable[] localVariables, int returnAddress, bool isFunction, string[] intermediate)
        {
            this.parameters = parameters;
            this.localVariables = localVariables;
            this.returnAddress = returnAddress;
            this.isFunction = isFunction;
            this.intermediate = intermediate;
        }

        // Returns all the local variables
        public Variable[] GetLocalVariables()
        {
            return localVariables;
        }

        // Returns all the parameters
        public Variable[] GetParameters()
        {
            return parameters;
        }

        // Returns the return address
        public int GetReturnAddress()
        {
            return returnAddress;
        }

        // Returns if the subroutine the stack frame represents is a function
        public bool IsFunction()
        {
            return isFunction;
        }

        // Returns the intermediate code associated with the frame
        public string[] GetIntermediate()
        {
            return intermediate;
        }
    }
}
