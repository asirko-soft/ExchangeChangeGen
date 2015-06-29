using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeChangeGenerator
{
    class HelperMethods
    {
        public static String separateNameAndDomain(string stringToSeparate, NameParts partToSeparate)
        {
            var separatorIndex = stringToSeparate.IndexOf('@');
            switch (partToSeparate)
            {
                case NameParts.name:
                    return stringToSeparate.Remove(separatorIndex);
                case NameParts.domain:
                    return stringToSeparate.Remove(0, separatorIndex + 1);
                default:
                    return null;
            }
        }

        public enum NameParts
        {
            name,
            domain
        }
    }
}
