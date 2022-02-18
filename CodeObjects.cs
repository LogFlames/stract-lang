using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stract_lang
{
    public class StractCodeObject
    {
        public string NAME;

        public int tokenIndexStart;
        public int tokenIndexEnd;

        public bool isType = false;
        public bool isStructTemplate = false;
        public bool isAssignment = false;
        public bool isScope = false;
        public bool isStruct = false;
        public bool isFunction = false;

        public StractCodeObject()
        {
            NAME = "CodeObject";
        }

        public StractCodeObject(int tokenIndexStart, int tokenIndexEnd)
        {
            NAME = "CodeObject"; 

            this.tokenIndexStart = tokenIndexStart;
            this.tokenIndexEnd = tokenIndexEnd;
        }
    }

    public class StractAssignment : StractCodeObject
    {
        public string identifier;
        public StractCodeObject codeObject;

        public StractAssignment(string identifier)
        {
            isAssignment = true;
            NAME = "Assignment";

            this.identifier = identifier;
        }
    }

    public class StractType : StractCodeObject
    {
        public StractType()
        {
            NAME = "Type";
            isType = true;
        }
    }

    public class StractStructTemplate : StractType
    {
        public Dictionary<string, StractType> namedValueTypes;

        public StractStructTemplate()
        {
            isStructTemplate = true;
            NAME = "StructTemplate";
            namedValueTypes = new Dictionary<string, StractType>();
        }
    }

    public class StractPrimitiveType : StractType
    {
        public string primitiveTypeName;

        public StractPrimitiveType(string primitiveTypeName)
        {
            NAME = "PrimitiveType";
            this.primitiveTypeName = primitiveTypeName;
        }
    }

    public class StractScope : StractCodeObject
    {
        public List<StractCodeObject> codeObjects;

        public StractScope()
        {
            isScope = true;
            NAME = "Scope";
            codeObjects = new List<StractCodeObject>();
        }
    }

    public class StractStruct : StractCodeObject
    {
        public StractStructTemplate structTemplate;

        public StractStruct()
        {
            isStruct = true;
            NAME = "Struct";
        }
    }

    public class StractFunction : StractCodeObject
    {
        public StractStructTemplate strucTemplate;
        public StractScope scope;

        public StractFunction()
        {
            isFunction = true;
            NAME = "Function";
        }
    }

    public class StractExpressionDivider : StractCodeObject
    {
        public StractExpressionDivider(int tokenIndexStart, int tokenIndexEnd) : base(tokenIndexStart, tokenIndexEnd)
        {
            NAME = "ExpressionDivider";
        }

        public StractExpressionDivider() : base()
        {
            NAME = "ExpressionDivider";
        }
    }
}
